using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TasiaFriendClient;

/// <summary>
/// Friend-side external WebSocket sync receiver.
/// Connects to Tasia Sync Server as a client, receives Tasia state
/// and feeds it to TasiaFriendAvatar for rendering.
/// </summary>
internal sealed class TasiaFriendSync : MonoBehaviour
{
    private const int ProtocolVersion = 1;

    private ClientWebSocket _ws;
    private CancellationTokenSource _cts = new();
    private bool _connected;
    private bool _reconnecting;
    private string _serverUrl;
    private string _roomId;
    private string _roomToken;
    private float _lastStateTime;

    internal bool Connected => _connected;
    internal float LastStateAge => _connected ? Time.time - _lastStateTime : float.MaxValue;

    internal void Configure(string url, string roomId, string token)
    {
        _serverUrl = url;
        _roomId = roomId;
        _roomToken = token;
        Connect();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _ws?.Dispose();
    }

    private async void Connect()
    {
        if (_reconnecting || string.IsNullOrWhiteSpace(_serverUrl)) return;
        _reconnecting = true;

        try
        {
            _ws?.Dispose();
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);

            var uri = new UriBuilder(_serverUrl)
            {
                Query = $"room={Uri.EscapeDataString(_roomId)}&role=client&protocol={ProtocolVersion}"
            };
            if (!string.IsNullOrWhiteSpace(_roomToken))
                uri.Query += $"&token={Uri.EscapeDataString(_roomToken)}";

            TasiaFriendClientPlugin.Log.LogInfo($"[TasiaFriendSync] Connecting...");
            await _ws.ConnectAsync(uri.Uri, _cts?.Token ?? CancellationToken.None);
            _connected = true;
            _reconnecting = false;
            _lastStateTime = Time.time;

            TasiaFriendClientPlugin.Log.LogInfo("[TasiaFriendSync] Connected to sync server.");

            // Ensure we have a TasiaFriendAvatar to render
            if (TasiaFriendAvatar.Instance == null)
            {
                var go = new GameObject("TasiaFriendClientAvatar");
                DontDestroyOnLoad(go);
                go.AddComponent<TasiaFriendAvatar>();
            }

            StartReceiveLoop();
        }
        catch (Exception ex)
        {
            TasiaFriendClientPlugin.Log.LogInfo($"[TasiaFriendSync] Connection failed: {ex.Message}");
            _connected = false;
            _reconnecting = false;
            StartCoroutine(ReconnectOnce());
        }
    }

    private System.Collections.IEnumerator ReconnectOnce()
    {
        yield return new WaitForSeconds(5f);
        Connect();
    }

    private async void StartReceiveLoop()
    {
        var buffer = new byte[8192];
        var msgBuffer = new StringBuilder();

        try
        {
            while (_ws.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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
        TasiaFriendClientPlugin.Log.LogInfo("[TasiaFriendSync] Disconnected.");
        StartCoroutine(ReconnectOnce());
    }

    private void HandleMessage(string raw)
    {
        try
        {
            var msg = JObject.Parse(raw);
            var type = msg["type"]?.ToString() ?? "";

            switch (type)
            {
                case "STATE_UPDATE":
                    _lastStateTime = Time.time;
                    var tasia = msg["tasia"];
                    if (tasia != null && TasiaFriendAvatar.Instance != null)
                    {
                        var pos = tasia["position"];
                        var rotY = tasia["rotationY"]?.Value<float>() ?? 0f;
                        var active = tasia["active"]?.Value<bool>() ?? false;
                        if (!active) return;

                        var position = new Vector3(
                            pos["x"]?.Value<float>() ?? 0f,
                            pos["y"]?.Value<float>() ?? 0f,
                            pos["z"]?.Value<float>() ?? 0f
                        );

                        var mode = tasia["mode"]?.ToString() ?? "IDLE";
                        var intent = tasia["intent"]?.ToString() ?? "NONE";
                        var carrying = tasia["carrying"]?.Value<bool>() ?? false;

                        TasiaFriendAvatar.Instance.UpdateState(position, Quaternion.Euler(0, rotY, 0), mode, intent, carrying);
                    }
                    break;

                case "VOICE_LINE":
                    var text = msg["text"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(text) && TasiaFriendAvatar.Instance != null)
                    {
                        TasiaFriendAvatar.Instance.ShowSpeech(text);
                    }
                    break;

                case "HOST_LEFT":
                    _lastStateTime = 0;
                    TasiaFriendClientPlugin.Log.LogInfo("[TasiaFriendSync] Host disconnected.");
                    break;
            }
        }
        catch { }
    }
}
