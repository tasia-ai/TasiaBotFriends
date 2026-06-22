using UnityEngine;

namespace TasiaBotFriends;

internal sealed class TasiaOverlayNew : MonoBehaviour
{
    internal static TasiaOverlayNew Instance;
    private bool _show;
    private Rect _btnRect = new Rect(Screen.width - 90, 12, 80, 28);
    private Rect _winRect = new Rect(60, 60, 380, 400);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        name = "TasiaOverlayNew";
    }

    private void OnGUI()
    {
        if (GUI.Button(_btnRect, "Tasia")) _show = !_show;
        if (!_show) return;
        _winRect = GUI.Window(999, _winRect, WinFn, "Tasia Control");
    }

    private void WinFn(int id)
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Spawn (F8)")) TasiaBotFriendsPlugin.Instance?.ManualSpawn("GUI");
        if (GUILayout.Button("Despawn (F9)")) { var i = TasiaBotFriendsPlugin.Instance; if (i != null) { i.RemoveAllBots(); } }
        GUILayout.Label($"Bots: {TasiaBotFriendsPlugin.Instance?.GetBotList().Count ?? 0}");
        GUILayout.EndVertical();
        GUI.DragWindow();
    }
}
