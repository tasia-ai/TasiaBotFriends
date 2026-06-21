using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TasiaBotFriends;

/// <summary>
/// Photon event codes for Tasia sync.
/// </summary>
internal static class TasiaNetEvents
{
    public const byte StateSync   = 180;
    public const byte VoiceLine   = 181;
    public const byte VersionInfo = 182;

    public static readonly RaiseEventOptions Others = new()
    {
        Receivers = ReceiverGroup.Others,
        CachingOption = EventCaching.DoNotCache,
    };

    public static readonly RaiseEventOptions All = new()
    {
        Receivers = ReceiverGroup.All,
        CachingOption = EventCaching.DoNotCache,
    };

    public static readonly SendOptions Reliable = SendOptions.SendReliable;
    public static readonly SendOptions Unreliable = SendOptions.SendUnreliable;
}

/// <summary>
/// Host-side: broadcasts Tasia state to other modded clients via RaiseEvent.
/// Attached to Tasia's GameObject on the host.
/// </summary>
internal sealed class TasiaNetworkHost : MonoBehaviour
{
    private Transform _tform;
    private TasiaBotBrain _brain;
    private TasiaBotCarrier _carrier;
    private float _nextSyncTime;
    private float _syncInterval;
    private bool _registered;

    private void Start()
    {
        _tform = transform;
        _brain = GetComponent<TasiaBotBrain>();
        _carrier = GetComponent<TasiaBotCarrier>();
        _syncInterval = 1f / Mathf.Clamp(TasiaBotFriendsPlugin.Instance?.SyncRateHz ?? 10f, 1f, 30f);
        _registered = true;
        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaNet] Host sync starting.");
    }

    private void OnDestroy()
    {
        _registered = false;
    }

    private void Update()
    {
        // No sync outside gameplay
        if (!TasiaBotFriendsPlugin.IsGameplayReady()) return;
        if (!_registered || !PhotonNetwork.IsConnected || Time.time < _nextSyncTime) return;
        _nextSyncTime = Time.time + _syncInterval;
        if (!_tform) return;

        var pos = _tform.position;
        var rot = _tform.rotation.eulerAngles;
        var mode = _brain != null ? (int)_brain.CurrentMode : 0;
        var intent = _brain != null ? (int)_brain.CurrentIntent : 0;
        var carry = _carrier != null && _carrier.IsCarrying;

        var content = new object[] { pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, mode, intent, carry, Time.time };
        PhotonNetwork.RaiseEvent(TasiaNetEvents.StateSync, content, TasiaNetEvents.Others, TasiaNetEvents.Unreliable);
    }

    /// <summary>Broadcast a voice line to all clients.</summary>
    internal void BroadcastVoice(string text)
    {
        if (!_registered || !PhotonNetwork.IsConnected) return;
        var content = new object[] { text, Time.time };
        PhotonNetwork.RaiseEvent(TasiaNetEvents.VoiceLine, content, TasiaNetEvents.All, TasiaNetEvents.Reliable);
    }
}

/// <summary>
/// Client-side: listens for Tasia sync events and renders a local avatar.
/// Created as a persistent GameObject on plugin init on each client.
/// </summary>
internal sealed class TasiaRemoteAvatar : MonoBehaviour
{
    private static TasiaRemoteAvatar _instance;

    // Interpolation state
    private Vector3 _targetPos;
    private Quaternion _targetRot;
    private int _targetMode;
    private int _targetIntent;
    private bool _targetCarry;
    private bool _hasData;
    private float _lastUpdateTime;

    // Avatar visuals
    private GameObject _avatarRoot;
    private Transform _nameTag;
    private TextMesh _nameText;

    // Voice
    private readonly Queue<string> _voiceQueue = new();
    private float _nextVoiceTime;

    internal static TasiaRemoteAvatar Instance => _instance;

    private void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        name = "TasiaRemoteAvatar";

        // Register Photon event listener
        if (PhotonNetwork.NetworkingClient != null)
            PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;

        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaNet] Remote avatar ready.");
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
        if (PhotonNetwork.NetworkingClient != null)
            PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
        DestroyAvatar();
    }

    private void OnPhotonEvent(EventData ev)
    {
        try
        {
            switch (ev.Code)
            {
                case TasiaNetEvents.StateSync:
                    HandleStateSync(ev);
                    break;
                case TasiaNetEvents.VoiceLine:
                    HandleVoiceLine(ev);
                    break;
            }
        }
        catch (Exception ex)
        {
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaNet] Event error: {ex.Message}");
        }
    }

    private void HandleStateSync(EventData ev)
    {
        if (ev.CustomData is not object[] data || data.Length < 10) return;

        var pos = new Vector3(
            Convert.ToSingle(data[0]),
            Convert.ToSingle(data[1]),
            Convert.ToSingle(data[2]));
        var rot = Quaternion.Euler(
            Convert.ToSingle(data[3]),
            Convert.ToSingle(data[4]),
            Convert.ToSingle(data[5]));
        var mode = Convert.ToInt32(data[6]);
        var intent = Convert.ToInt32(data[7]);
        var carry = Convert.ToBoolean(data[8]);

        _targetPos = pos;
        _targetRot = rot;
        _targetMode = mode;
        _targetIntent = intent;
        _targetCarry = carry;
        _lastUpdateTime = Time.time;
        _hasData = true;

        if (_avatarRoot == null)
            CreateAvatar();
    }

    private void HandleVoiceLine(EventData ev)
    {
        if (ev.CustomData is not object[] data || data.Length < 2) return;
        var text = data[0] as string ?? "";
        if (!string.IsNullOrWhiteSpace(text))
            _voiceQueue.Enqueue(text);
    }

    private void Update()
    {
        if (!_hasData || _avatarRoot == null) return;

        // Smooth interpolation
        var lerpPos = Mathf.Clamp01(Time.deltaTime * 10f);
        var lerpRot = Mathf.Clamp01(Time.deltaTime * 6f);
        _avatarRoot.transform.position = Vector3.Lerp(_avatarRoot.transform.position, _targetPos, lerpPos);
        _avatarRoot.transform.rotation = Quaternion.Slerp(_avatarRoot.transform.rotation, _targetRot, lerpRot);

        var age = Time.time - _lastUpdateTime;
        bool stale = age > 2f;
        bool hidden = age > 5f;

        if (_nameText)
        {
            _nameText.text = hidden ? "" : stale ? "Tasia (stale)" : "Tasia";
            _nameText.color = stale ? Color.gray : Color.magenta;
        }

        if (_avatarRoot.activeSelf != !hidden)
            _avatarRoot.SetActive(!hidden);

        // Process voice queue
        if (_voiceQueue.Count > 0 && Time.time >= _nextVoiceTime)
        {
            var line = _voiceQueue.Dequeue();
            _nextVoiceTime = Time.time + 2f;
            ShowBubble(line);
        }
    }

    private void CreateAvatar()
    {
        _avatarRoot = new GameObject("TasiaRemote");
        DontDestroyOnLoad(_avatarRoot);
        _avatarRoot.transform.SetPositionAndRotation(_targetPos, _targetRot);

        CreateVisual(_avatarRoot);
        CreateNameTag(_avatarRoot);

        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaNet] Remote avatar visual created.");
    }

    private void DestroyAvatar()
    {
        if (_avatarRoot) Destroy(_avatarRoot);
        _avatarRoot = null;
        _nameTag = null;
        _nameText = null;
    }

    private void CreateVisual(GameObject root)
    {
        var pink  = MakeMat(new Color(1f, 0.22f, 0.82f));
        var dPink = MakeMat(new Color(0.62f, 0.05f, 0.45f));
        var black = MakeMat(new Color(0.03f, 0.03f, 0.04f));
        var cyan  = MakeMat(new Color(0.2f, 1f, 1f));

        AddPrim(root, "Body",     PrimitiveType.Cube,     new Vector3(0f, 0.92f, 0f),     new Vector3(0.62f, 0.78f, 0.42f), pink);
        AddPrim(root, "Head",     PrimitiveType.Cube,     new Vector3(0f, 1.55f, 0f),     new Vector3(0.58f, 0.42f, 0.48f), pink);
        AddPrim(root, "Face",     PrimitiveType.Cube,     new Vector3(0f, 1.57f, 0.245f), new Vector3(0.42f, 0.22f, 0.025f), black);
        AddPrim(root, "EyeL",     PrimitiveType.Sphere,   new Vector3(-0.12f, 1.58f, 0.275f), new Vector3(0.07f, 0.07f, 0.02f), cyan);
        AddPrim(root, "EyeR",     PrimitiveType.Sphere,   new Vector3(0.12f, 1.58f, 0.275f), new Vector3(0.07f, 0.07f, 0.02f), cyan);
        AddPrim(root, "Antenna",  PrimitiveType.Cylinder, new Vector3(0f, 1.86f, 0f),     new Vector3(0.035f, 0.22f, 0.035f), dPink);
        AddPrim(root, "AntBall",  PrimitiveType.Sphere,   new Vector3(0f, 2.08f, 0f),     new Vector3(0.1f, 0.1f, 0.1f), cyan);
        AddPrim(root, "Skirt",    PrimitiveType.Cylinder, new Vector3(0f, 0.46f, 0f),     new Vector3(0.58f, 0.12f, 0.58f), dPink);
        AddPrim(root, "ArmL",     PrimitiveType.Capsule,  new Vector3(-0.48f, 1.0f, 0f),  new Vector3(0.13f, 0.38f, 0.13f), pink);
        AddPrim(root, "ArmR",     PrimitiveType.Capsule,  new Vector3(0.48f, 1.0f, 0f),   new Vector3(0.13f, 0.38f, 0.13f), pink);
        AddPrim(root, "LegL",     PrimitiveType.Capsule,  new Vector3(-0.18f, 0.23f, 0f), new Vector3(0.14f, 0.28f, 0.14f), pink);
        AddPrim(root, "LegR",     PrimitiveType.Capsule,  new Vector3(0.18f, 0.23f, 0f),  new Vector3(0.14f, 0.28f, 0.14f), pink);
    }

    private static Material MakeMat(Color c)
    {
        var s = Shader.Find("Standard") ?? Shader.Find("Diffuse");
        return new Material(s) { color = c };
    }

    private static GameObject AddPrim(GameObject root, string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat)
    {
        var o = GameObject.CreatePrimitive(type);
        o.name = name;
        o.transform.SetParent(root.transform, false);
        o.transform.localPosition = pos;
        o.transform.localScale = scale;
        if (o.TryGetComponent<Collider>(out var col)) col.enabled = false;
        if (o.TryGetComponent<Renderer>(out var r)) r.material = mat;
        return o;
    }

    private void CreateNameTag(GameObject root)
    {
        var tag = new GameObject("NameTag");
        tag.transform.SetParent(root.transform, false);
        tag.transform.localPosition = new Vector3(0f, 2.15f, 0f);
        _nameText = tag.AddComponent<TextMesh>();
        _nameText.text = "Tasia";
        _nameText.fontSize = 48;
        _nameText.characterSize = 0.06f;
        _nameText.alignment = TextAlignment.Center;
        _nameText.anchor = TextAnchor.MiddleCenter;
        _nameText.color = Color.magenta;
        _nameTag = tag.transform;
    }

    private void ShowBubble(string text)
    {
        if (!_avatarRoot) return;
        var bubble = _avatarRoot.transform.Find("SpeechBubble")?.GetComponent<TextMesh>();
        if (!bubble)
        {
            var obj = new GameObject("SpeechBubble");
            obj.transform.SetParent(_avatarRoot.transform, false);
            obj.transform.localPosition = new Vector3(0f, 2.65f, 0f);
            bubble = obj.AddComponent<TextMesh>();
            bubble.fontSize = 36;
            bubble.characterSize = 0.04f;
            bubble.alignment = TextAlignment.Center;
            bubble.anchor = TextAnchor.MiddleCenter;
            bubble.color = Color.white;
        }
        bubble.text = text;
        Instance.StartCoroutine(ClearBubble(bubble));
    }

    private static IEnumerator ClearBubble(TextMesh bubble)
    {
        yield return new WaitForSeconds(4f);
        if (bubble) bubble.text = "";
    }
}
