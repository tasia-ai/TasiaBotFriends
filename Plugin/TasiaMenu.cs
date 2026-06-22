using UnityEngine;

namespace TasiaBotFriends;

internal sealed class TasiaMenu : MonoBehaviour
{
    internal static TasiaMenu Instance;
    private bool _show, _attached;
    private float _nextTry;
    private Rect _btnRect = new Rect(Screen.width - 90, 12, 80, 28);
    private Rect _winRect = new Rect(60, 60, 380, 400);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (!_attached && Time.time >= _nextTry)
        {
            _nextTry = Time.time + 3f;
            TryAttach();
        }
    }

    private void TryAttach()
    {
        Transform player = null;
        try { if (PlayerController.instance) player = PlayerController.instance.transform; } catch { }
        if (player != null)
        {
            transform.SetParent(player, false);
            _attached = true;
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMenu] Attached to player.");
        }
    }

    private void OnGUI()
    {
        if (!_show)
        {
            if (GUI.Button(_btnRect, "Tasia")) _show = true;
            return;
        }
        _winRect = GUI.Window(999, _winRect, WinFn, "Tasia");
    }

    private void WinFn(int id)
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Spawn (F8)", GUILayout.Height(30))) TasiaBotFriendsPlugin.Instance?.ManualSpawn("GUI");
        if (GUILayout.Button("Despawn (F9)", GUILayout.Height(30))) { var i = TasiaBotFriendsPlugin.Instance; if (i != null) i.RemoveAllBots(); }
        if (GUILayout.Button("Close", GUILayout.Height(22))) _show = false;
        GUILayout.EndVertical();
        GUI.DragWindow();
    }
}
