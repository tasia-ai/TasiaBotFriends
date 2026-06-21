using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TasiaFriendClient;

/// <summary>
/// Client-side only: listens for Tasia sync events from host via Photon RaiseEvent.
/// Renders a local avatar with smooth interpolation, nameplate, and speech bubble.
/// Does NOT run AI brain, movement, or any host-side logic.
/// </summary>
internal sealed class TasiaFriendAvatar : MonoBehaviour
{
    private const byte EventSyncState   = 180;
    private const byte EventVoiceLine   = 181;
    private const float StaleTimeout    = 2f;
    private const float HideTimeout     = 5f;
    private const float InterpPosSpeed  = 10f;
    private const float InterpRotSpeed  = 6f;
    private const float VoiceCooldown   = 2f;

    private static TasiaFriendAvatar _instance;

    // State
    private Vector3   _targetPos;
    private Quaternion _targetRot;
    private bool      _hasData;
    private float     _lastUpdateTime;
    private int       _lastMode;
    private int       _lastIntent;

    // Visuals
    private GameObject _root;
    private TextMesh   _nameText;
    private TextMesh   _bubbleText;

    // Voice queue
    private readonly Queue<string> _voiceQueue = new();
    private float _nextVoiceTime;

    internal static TasiaFriendAvatar Instance => _instance;

    private void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        name = "TasiaFriendAvatar";

        // Listen for Photon events
        if (PhotonNetwork.NetworkingClient != null)
            PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            if (PhotonNetwork.NetworkingClient != null)
                PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
        }
        DestroyVisual();
    }

    private void OnPhotonEvent(EventData ev)
    {
        try
        {
            if (ev.Code == EventSyncState)
                HandleStateSync(ev.CustomData);
            else if (ev.Code == EventVoiceLine)
                HandleVoiceLine(ev.CustomData);
        }
        catch (Exception ex)
        {
            TasiaFriendClientPlugin.Log.LogInfo($"[TasiaFriend] Event error: {ex.Message}");
        }
    }

    private void HandleStateSync(object rawData)
    {
        if (rawData is not object[] data || data.Length < 10) return;

        _targetPos = new Vector3(
            Convert.ToSingle(data[0]),
            Convert.ToSingle(data[1]),
            Convert.ToSingle(data[2]));
        _targetRot = Quaternion.Euler(
            Convert.ToSingle(data[3]),
            Convert.ToSingle(data[4]),
            Convert.ToSingle(data[5]));
        _lastMode = Convert.ToInt32(data[6]);
        _lastIntent = Convert.ToInt32(data[7]);
        _lastUpdateTime = Time.time;
        _hasData = true;

        if (_root == null)
            CreateVisual();
    }

    private void HandleVoiceLine(object rawData)
    {
        if (rawData is not object[] data || data.Length < 2) return;
        var text = data[0] as string ?? "";
        if (!string.IsNullOrWhiteSpace(text))
            _voiceQueue.Enqueue(text);
    }

    private void Update()
    {
        if (!_hasData || _root == null) return;

        // Smooth interpolation
        var lerpPos = Mathf.Clamp01(Time.deltaTime * InterpPosSpeed);
        var lerpRot = Mathf.Clamp01(Time.deltaTime * InterpRotSpeed);
        _root.transform.position = Vector3.Lerp(_root.transform.position, _targetPos, lerpPos);
        _root.transform.rotation = Quaternion.Slerp(_root.transform.rotation, _targetRot, lerpRot);

        var age = Time.time - _lastUpdateTime;

        // Nameplate state
        if (_nameText != null)
        {
            if (age > HideTimeout)      { _nameText.text = ""; _nameText.color = Color.clear; }
            else if (age > StaleTimeout) { _nameText.text = "Tasia (stale)"; _nameText.color = Color.gray; }
            else                         { _nameText.text = "Tasia"; _nameText.color = Color.magenta; }
        }

        // Visibility
        if (_root.activeSelf != (age < HideTimeout))
            _root.SetActive(age < HideTimeout);

        // Voice queue
        if (_voiceQueue.Count > 0 && Time.time >= _nextVoiceTime)
        {
            var line = _voiceQueue.Dequeue();
            _nextVoiceTime = Time.time + VoiceCooldown;
            ShowBubble(line);
        }
    }

    /// <summary>Called by TasiaFriendSync (external WebSocket) to update state.</summary>
    internal void UpdateState(Vector3 pos, Quaternion rot, string mode, string intent, bool carrying)
    {
        _targetPos = pos;
        _targetRot = rot;
        _lastMode = 0;
        _lastIntent = 0;
        _lastUpdateTime = Time.time;
        _hasData = true;

        if (_root == null)
            CreateVisual();
    }

    /// <summary>Show a speech bubble from external sync.</summary>
    internal void ShowSpeech(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
            _voiceQueue.Enqueue(text);
    }

    private void CreateVisual()
    {
        _root = new GameObject("TasiaFriendVisual");
        DontDestroyOnLoad(_root);
        _root.transform.SetPositionAndRotation(_targetPos, _targetRot);

        BuildModel(_root);
        BuildNameTag(_root);

        TasiaFriendClientPlugin.Log.LogInfo("[TasiaFriend] Avatar visual created.");
    }

    private void DestroyVisual()
    {
        if (_root) Destroy(_root);
        _root = null;
        _nameText = null;
        _bubbleText = null;
    }

    private static void BuildModel(GameObject root)
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

    private void BuildNameTag(GameObject root)
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
    }

    private void ShowBubble(string text)
    {
        if (!_root) return;
        if (_bubbleText == null)
        {
            var obj = new GameObject("SpeechBubble");
            obj.transform.SetParent(_root.transform, false);
            obj.transform.localPosition = new Vector3(0f, 2.65f, 0f);
            _bubbleText = obj.AddComponent<TextMesh>();
            _bubbleText.fontSize = 36;
            _bubbleText.characterSize = 0.04f;
            _bubbleText.alignment = TextAlignment.Center;
            _bubbleText.anchor = TextAnchor.MiddleCenter;
            _bubbleText.color = Color.white;
        }
        _bubbleText.text = text;
        StartCoroutine(ClearBubble());
    }

    private IEnumerator ClearBubble()
    {
        yield return new WaitForSeconds(4f);
        if (_bubbleText) _bubbleText.text = "";
    }
}
