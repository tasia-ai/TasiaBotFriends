using System; using System.Reflection; using BepInEx; using BepInEx.Logging; using HarmonyLib; using UnityEngine;
namespace TasiaNetwork;
public sealed class TasiaNetworkPlugin
{
    internal static ManualLogSource Log;
    private static HarmonyLib.Harmony _harmony;

    public static void LateLoad()
    {
        Log = BepInEx.Logging.Logger.CreateLogSource("TasiaNetwork");
        _harmony = new HarmonyLib.Harmony("Tasia.Network");
        _harmony.PatchAll();
        Log.LogInfo("[TasiaNetwork] v1.0.0 loaded via TasiaLoader.");
    }
}

// ── Harmony patch: adds relay to each player ──
[HarmonyPatch(typeof(PlayerAvatar), "Awake")]
internal static class PlayerAvatarHook
{
    static void Postfix(PlayerAvatar __instance)
    {
        if (__instance.GetComponent<TasiaNetRelay>() == null)
            __instance.gameObject.AddComponent<TasiaNetRelay>();
    }
}

// ── Relay: zero direct Photon refs, all via reflection ──
internal sealed class TasiaNetRelay : MonoBehaviour
{
    private bool _ready, _registered;
    private float _nextSend, _nextCheck;
    private object _tasiaPlugin;
    private Type _provType;
    private const byte StateEvent = 200;
    
    // Photon types (resolved via reflection)
    private Type _photonNetType, _raiseOptType, _sendOptType;
    private object _othersReceivers, _unreliableSend;

    private void Awake()
    {
        _photonNetType = GetType("Photon.Pun.PhotonNetwork, PhotonUnityNetworking");
        if (_photonNetType == null) { enabled = false; return; }
    }

    private void Update()
    {
        // Remote client always runs this
        TasiaRemote.Tick();

        if (!_ready) { if (Time.time >= _nextCheck) { _nextCheck = Time.time + 2f; TryReady(); } return; }
        if (Time.time >= _nextSend)
        {
            _nextSend = Time.time + 0.25f;
            if (_tasiaPlugin != null) SendState();
        }
    }

    private void TryReady()
    {
        if (_tasiaPlugin == null)
        {
            _provType = GetType("TasiaBotFriends.ITasiaNetworkStateProvider, TasiaBotFriends");
            var pluginType = GetType("TasiaBotFriends.TasiaBotFriendsPlugin, TasiaBotFriends");
            if (_provType == null || pluginType == null) return;
            _tasiaPlugin = pluginType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (_tasiaPlugin == null) return;
        }

        try
        {
            if (!(bool)_provType.GetProperty("HasActiveTasia").GetValue(_tasiaPlugin)) return;
            
            // Register event listener only when ready (deferred from Awake)
            if (!_registered) RegisterListener();
            
            _ready = true;
            TasiaNetworkPlugin.Log.LogInfo("[TasiaNet] Ready, sending state.");
        }
        catch { }
    }

    private void RegisterListener()
    {
        try
        {
            var clientProp = _photonNetType.GetProperty("NetworkingClient", BindingFlags.Static | BindingFlags.Public);
            if (clientProp == null) return;
            var client = clientProp.GetValue(null);
            if (client == null) return;
            var evType = client.GetType().GetEvent("EventReceived", BindingFlags.Instance | BindingFlags.Public);
            if (evType == null) return;
            var handler = Delegate.CreateDelegate(evType.EventHandlerType, this, GetType().GetMethod("OnEvent", BindingFlags.Instance | BindingFlags.NonPublic));
            evType.AddEventHandler(client, handler);
            _registered = true;
            TasiaNetworkPlugin.Log.LogInfo("[TasiaNet] Event listener registered.");
        }
        catch (Exception ex) { TasiaNetworkPlugin.Log.LogInfo($"[TasiaNet] Register failed: {ex.Message}"); }
    }

    private void OnEvent(object ev)
    {
        try
        {
            var code = (byte)ev.GetType().GetProperty("Code").GetValue(ev);
            if (code != StateEvent) return;
            var data = ev.GetType().GetProperty("CustomData").GetValue(ev) as object[];
            if (data == null || data.Length < 5) return;
            TasiaRemote.HandleState(
                new Vector3(ToF(data[0]), ToF(data[1]), ToF(data[2])),
                ToF(data[3]),
                data[4] as string ?? ""
            );
        }
        catch { }
    }

    private void SendState()
    {
        try
        {
            var state = _provType.GetMethod("GetVisibleState").Invoke(_tasiaPlugin, null);
            var p = (Vector3)state.GetType().GetField("Position").GetValue(state);
            var r = (float)state.GetType().GetField("RotationY").GetValue(state);
            var s = (string)state.GetType().GetField("SpeechText").GetValue(state) ?? "";

            // Resolve RaiseEventOptions and SendOptions lazily
            if (_othersReceivers == null)
            {
                var reType = GetType("Photon.Realtime.RaiseEventOptions, PhotonRealtime");
                if (reType == null) return;
                var opts = Activator.CreateInstance(reType);
                var recEnum = GetType("Photon.Realtime.ReceiverGroup, PhotonRealtime");
                if (recEnum == null) return;
                var others = Enum.Parse(recEnum, "Others");
                reType.GetField("Receivers")?.SetValue(opts, others);
                _othersReceivers = opts;

                var soType = GetType("Photon.Realtime.SendOptions, PhotonRealtime");
                if (soType != null)
                {
                    var sendEnum = GetType("Photon.Realtime.SendOptions+SendUnity, PhotonRealtime");
                    if (sendEnum != null)
                        _unreliableSend = Enum.Parse(sendEnum, "SendUnreliable");
                }
            }

            if (_othersReceivers == null) return;

            var content = new object[] { p.x, p.y, p.z, r, s };
            var raiseEvent = _photonNetType.GetMethod("RaiseEvent", BindingFlags.Static | BindingFlags.Public,
                null, new[] { typeof(byte), typeof(object), typeof(object), typeof(object) }, null);
            if (raiseEvent != null)
                raiseEvent.Invoke(null, new object[] { StateEvent, content, _othersReceivers, _unreliableSend ?? 0 });
        }
        catch { }
    }

    private static float ToF(object v) => v is float f ? f : 0f;
    private static Type GetType(string s) { try { return Type.GetType(s); } catch { return null; } }
}// ── Remote visual clone (client-side) ──
internal static class TasiaRemote
{
    private static GameObject _marker;
    private static TextMesh _label;
    private static Vector3 _target, _smooth;
    private static float _smoothY, _targetY;
    private static float _lastUpdate;
    private static bool _hasData;

    internal static void HandleState(Vector3 pos, float rotY, string speech)
    {
        _target = pos; _targetY = rotY; _lastUpdate = Time.time; _hasData = true;
        if (_marker == null) Create();
        if (!string.IsNullOrEmpty(speech) && _label != null) _label.text = $"Tasia: {speech}";
    }

    internal static void Tick()
    {
        if (!_hasData || _marker == null) return;
        if (Time.time - _lastUpdate > 3f) { if (_marker.activeSelf) _marker.SetActive(false); return; }
        if (!_marker.activeSelf) _marker.SetActive(true);
        _smooth = Vector3.Lerp(_smooth, _target, Time.deltaTime * 10f);
        _smoothY = Mathf.LerpAngle(_smoothY, _targetY, Time.deltaTime * 6f);
        _marker.transform.SetPositionAndRotation(_smooth, Quaternion.Euler(0, _smoothY, 0));
        if (_label && Time.time - _lastUpdate > 2f) _label.text = "Tasia (remote)";
        else if (_label && string.IsNullOrEmpty(_label.text)) _label.text = "Tasia";
    }

    private static void Create()
    {
        _marker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        _marker.name = "TasiaRemote"; UnityEngine.Object.DontDestroyOnLoad(_marker);
        var r = _marker.GetComponent<Renderer>(); if (r) r.material.color = Color.magenta;
        UnityEngine.Object.Destroy(_marker.GetComponent<Collider>());
        var tag = new GameObject("Tag"); tag.transform.SetParent(_marker.transform, false);
        tag.transform.localPosition = new Vector3(0, 1.5f, 0);
        _label = tag.AddComponent<TextMesh>(); _label.text = "Tasia";
        _label.fontSize = 36; _label.characterSize = 0.05f; _label.alignment = TextAlignment.Center;
        _label.anchor = TextAnchor.MiddleCenter; _label.color = Color.magenta;
        _smooth = _target;
    }
}
