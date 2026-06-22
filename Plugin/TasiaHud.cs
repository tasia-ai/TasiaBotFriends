using UnityEngine;
using Object = UnityEngine.Object;

namespace TasiaBotFriends;

/// <summary>
/// Simple world-space HUD using TextMesh above the player's view.
/// Alternative to OnGUI which doesn't work in REPO's lobby.
/// </summary>
internal sealed class TasiaHud : MonoBehaviour
{
    private static TasiaHud _instance;
    private TextMesh _text;
    private GameObject _hudObj;
    private float _nextUpdate;
    private bool _posInit;
    private string _lastMessage = "";

    internal static void Show(string msg)
    {
        if (_instance == null) return;
        _instance._lastMessage = msg;
    }

    private void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        name = "TasiaHud";

        _hudObj = new GameObject("TasiaHudText");
        _hudObj.transform.SetParent(transform, false);
        // Will follow camera via Update
        _hudObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        _text = _hudObj.AddComponent<TextMesh>();
        _text.fontSize = 24;
        _text.characterSize = 0.03f;
        _text.alignment = TextAlignment.Left;
        _text.anchor = TextAnchor.UpperLeft;
        _text.color = Color.magenta;
    }

    private void Update()
    {
        if (Time.time < _nextUpdate || _text == null) return;
        _nextUpdate = Time.time + 0.5f;

        var msg = "";
        var bot = GetBot();
        var brain = bot?.GetComponent<TasiaBotBrain>();
        var carrier = bot?.GetComponent<TasiaBotCarrier>();
        if (brain != null)
            msg = $"Tasia | {brain.CurrentState} | {brain.CurrentIntent} | Carry:{carrier?.IsCarrying}";
        else
            msg = _lastMessage;

        _text.text = msg;
    }

    private static GameObject GetBot()
    {
        var inst = TasiaBotFriendsPlugin.Instance;
        return inst?.GetBotList().Count > 0 ? inst.GetBotList()[0] : null;
    }
}
