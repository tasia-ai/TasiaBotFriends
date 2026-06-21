using System.Collections.Generic;
using UnityEngine;

namespace TasiaBotFriends;

/// <summary>
/// Floating Tasia overlay menu. Shows a single button that opens a draggable
/// tabbed panel with status, modes, sync, voice, debug, config, and safety controls.
/// Private dev/debug UI only — not a public cheat menu.
/// </summary>
internal sealed class TasiaOverlay : MonoBehaviour
{
    // ── Singleton ──
    private static TasiaOverlay _instance;
    internal static TasiaOverlay Instance => _instance;

    // ── Window state ──
    private int _windowId = 999001;
    private Rect _buttonRect;
    private Rect _windowRect;
    private bool _showWindow;
    private int _activeTab;
    private Vector2 _scrollPos;
    private string[] _tabNames = { "Status", "Modes", "Sync", "Voice", "Debug", "Config", "Safety" };
    private bool _dragEnabled = true;
    private float _nextLogTime;

    // ── Config (read from plugin) ──
    private float _buttonX = 20f;
    private float _buttonY = 120f;
    private const float ButtonW = 80f;
    private const float ButtonH = 28f;
    private const float WindowW = 420f;
    private const float WindowH = 480f;

    private void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        name = "TasiaOverlay";
        _buttonRect = new Rect(_buttonX, _buttonY, ButtonW, ButtonH);
        _windowRect = new Rect(120, 80, WindowW, WindowH);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    /// <summary>Called from RuntimeOnGUI. Draws button + window if open.</summary>
    internal void DrawOverlay()
    {
        var cfg = TasiaBotFriendsPlugin.Instance;
        if (cfg == null) return;

        var isGameplay = TasiaRuntimeRunner.IsGameplayCached;

        // Lifecycle gating: hide in menu/lobby/loading unless explicitly enabled
        if (!isGameplay && !cfg.ShowOverlayInMenu)
        {
            if (_showWindow) _showWindow = false;
            return;
        }

        // Draw floating button
        var style = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.magenta },
            hover = { textColor = Color.white },
        };
        _buttonRect.position = new Vector2(cfg.OverlayX, cfg.OverlayY);

        if (GUI.Button(_buttonRect, "Tasia", style))
            _showWindow = !_showWindow;

        // Draw overlay window
        if (!_showWindow) return;
        _windowRect = GUI.Window(_windowId, _windowRect, DrawWindow, "Tasia Control Panel", GUI.skin.window);
    }

    private void DrawWindow(int id)
    {
        // Tab bar
        GUILayout.BeginHorizontal();
        for (int i = 0; i < _tabNames.Length; i++)
        {
            var wasActive = _activeTab == i;
            var clicked = GUILayout.Toggle(wasActive, _tabNames[i], GUI.skin.button, GUILayout.ExpandWidth(false));
            if (clicked && !wasActive) _activeTab = i;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        // Tab content
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true);

        switch (_activeTab)
        {
            case 0: DrawStatusTab(); break;
            case 1: DrawModesTab(); break;
            case 2: DrawSyncTab(); break;
            case 3: DrawVoiceTab(); break;
            case 4: DrawDebugTab(); break;
            case 5: DrawConfigTab(); break;
            case 6: DrawSafetyTab(); break;
        }

        GUILayout.EndScrollView();

        // Close button
        if (GUILayout.Button("Close (F10)", GUILayout.Height(22)))
            _showWindow = false;

        GUI.DragWindow(new Rect(0, 0, 10000, 30));
    }

    private string StatusValue(string label, string value, Color? color = null)
    {
        var c = color ?? Color.white;
        GUILayout.BeginHorizontal();
        GUILayout.Label(label + ": ", GUILayout.Width(140));
        var prev = GUI.color;
        GUI.color = c;
        GUILayout.Label(value);
        GUI.color = prev;
        GUILayout.EndHorizontal();
        return value;
    }

    private GameObject GetBot()
    {
        var inst = TasiaBotFriendsPlugin.Instance;
        if (inst == null) return null;
        var bots = inst.GetBotList();
        return bots.Count > 0 ? bots[0] : null;
    }

    // ══════════════════════════════════════════════════════
    //  STATUS TAB
    // ══════════════════════════════════════════════════════
    private void DrawStatusTab()
    {
        var bot = GetBot();
        var brain = bot ? bot.GetComponent<TasiaBotBrain>() : null;
        var carrier = bot ? bot.GetComponent<TasiaBotCarrier>() : null;
        var ready = TasiaRuntimeRunner.IsGameplayCached;

        StatusValue("Gameplay Ready", ready ? "YES" : "no", ready ? Color.green : Color.gray);
        StatusValue("Tasia State", ready ? (brain ? "active" : "no bot") : "passive",
            ready ? Color.green : Color.gray);
        StatusValue("Mode", brain?.CurrentMode.ToString() ?? "N/A");
        StatusValue("Intent", brain?.CurrentIntent.ToString() ?? "N/A");
        StatusValue("State", brain?.CurrentState.ToString() ?? "N/A");
        StatusValue("Carrying", carrier != null && carrier.IsCarrying ? $"YES ({carrier.CarriedItemName})" : "no",
            carrier != null && carrier.IsCarrying ? Color.yellow : Color.gray);
        StatusValue("Danger", brain?.Perception?.DangerLevel ?? "unknown",
            brain?.Perception?.DangerLevel == "high" ? Color.red : Color.green);
        StatusValue("Room", brain?.Perception?.CurrentRoom ?? "?");
        StatusValue("Marty Distance", brain?.Perception?.PlayerDistance != null
            ? $"{brain.Perception.PlayerDistance:F1}m" : "?");
        StatusValue("Extraction", brain?.Perception?.ExtractionPointFound == true ? "known" : "unknown");
        StatusValue("Memory Loot", brain?.WorldMemory.KnownLoot.Count.ToString() ?? "0");
    }

    // ══════════════════════════════════════════════════════
    //  MODES TAB
    // ══════════════════════════════════════════════════════
    private void DrawModesTab()
    {
        var bot = GetBot();
        var brain = bot ? bot.GetComponent<TasiaBotBrain>() : null;
        var ready = TasiaRuntimeRunner.IsGameplayCached;

        if (!ready)
        {
            GUILayout.Label("Tasia inactive: gameplay not ready.", GUI.skin.label);
            return;
        }

        if (brain == null)
        {
            GUILayout.Label("No Tasia bot spawned.", GUI.skin.label);
            if (GUILayout.Button("Spawn Tasia (F8)"))
                TasiaBotFriendsPlugin.Instance?.ManualSpawn("Overlay");
            return;
        }

        var current = brain.CurrentMode;
        GUILayout.Label($"Current mode: {current}", GUI.skin.label);
        GUILayout.Space(6);

        if (GUILayout.Button($"  FOLLOW  {(current == TasiaMode.FOLLOW ? "✓" : "")}", GUILayout.Height(30)))
            brain.SetTasiaMode(TasiaMode.FOLLOW);
        if (GUILayout.Button($"  COLLECT {(current == TasiaMode.COLLECT ? "✓" : "")}", GUILayout.Height(30)))
            brain.SetTasiaMode(TasiaMode.COLLECT);
        if (GUILayout.Button($"  FIGHT   {(current == TasiaMode.FIGHT ? "✓" : "")}", GUILayout.Height(30)))
            brain.SetTasiaMode(TasiaMode.FIGHT);
        if (GUILayout.Button($"  WAIT    {(current == TasiaMode.WAIT ? "✓" : "")}", GUILayout.Height(30)))
            brain.SetTasiaMode(TasiaMode.WAIT);

        GUILayout.Space(8);
        if (GUILayout.Button("Despawn Tasia (F9)", GUILayout.Height(26)))
        {
            TasiaBotFriendsPlugin.Instance?.RemoveAllBots();
        }
    }

    // ══════════════════════════════════════════════════════
    //  SYNC TAB
    // ══════════════════════════════════════════════════════
    private void DrawSyncTab()
    {
        var plugin = TasiaBotFriendsPlugin.Instance;

        StatusValue("External Sync", TasiaExternalSync.Enabled ? "enabled" : "disabled",
            TasiaExternalSync.Enabled ? Color.green : Color.gray);
        StatusValue("WebSocket", TasiaExternalSync.Connected ? "connected" : "disconnected",
            TasiaExternalSync.Connected ? Color.green : Color.red);
        StatusValue("Room ID", TasiaExternalSync.RoomId, Color.cyan);
        StatusValue("Sync Age", TasiaExternalSync.Connected
            ? $"{TasiaExternalSync.LastStateAge:F1}s" : "N/A");
        StatusValue("Protocol", "v1");

        GUILayout.Space(6);
        GUILayout.Label("Photon RaiseEvent", GUI.skin.label);
        StatusValue("Status", plugin?.MpVisible == true ? "enabled" : "disabled");
    }

    // ══════════════════════════════════════════════════════
    //  VOICE TAB
    // ══════════════════════════════════════════════════════
    private void DrawVoiceTab()
    {
        var plugin = TasiaBotFriendsPlugin.Instance;

        StatusValue("Voice Enabled", plugin?.VoiceOn == true ? "yes" : "no");
        StatusValue("Shared Voice", plugin?.VoiceSharedOn == true ? "yes" : "no");
        StatusValue("Speech Bubbles", plugin?.SpeechOn == true ? "yes" : "no");

        GUILayout.Space(4);
        GUILayout.Label("API keys are in config file only.", GUI.skin.label);
        GUILayout.Label("No keys shown here.", GUI.skin.label);
    }

    // ══════════════════════════════════════════════════════
    //  DEBUG TAB
    // ══════════════════════════════════════════════════════
    private void DrawDebugTab()
    {
        var bot = GetBot();
        var brain = bot ? bot.GetComponent<TasiaBotBrain>() : null;

        GUILayout.Label($"Intent: {brain?.CurrentIntent}", GUI.skin.label);
        GUILayout.Label($"State: {brain?.CurrentState}", GUI.skin.label);
        GUILayout.Space(4);

        if (brain?.Perception != null)
        {
            GUILayout.Label($"Enemies visible: {brain.Perception.EnemyList.Count}", GUI.skin.label);
            GUILayout.Label($"Loot visible: {brain.Perception.LootList.Count}", GUI.skin.label);
        }

        GUILayout.Space(4);

        var plugin = TasiaBotFriendsPlugin.Instance;
        if (plugin != null)
        {
            var entries = plugin.GetLogLines();
            GUILayout.Label($"Log (last {entries.Count}):", GUI.skin.label);
            for (int i = Mathf.Max(0, entries.Count - 6); i < entries.Count; i++)
                GUILayout.Label(entries[i], GUI.skin.label);
        }
    }

    // ══════════════════════════════════════════════════════
    //  CONFIG TAB
    // ══════════════════════════════════════════════════════
    private void DrawConfigTab()
    {
        var plugin = TasiaBotFriendsPlugin.Instance;
        if (plugin == null) return;

        GUILayout.Label("Runtime toggles (config-safe):", GUI.skin.label);
        GUILayout.Space(4);

        plugin.ShowOverlayInMenuEntry.Value = GUILayout.Toggle(plugin.ShowOverlayInMenu, " Show overlay in menu");
        plugin.EnableVoiceInOverlay = GUILayout.Toggle(plugin.EnableVoiceInOverlay, " Enable voice");
        plugin.EnableSharedVoiceInOverlay = GUILayout.Toggle(plugin.EnableSharedVoiceInOverlay, " Enable shared voice");
        plugin.EnableSyncInOverlay = GUILayout.Toggle(plugin.EnableSyncInOverlay, " Enable external sync");

        GUILayout.Space(8);
        GUILayout.Label("API keys and tokens are in config file only.", GUI.skin.label);
        GUILayout.Label("Edit Tasia.BotFriends.cfg to change them.", GUI.skin.label);
    }

    // ══════════════════════════════════════════════════════
    //  SAFETY TAB
    // ══════════════════════════════════════════════════════
    private void DrawSafetyTab()
    {
        var plugin = TasiaBotFriendsPlugin.Instance;
        if (plugin == null) return;

        if (GUILayout.Button("Despawn Tasia", GUILayout.Height(30)))
            plugin.RemoveAllBots();

        GUILayout.Space(4);
        if (GUILayout.Button("Force WAIT mode (passive)", GUILayout.Height(30)))
        {
            plugin.ForcePassiveNow();
        }

        GUILayout.Space(8);

        // God mode - only shown if enabled in config
        if (plugin.EnableTestGodModeCfg?.Value == true)
        {
            GUI.color = Color.red;
            GUILayout.Label("PRIVATE TESTING GOD MODE", GUI.skin.label);
            if (GUILayout.Button($"God Mode: {(TasiaBotFriendsPlugin.GodModeOn ? "ON" : "OFF")}", GUILayout.Height(26)))
                plugin.ToggleGodModeExternal();
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.gray;
            GUILayout.Label("God mode: disabled (set EnableMartyTestGodMode=true in config)", GUI.skin.label);
            GUI.color = Color.white;
        }
    }
}
