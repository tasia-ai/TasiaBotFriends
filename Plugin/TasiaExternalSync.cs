using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TasiaBotFriends;

/// <summary>
/// External WebSocket sync client for Tasia state relay.
/// Host publishes Tasia state, friends subscribe and render.
/// Singleton — access via static methods after creation.
/// </summary>
internal sealed class TasiaExternalSync : MonoBehaviour
{
    private const int ProtocolVersion = 1;
    private const string ModVersion = "1.2.0";

    // ── Singleton ──
    private static TasiaExternalSync _instance;
    private static bool _hasInstance;

    // ── Config (set before creating the component) ──
    internal static string ServerUrl = "";
    internal static string RoomId = "tasia-default";
    internal static string RoomToken = "";
    internal static string Role = "host";
    internal static float SendRateHz = 10f;
    internal static bool Enabled = false;

    // ── Static status access ──
    internal static bool Connected => _hasInstance && _instance._connected;
    internal static float LastStateAge => _hasInstance ? Time.time - _instance._lastStateTime : float.MaxValue;

    // ── Instance state ──
    private ClientWebSocket _ws;
    private CancellationTokenSource _cts;
    private float _nextSendTime;
    private bool _connected;
    private bool _reconnecting;
    private float _lastStateTime;
    private readonly ConcurrentQueue<string> _voiceQueue = new();

    private void Awake()
    {
        if (_hasInstance && _instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        _hasInstance = true;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!Enabled || string.IsNullOrWhiteSpace(ServerUrl)) return;
        _cts = new CancellationTokenSource();
        TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaSync] External sync enabled ({Role}) → {ServerUrl}");
        Connect();
    }

    private void OnDestroy()
    {
        if (_instance == this) { _hasInstance = false; _instance = null; }
        _cts?.Cancel();
        _ws?.Dispose();
    }

    // ── Static send methods ──
    internal static void SendState(Vector3 position, float rotationY, string mode, string intent,
        bool carrying, string danger, bool active)
    {
        _instance?.SendStateInternal(position, rotationY, mode, intent, carrying, danger, active);
    }

    internal static void SendVoice(string lineId, string text, Vector3 position)
    {
        _instance?.SendVoiceInternal(lineId, text, position);
    }

    // ── Instance methods ──
    private async void Connect()
    {
        if (_reconnecting) return;
        _reconnecting = true;

        try
        {
            _ws?.Dispose();
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);

            var uri = new UriBuilder(ServerUrl)
            {
                Query = $"room={Uri.EscapeDataString(RoomId)}&role={Uri.EscapeDataString(Role)}&protocol={ProtocolVersion}"
            };
            if (!string.IsNullOrWhiteSpace(RoomToken))
                uri.Query += $"&token={Uri.EscapeDataString(RoomToken)}";

            await _ws.ConnectAsync(uri.Uri, _cts.Token);
            _connected = true;
            _reconnecting = false;
            _lastStateTime = Time.time;

            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaSync] Connected as {Role}");
            StartReceiveLoop();
        }
        catch (Exception ex)
        {
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaSync] Connection failed: {ex.Message}");
            _connected = false;
            _reconnecting = false;
            if (Enabled)
                StartCoroutine(ReconnectLater());
        }
    }

    private System.Collections.IEnumerator ReconnectLater()
    {
        yield return new WaitForSeconds(5f);
        if (Enabled && !_connected)
            Connect();
    }

    private async void StartReceiveLoop()
    {
        var buffer = new byte[8192];
        var msgBuffer = new StringBuilder();

        try
        {
            while (_ws.State == WebSocketState.Open && !_cts.IsCancellationRequested)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                if (result.MessageType == WebSocketMessageType.Close) break;
                msgBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                if (result.EndOfMessage)
                {
                    HandleMessage(msgBuffer.ToString());
                    msgBuffer.Clear();
                }
            }
        }
        catch { }

        _connected = false;
        if (Enabled)
            StartCoroutine(ReconnectLater());
    }

    private void HandleMessage(string raw)
    {
        try
        {
            var msg = JObject.Parse(raw);
            var type = msg["type"]?.ToString() ?? "";
            switch (type)
            {
                case "STATE_UPDATE": _lastStateTime = Time.time; break;
                case "PONG": break;
                case "CLIENT_HELLO":
                case "HOST_HELLO":
                    TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaSync] Handshake OK");
                    break;
                case "ERROR":
                    TasiaBotFriendsPlugin.Log.LogWarning($"[TasiaSync] Server error: {msg["message"]}");
                    break;
            }
        }
        catch { }
    }

    private async void SendStateInternal(Vector3 position, float rotationY, string mode, string intent,
        bool carrying, string danger, bool active)
    {
        if (!_connected || _ws.State != WebSocketState.Open) return;
        if (Time.time < _nextSendTime) return;
        _nextSendTime = Time.time + (1f / SendRateHz);

        var state = new
        {
            type = "STATE_UPDATE",
            protocolVersion = ProtocolVersion,
            modVersion = ModVersion,
            roomId = RoomId,
            timestamp = Time.time,
            tasia = new
            {
                active,
                position = new { x = position.x, y = position.y, z = position.z },
                rotationY,
                mode, intent, carrying, danger,
                name = "Tasia", stale = false
            }
        };

        try
        {
            var raw = JsonConvert.SerializeObject(state);
            await _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(raw)), WebSocketMessageType.Text, true, _cts.Token);
        }
        catch { _connected = false; }
    }

    private async void SendVoiceInternal(string lineId, string text, Vector3 position)
    {
        if (!_connected || _ws.State != WebSocketState.Open) return;

        var voice = new
        {
            type = "VOICE_LINE",
            protocolVersion = ProtocolVersion,
            modVersion = ModVersion,
            roomId = RoomId,
            timestamp = Time.time,
            lineId, speaker = "Tasia", text, audioUrl = "",
            position = new { x = position.x, y = position.y, z = position.z }
        };

        try
        {
            var raw = JsonConvert.SerializeObject(voice);
            await _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(raw)), WebSocketMessageType.Text, true, _cts.Token);
        }
        catch { _connected = false; }
    }
}
