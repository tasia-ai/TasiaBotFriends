using BepInEx; using BepInEx.Logging; using HarmonyLib; using Photon.Pun; using UnityEngine;
namespace TasiaNetwork;

[BepInDependency("Tasia.BotFriends")]
[BepInPlugin("Tasia.Network", "TasiaNetwork", "1.0.0")]
public sealed class TasiaNetworkPlugin : BaseUnityPlugin
{
    internal static TasiaNetworkPlugin Instance;
    internal static ManualLogSource Log => Instance.Logger;
    private Harmony _harmony;

    private void Awake()
    {
        Instance = this;
        _harmony = new Harmony("Tasia.Network");
        _harmony.PatchAll(); // patches PlayerAvatar.Awake to add TasiaNetworkRelay
        Logger.LogInfo("[TasiaNetwork] v1.0.0 loaded.");
    }
}

/// <summary>Added to each player's avatar. Uses their PhotonView for RPC.</summary>
internal sealed class TasiaNetworkRelay : MonoBehaviour
{
    private PhotonView _pv;
    private float _nextSend;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        if (_pv == null) { enabled = false; return; }
        if (_pv.IsMine) TasiaNetworkPlugin.Log.LogInfo("[TasiaRelay] Host relay active.");
        else TasiaNetworkPlugin.Log.LogInfo("[TasiaRelay] Client relay active.");
    }

    private void Update()
    {
        // Host: send Tasia state
        if (_pv != null && _pv.IsMine && Time.time >= _nextSend)
        {
            _nextSend = Time.time + 0.25f; // 4 Hz
            SendTasiaState();
        }

        // Client: update remote marker
        TasiaRemote.UpdateMarker(TasiaRemote.UseSmooth);
    }

    private void SendTasiaState()
    {
        var tasiaPlugin = FindTasiaPlugin();
        if (tasiaPlugin == null) return;
        var getBots = tasiaPlugin.GetType().GetMethod("GetBotList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (getBots == null) return;
        var bots = getBots.Invoke(tasiaPlugin, null) as System.Collections.Generic.List<GameObject>;
        if (bots == null || bots.Count == 0) return;
        var bot = bots[0];
        if (bot == null) return;

        var pos = bot.transform.position;
        var rotY = bot.transform.eulerAngles.y;
        var bubble = bot.transform.Find("SpeechBubble")?.GetComponent<TextMesh>();
        var speech = bubble != null ? bubble.text : "";

        _pv.RPC("TasiaOnState", RpcTarget.Others, pos.x, pos.y, pos.z, rotY, "", "", false, speech ?? "");
    }

    private static object FindTasiaPlugin()
    {
        var t = System.Type.GetType("TasiaBotFriends.TasiaBotFriendsPlugin, TasiaBotFriends");
        if (t == null) return null;
        var inst = t.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?.GetValue(null);
        return inst;
    }

    [PunRPC]
    internal void TasiaOnState(float x, float y, float z, float rotY, string mode, string intent, bool carrying, string speech)
    {
        TasiaRemote.HandleState(new Vector3(x, y, z), rotY, speech);
    }
}

/// <summary>Patching PlayerAvatar.Awake to add TasiaNetworkRelay.</summary>
[HarmonyPatch(typeof(PlayerAvatar), "Awake")]
internal static class PlayerAvatarPatch
{
    static void Postfix(PlayerAvatar __instance)
    {
        if (__instance.GetComponent<TasiaNetworkRelay>() == null)
            __instance.gameObject.AddComponent<TasiaNetworkRelay>();
    }
}

// ── Remote marker (client-side) ──
internal static class TasiaRemote
{
    internal static bool UseSmooth = true;
    private static GameObject _marker;
    private static TextMesh _label;
    private static Vector3 _targetPos, _smoothPos;
    private static float _targetRotY, _smoothRotY;
    private static string _lastSpeech;
    private static float _lastUpdate;
    private static bool _hasData;

    internal static void HandleState(Vector3 pos, float rotY, string speech)
    {
        _targetPos = pos; _targetRotY = rotY;
        _lastUpdate = Time.time; _lastSpeech = speech; _hasData = true;
        if (_marker == null) Create();
    }

    internal static void UpdateMarker(bool smooth)
    {
        if (!_hasData || _marker == null) return;
        var age = Time.time - _lastUpdate;
        if (age > 3f) { if (_marker.activeSelf) _marker.SetActive(false); return; }
        if (!_marker.activeSelf) _marker.SetActive(true);

        if (smooth) {
            _smoothPos = Vector3.Lerp(_smoothPos, _targetPos, Time.deltaTime * 10f);
            _smoothRotY = Mathf.LerpAngle(_smoothRotY, _targetRotY, Time.deltaTime * 6f);
        } else { _smoothPos = _targetPos; _smoothRotY = _targetRotY; }
        _marker.transform.position = _smoothPos;
        _marker.transform.rotation = Quaternion.Euler(0, _smoothRotY, 0);

        if (_label && age < 2f && !string.IsNullOrEmpty(_lastSpeech))
            _label.text = $"Tasia: {_lastSpeech}";
        else if (_label)
            _label.text = age > 2f ? "Tasia (stale)" : "Tasia";
    }

    private static void Create()
    {
        _marker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        _marker.name = "TasiaRemote"; Object.DontDestroyOnLoad(_marker);
        var r = _marker.GetComponent<Renderer>(); if (r) r.material.color = Color.magenta;
        Object.Destroy(_marker.GetComponent<Collider>());
        var tag = new GameObject("Tag"); tag.transform.SetParent(_marker.transform, false);
        tag.transform.localPosition = new Vector3(0, 1.5f, 0);
        _label = tag.AddComponent<TextMesh>(); _label.text = "Tasia";
        _label.fontSize = 36; _label.characterSize = 0.05f; _label.alignment = TextAlignment.Center;
        _label.anchor = TextAnchor.MiddleCenter; _label.color = Color.magenta;
        _smoothPos = _targetPos; _smoothRotY = _targetRotY;
    }
}
