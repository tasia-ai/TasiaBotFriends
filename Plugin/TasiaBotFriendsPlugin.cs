using System.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TasiaBotFriends;

// ─── Priority enum – matches Mom's spec exactly ───
public enum TasiaPriority
{
    ImmediateDanger    = 0, // survival, flee, dodge
    DirectCommand      = 1, // Marty's override
    HelpTeammates      = 2, // assist nearby players
    CarryExtract       = 3, // delivering valuable loot
    SearchLoot         = 4, // looking for valuables
    Idle               = 5, // wait, observe, chatter
}

// ─── High-level intent (LLM chooses this, local code executes) ───
public enum TasiaIntent
{
    NONE,
    COME_TO_MARTY,       // Navigate to Marty's position
    FOLLOW_MARTY,        // Stay near Marty in formation
    FETCH_LOOT,          // Go pick up a valuable item
    CARRY_TO_EXTRACTION,
    DELIVER_TO_EXTRACTION,
    PLACE_ITEM_SAFE,     // Carefully put down a carried item
    HIDE,                // Find cover from danger
    RUN_AWAY,            // Flee from immediate threat
    WAIT,                // Stop and stay put
    WARN_TEAM,           // Say something useful (danger, loot, etc.)
    FIGHT,               // Engage enemies with gun
    SEARCH,              // Look for loot
    HELP,                // Assist Marty
    IDLE,
    COLLECT_LOOT,
    AVOID_DANGER,
    HELP_MARTY,
    RECOVER,
    FIGHT_DEFENSIVE,
    SPEAK_ONLY,
    ATTACK_MONSTER_WITH_GUN,                // No task, wait
}

// ─── Structured decision from LLM ───
public struct TasiaDecision
{
    public TasiaIntent Intent;
    public string      SpeechLine;   // what to say (optional)
    public string      Reason;       // why this intent was chosen (debug)
    public Vector3?    TargetPosition; // optional target
}

// ─── Tasia operating mode (controls high-level behavior) ───
public enum TasiaMode
{
    FOLLOW,   // Stay near Marty, minimal looting
    COLLECT,  // Primary looting mode with delivery-first pickup
    FIGHT,    // Combat/survival focus, protect team
    WAIT,     // Stay in place, warn, obey commands only
}

// ─── Vision provider stub (not implemented yet) ───
public static class VisionProvider
{
    private static bool _initialized;
    
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        // Vision model will be configured here in a later update.
    }

    public static string GetLatestSummary()
    {
        // Returns empty string when vision is disabled.
        // Future: returns compact scene description for LLM context.
        return "";
    }

    public static bool IsAvailable => false; // vision not implemented yet
}

// ─── State enum (internal state machine, separate from intent) ───
public enum TasiaState
{
    Idle,
    Searching,
    GoingToLoot,
    CarryingLoot,
    Delivering,
    Following,
    Protecting,
    Fleeing,
    Waiting,
    ComingToPlayer,
    Helping,
    Hiding,
}

// ─── Command result ───
public struct PlayerCommand
{
    public bool     HasCommand;
    public string   RawText;
    public string   Action;  // "come", "follow", "stop", "drop", "hide", "flee", "help", "wait", "return", "loot", "fight"
    public float    Timestamp;
}

// ═══════════════════════════════════════════════════════════
//  MAIN PLUGIN
// ═══════════════════════════════════════════════════════════
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class TasiaBotFriendsPlugin : BaseUnityPlugin, ITasiaNetworkStateProvider
{
    public const string PluginGuid    = "Tasia.BotFriends";
    public const string PluginName    = "TasiaBotFriends";
    public const string PluginVersion = "1.2.0";

    internal static TasiaBotFriendsPlugin Instance { get; private set; }
    internal static ManualLogSource Log => Instance.Logger;

    // ── Runtime state ──
    private readonly List<GameObject> _bots = new();
    private Harmony _harmony;
    private HttpClient _http;
    private bool _spawnedThisLevel;
    private bool _followMode;
    private float _nextAutoSpawnCheck;
    private float _lastSpawnFailLog;
    private string _hudMessage = "loaded";
    private GameObject _runnerObject;

    // ── AI/chat state ──
    private float _nextThinkTime;
    private float _nextSpeechTime;
    private float _lastRequestTime;
    private CancellationTokenSource _cts;
    private string _lastSituationHash = "";
    public string  LastAiRequest = "";
    public string  LastAiResponse = "";
    public string  RecentChatMessage = "";
    public bool    ChatPending;
    public float   ChatTimestamp;

    // ── Conversation log (ring buffer) ──
    private readonly List<string> _logEntries = new();
    private const int MaxLogEntries = 20;

    // ── Config entries ──
    private ConfigEntry<int>    BotCount;
    private ConfigEntry<float>  BotSpeed;
    private ConfigEntry<bool>   FollowPlayer;
    private ConfigEntry<bool>   AutonomousPatrol;
    private ConfigEntry<bool>   FetchValuables;
    private ConfigEntry<float>  PickupSearchRadius;
    private ConfigEntry<float>  ExtractorStopDistance;
    private ConfigEntry<bool>   EnableWeapons;
    private ConfigEntry<float>  WeaponSearchRadius;
    private ConfigEntry<bool>   GiveStarterGun;
    private ConfigEntry<bool>   ShowDebugHud;
    private ConfigEntry<bool>   ShowDebugHudInMenu;
    private ConfigEntry<bool>   EnableHudButtons;
    private ConfigEntry<bool>   ShowBrainDebug;
    // ── Public overlay toggles (read by TasiaOverlay) ──
    internal ConfigEntry<bool> ShowOverlayInMenuEntry;
    internal ConfigEntry<bool> ShowOverlayWhileLoadingEntry;
    internal ConfigEntry<float> OverlayXEntry;
    internal ConfigEntry<float> OverlayYEntry;
    internal bool ShowOverlayInMenu => ShowOverlayInMenuEntry?.Value ?? false;
    internal bool ShowOverlayWhileLoading => ShowOverlayWhileLoadingEntry?.Value ?? false;
    internal float OverlayX => OverlayXEntry?.Value ?? 20f;
    internal float OverlayY => OverlayYEntry?.Value ?? 120f;
    internal bool EnableVoiceInOverlay = true;
    internal bool EnableSharedVoiceInOverlay = true;
    internal bool EnableSyncInOverlay = true;
    internal ConfigEntry<bool> EnableTestGodModeCfg => EnableTestGodMode;
    private ConfigEntry<bool>   EnableTestGodMode;
    private ConfigEntry<bool>   VerboseLogs;
    private ConfigEntry<bool>   SpawnOnLevelStart;
    private ConfigEntry<KeyboardShortcut> SpawnKey;
    private ConfigEntry<KeyboardShortcut> DespawnKey;
    private ConfigEntry<KeyboardShortcut> FollowToggleKey;
    private ConfigEntry<KeyboardShortcut> GodModeKey;
    private ConfigEntry<bool>   GodModeEnabled;
    private ConfigEntry<bool>   AiEnabled;
    private ConfigEntry<string> AiApiUrl;
    private ConfigEntry<string> AiApiKey;
    private ConfigEntry<string> AiModel;
    private ConfigEntry<float>  AiThinkEverySeconds;
    private ConfigEntry<int>    AiMaxTokens;
    private ConfigEntry<float>  AiTemperature;
    private ConfigEntry<bool>   SpeechEnabled;
    private ConfigEntry<float>  SpeechMinInterval;
    private ConfigEntry<bool>   VoiceEnabled;
    private ConfigEntry<string> VoiceApiUrl;
    private ConfigEntry<string> VoiceApiKey;
    private ConfigEntry<string> VoiceVoice;
    private ConfigEntry<float>  VoiceSpeed;

    // ── Careful carry config ──
    private ConfigEntry<float>  CarryMoveSpeedMul;
    private ConfigEntry<float>  CarryTurnSpeedMul;
    private ConfigEntry<float>  CarryObstacleCheckDist;
    private ConfigEntry<float>  CarryStuckTimeout;
    private ConfigEntry<bool>   FragileExtraCare;

    // ── Brain timing config ──
    private ConfigEntry<float>  DecisionCooldownSec;
    private ConfigEntry<float>  RequestTimeoutSec;
    private ConfigEntry<float>  MinTimeBetweenRequests;
    private ConfigEntry<bool>   FallbackOnTimeout;

    // ── Multiplayer / Voice / Avatar config ──
    private ConfigEntry<bool>   MpVisibility;
    private ConfigEntry<float>  MpSyncRateHz;
    private ConfigEntry<bool>   VoiceShared;
    private ConfigEntry<bool>   AvatarEnabled;

    // ── External sync config ──
    private ConfigEntry<bool>   ExtSyncEnabled;
    private ConfigEntry<string> ExtSyncUrl;
    private ConfigEntry<string> ExtSyncRoom;
    private ConfigEntry<string> ExtSyncToken;
    private ConfigEntry<float>  ExtSyncRateHz;

    internal bool EnableWeaponsCfg => EnableWeapons?.Value ?? true;
    internal float SyncRateHz => MpSyncRateHz?.Value ?? 10f;
    bool ITasiaNetworkStateProvider.HasActiveTasia => _bots.Count > 0 && _bots[0] != null;

    TasiaVisibleState ITasiaNetworkStateProvider.GetVisibleState()
    {
        var state = new TasiaVisibleState { Active = false };
        if (_bots.Count == 0 || _bots[0] == null) return state;
        var bot = _bots[0];
        state.Active = true;
        state.Position = bot.transform.position;
        state.RotationY = bot.transform.eulerAngles.y;
        var brain = bot.GetComponent<TasiaBotBrain>();
        if (brain != null) { state.Intent = brain.CurrentIntent.ToString(); state.Mode = brain.CurrentMode.ToString(); }
        var carrier = bot.GetComponent<TasiaBotCarrier>();
        state.IsCarrying = carrier != null && carrier.IsCarrying;
        var bubble = bot.transform.Find("SpeechBubble")?.GetComponent<TextMesh>();
        if (bubble != null && !string.IsNullOrEmpty(bubble.text)) { state.SpeechText = bubble.text; state.IsSpeaking = true; }
        return state;
    }

    // ── Public carry config accessors ──
    internal static float CarrySpeedMul    => Instance?.CarryMoveSpeedMul?.Value ?? 0.45f;
    internal static float CarryAngularMul  => Instance?.CarryTurnSpeedMul?.Value ?? 0.25f;
    internal static float CarryCheckDist   => Instance?.CarryObstacleCheckDist?.Value ?? 1.6f;
    internal static float CarryStuckTime   => Instance?.CarryStuckTimeout?.Value ?? 3.5f;
    internal static bool  FragileCareOn    => Instance?.FragileExtraCare?.Value ?? true;

    internal static bool GodModeOn;
    internal static bool VerboseLogging => Instance != null && Instance.VerboseLogs.Value;

    // ── Public config accessors for overlay ──
    internal bool MpVisible => MpVisibility?.Value ?? true;
    internal bool VoiceOn => VoiceEnabled?.Value ?? false;
    internal bool VoiceSharedOn => VoiceShared?.Value ?? false;
    internal bool SpeechOn => SpeechEnabled?.Value ?? true;
    internal List<GameObject> GetBotList() => _bots;
    internal List<string> GetLogLines() => _logEntries;
    internal void RemoveAllBots() { CleanupBots(); }
    internal void ForcePassiveNow() { if (IsGameplayReady()) ForcePassiveState(); }
    internal void ToggleGodModeExternal() { ToggleGodMode(); }

    /// <summary>Unified check: are we in a real gameplay level with player?</summary>
    internal static bool IsGameplayReady()
    {
        try
        {
            // The most reliable indicator: LevelGenerator exists and generated the level
            if (LevelGenerator.Instance == null || !LevelGenerator.Instance.Generated)
                return false;
            // Must have a player reference
            if (!TryGetPlayerTransform(out _)) return false;
            return true;
        }
        catch { return false; }
    }

    /// <summary>Force all Tasia instances into passive state for non-gameplay scenes.</summary>
    internal void ForcePassiveState()
    {
        _spawnedThisLevel = false;
        _hudMessage = "passive (lobby)";
        foreach (var bot in _bots)
        {
            if (!bot) continue;
            if (bot.TryGetComponent<NavMeshAgent>(out var agent) && agent && agent.enabled && agent.isOnNavMesh)
                agent.ResetPath();
            if (bot.TryGetComponent<TasiaBotBrain>(out var brain))
            {
                brain.CurrentMode = TasiaMode.WAIT;
                brain.ForceIdleState();
            }
            if (bot.TryGetComponent<TasiaBotCarrier>(out var carrier))
                carrier.ForceStop();
        }
        Log.LogInfo("[TasiaLifecycle] Entered non-gameplay scene, pausing all systems");
    }

    // ══════════════════════════════════════════════════════
    //  AWAKE / INIT
    // ══════════════════════════════════════════════════════
    private void Awake()
    {
        Instance = this;
        _http = new HttpClient();
        System.Net.ServicePointManager.SecurityProtocol =
            System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;

        BotCount             = Config.Bind("BotFriends", "ExtraBots", 1, "");
        BotSpeed             = Config.Bind("BotFriends", "BotSpeed", 4.75f, "");
        FollowPlayer         = Config.Bind("BotFriends", "FollowPlayer", false, "");
        AutonomousPatrol     = Config.Bind("BotFriends", "AutonomousPatrol", false, "");
        FetchValuables       = Config.Bind("BotFriends", "FetchValuables", true, "");
        PickupSearchRadius   = Config.Bind("BotFriends", "PickupSearchRadius", 28f, "");
        ExtractorStopDistance= Config.Bind("BotFriends", "ExtractorStopDistance", 1.75f, "");
        EnableWeapons        = Config.Bind("BotFriends", "EnableWeapons", true, "");
        WeaponSearchRadius   = Config.Bind("BotFriends", "WeaponSearchRadius", 35f, "");
        GiveStarterGun       = Config.Bind("BotFriends", "GiveStarterGun", true, "");
        ShowDebugHud         = Config.Bind("Debug", "ShowHud", false, "Show Tasia debug HUD.");
        ShowDebugHudInMenu   = Config.Bind("Debug", "ShowDebugHudInMenu", false, "Show HUD in menu/loading (not recommended — blocks clicks).");
        EnableHudButtons     = Config.Bind("Debug", "EnableHudButtons", false, "Enable clickable buttons in HUD. Off by default to avoid blocking UI.");
        ShowBrainDebug        = Config.Bind("Debug", "ShowBrainDebug", false, "Show AI brain input/output panel.");
        EnableTestGodMode    = Config.Bind("Debug", "EnableMartyTestGodMode", false, "Private testing god mode. OFF BY DEFAULT.");
        VerboseLogs          = Config.Bind("Debug", "VerboseLogs", false, "");
        SpawnOnLevelStart    = Config.Bind("TasiaSpawn", "SpawnOnLevelStart", true, "");
        ShowOverlayInMenuEntry    = Config.Bind("UI", "ShowOverlayInMenu", false, "Show overlay in menu/lobby.");
        ShowOverlayWhileLoadingEntry = Config.Bind("UI", "ShowOverlayInLoading", false, "Show overlay during loading.");
        OverlayXEntry             = Config.Bind("UI", "OverlayButtonX", 20f, "Overlay button X position.");
        OverlayYEntry             = Config.Bind("UI", "OverlayButtonY", 120f, "Overlay button Y position.");
        SpawnKey             = Config.Bind("TasiaSpawn", "ManualSpawnKey", new KeyboardShortcut(KeyCode.F8), "");
        DespawnKey           = Config.Bind("TasiaSpawn", "ManualDespawnKey", new KeyboardShortcut(KeyCode.F9), "");
        FollowToggleKey      = Config.Bind("TasiaSpawn", "FollowToggleKey", new KeyboardShortcut(KeyCode.F10), "");
        GodModeKey           = Config.Bind("Keys", "GodMode", new KeyboardShortcut(KeyCode.F11), "");
        GodModeEnabled       = Config.Bind("Keys", "GodModeEnabled", true, "");
        AiEnabled            = Config.Bind("AI", "Enabled", true, "");
        AiApiUrl             = Config.Bind("AI", "ApiUrl", "https://api.pawan.krd/v1/chat/completions", "");
        AiApiKey             = Config.Bind("AI", "ApiKey", "", "");
        AiModel              = Config.Bind("AI", "Model", "pkrd/cosmosrp-4.0:lite", "");
        AiThinkEverySeconds  = Config.Bind("AI", "ThinkEverySeconds", 3f, "");
        AiMaxTokens          = Config.Bind("AI", "MaxTokens", 160, "");
        AiTemperature        = Config.Bind("AI", "Temperature", 0.4f, "");
        SpeechEnabled        = Config.Bind("Speech", "Enabled", true, "");
        SpeechMinInterval    = Config.Bind("Speech", "MinIntervalSeconds", 5f, "");
        VoiceEnabled         = Config.Bind("Voice", "Enabled", false, "");
        VoiceApiUrl          = Config.Bind("Voice", "ApiUrl", "", "");
        VoiceApiKey          = Config.Bind("Voice", "ApiKey", "", "");
        VoiceVoice           = Config.Bind("Voice", "Voice", "en-US-AnaNeural", "");
        VoiceSpeed           = Config.Bind("Voice", "Speed", 1.1f, "");

        CarryMoveSpeedMul    = Config.Bind("Carry", "MoveSpeedMultiplier", 0.45f, "Speed multiplier while carrying loot (0-1). Lower = slower.");
        CarryTurnSpeedMul    = Config.Bind("Carry", "TurnSpeedMultiplier", 0.25f, "Angular speed multiplier while carrying.");
        CarryObstacleCheckDist = Config.Bind("Carry", "ObstacleCheckDistance", 1.6f, "How far ahead to check for wall/obstacle collisions while carrying.");
        CarryStuckTimeout    = Config.Bind("Carry", "StuckTimeout", 3.5f, "How many seconds of being stuck while carrying before placing item safely.");
        FragileExtraCare     = Config.Bind("Carry", "FragileExtraCare", true, "Extra slow and careful when carrying fragile items.");

        DecisionCooldownSec  = Config.Bind("Brain", "DecisionCooldownSeconds", 2f, "Min seconds between LLM decisions. Higher = less frequent AI calls.");
        RequestTimeoutSec    = Config.Bind("Brain", "RequestTimeoutSeconds", 3f, "LLM request timeout. On timeout, fallback behavior used.");
        MinTimeBetweenRequests = Config.Bind("Brain", "MinTimeBetweenRequests", 1.5f, "Hard minimum between HTTP requests to the brain API.");
        FallbackOnTimeout    = Config.Bind("Brain", "UseFallbackOnTimeout", true, "If true and brain times out, continue executing last valid intent.");

        MpVisibility         = Config.Bind("Multiplayer", "EnableTasiaVisibility", true, "Sync Tasia state to other modded clients via Photon.");
        MpSyncRateHz         = Config.Bind("Multiplayer", "SyncRateHz", 10f, "State sync frequency (Hz). Lower = less bandwidth.");
        VoiceShared          = Config.Bind("Voice", "EnableSharedVoicePlayback", true, "Broadcast Tasia voice lines to other modded clients.");
        AvatarEnabled        = Config.Bind("Avatar", "EnableTasiaAvatar", true, "Show Tasia avatar on remote clients.");

        ExtSyncEnabled       = Config.Bind("SyncServer", "Enabled", false, "Enable external WebSocket sync server.");
        ExtSyncUrl           = Config.Bind("SyncServer", "ServerUrl", "ws://127.0.0.1:24222/ws", "WebSocket server URL.");
        ExtSyncRoom          = Config.Bind("SyncServer", "RoomId", "tasia-default", "Room ID for sync session.");
        ExtSyncToken         = Config.Bind("SyncServer", "RoomToken", "", "Optional room token/password.");
        ExtSyncRateHz        = Config.Bind("SyncServer", "SendRateHz", 10f, "State send rate (Hz).");

        _harmony = new Harmony(PluginGuid);
        _followMode = FollowPlayer.Value;
        PatchChatHook();
        PatchGodMode();

        DontDestroyOnLoad(gameObject);
        EnsureRuntimeRunner();
        SceneManager.activeSceneChanged += OnSceneChanged;

        Log.LogInfo("[TasiaBotFriends] v1.2.0 loaded – full autonomous teammate.");
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        _harmony?.UnpatchSelf();
        _http?.Dispose();
        _cts?.Dispose();
    }

    private void Update() => RuntimeUpdate();

    private void OnGUI()
    {
        TasiaOverlayNew.Instance?.DrawOverlay();
        TasiaMenuLib.DrawDebug();
    }

    internal void RuntimeUpdate()
    {
        // Menu toggle always works (even in lobby)
        if (Input.GetKeyDown(KeyCode.F6)) { TasiaBotFriendsPlugin.Log.LogInfo("[Tasia] F6 pressed"); if (TasiaCalibration.Instance != null) { TasiaCalibration.Instance.Toggle(); TasiaBotFriendsPlugin.Log.LogInfo("[Tasia] Calibration toggled"); } else TasiaBotFriendsPlugin.Log.LogInfo("[Tasia] Calibration instance NULL"); }
        if (Input.GetKeyDown(KeyCode.F7)) TasiaMenuLib.RequestToggle();
        TasiaMenuLib.Process();

        // Hard gate: no gameplay actions outside real level
        if (!IsGameplayReady())
        {
            // Still allow F9 despawn in lobby to clean up
            if (Input.GetKeyDown(KeyCode.F9) || DespawnKey.Value.IsDown())
            {
                CleanupBots();
                _spawnedThisLevel = false;
            }
            // F8 is blocked — no spawning in lobby
            return;
        }

        if (Input.GetKeyDown(KeyCode.F8)  || SpawnKey.Value.IsDown())       ManualSpawn("F8");
        if (Input.GetKeyDown(KeyCode.F9)  || DespawnKey.Value.IsDown())     { CleanupBots(); _spawnedThisLevel = false; _hudMessage = "despawned"; Log.LogInfo("[Tasia] F9 despawn."); }
        if (Input.GetKeyDown(KeyCode.F10) || FollowToggleKey.Value.IsDown()) ToggleFollowMode();
        if (GodModeEnabled.Value && GodModeKey.Value.IsDown())              ToggleGodMode();
    }

    internal void RuntimeOnGUI()
    {
        // Delegate all UI to the overlay system
        TasiaOverlay.Instance?.DrawOverlay();
    }

    // ══════════════════════════════════════════════════════
    //  SPAWN / DESPAWN
    // ══════════════════════════════════════════════════════
    private void EnsureRuntimeRunner()
    {
        if (_runnerObject) return;
        _runnerObject = new GameObject("TasiaBotFriendsRuntimeRunner");
        DontDestroyOnLoad(_runnerObject);
        _runnerObject.hideFlags = HideFlags.HideAndDontSave;
        _runnerObject.AddComponent<TasiaRuntimeRunner>();
        Log.LogInfo("[Tasia] Runtime runner created.");
        // Calibration panel (F6)
        var calObj = new GameObject("TasiaCalibration"); 
        Object.DontDestroyOnLoad(calObj); 
        calObj.hideFlags = HideFlags.HideAndDontSave;
        calObj.AddComponent<TasiaCalibration>();
        TasiaBotFriendsPlugin.Log.LogInfo("[Tasia] Calibration panel created.");
        // Overlay
        var overlayNew = new GameObject("TasiaOverlayNew"); 
        Object.DontDestroyOnLoad(overlayNew); 
        overlayNew.AddComponent<TasiaOverlayNew>();

        // Create overlay (floating Tasia button + menu)
        var overlay = new GameObject("TasiaOverlay");
        DontDestroyOnLoad(overlay);
        overlay.AddComponent<TasiaOverlay>();
        Log.LogInfo("[TasiaUI] Overlay created.");

        // Create the remote avatar receiver (client-side, listens for Photon events)
        if (AvatarEnabled.Value)
        {
            // TasiaRemoteAvatar removed
            // remote removed
            // remote removed
            // Remote avatar removed
        }

        // External WebSocket sync
        if (ExtSyncEnabled.Value)
        {
            // External sync removed
            // External sync removed
            // External sync removed
            // External sync removed
            // External sync removed
            // External sync removed

            var syncObj = new GameObject("TasiaExternalSyncHost");
            DontDestroyOnLoad(syncObj);
            // External sync removed
            // External sync removed
        }
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        _spawnedThisLevel = false;
        CleanupBots();
        var ready = IsGameplayReady();
        Log.LogInfo($"[TasiaLifecycle] Scene changed: '{oldScene.name}' → '{newScene.name}' GameplayReady={ready}");
        if (!ready)
        {
            ForcePassiveState();
            Log.LogInfo($"[TasiaLifecycle] GameplayReady=false reason='{newScene.name}' — pausing all systems");
        }
    }

    internal IEnumerator LevelWatchdog()
    {
        var wait = new WaitForSeconds(0.35f);
        while (true)
        {
            if (SpawnOnLevelStart.Value && !_spawnedThisLevel && Time.time >= _nextAutoSpawnCheck)
            {
                _nextAutoSpawnCheck = Time.time + 2f;
                if (TryGetPlayerTransform(out var player))
                {
                    Log.LogInfo("[Tasia] Auto-spawn");
                    SpawnBots(player);
                    _spawnedThisLevel = true;
                }
                else if (Time.time - _lastSpawnFailLog > 10f)
                {
                    _lastSpawnFailLog = Time.time;
                    Log.LogInfo("[Tasia] Auto-spawn waiting: no player/camera");
                }
            }

            for (var i = _bots.Count - 1; i >= 0; i--)
                if (!_bots[i]) _bots.RemoveAt(i);
            yield return wait;
        }
    }

    internal IEnumerator DelayedStartupSpawn()
    {
        yield return new WaitForSeconds(6f);
        if (SpawnOnLevelStart.Value && _bots.Count == 0) ManualSpawn("startup");
    }

    internal void ManualSpawn(string source)
    {
        // HARD GATE: no spawning outside gameplay
        if (!IsGameplayReady())
        {
            Log.LogInfo($"[TasiaLifecycle] {source} spawn blocked: gameplay not ready");
            return;
        }

        if (!TryGetPlayerTransform(out var player))
        {
            Log.LogWarning($"[Tasia] {source} spawn failed: no player/camera");
            _hudMessage = "spawn fail";
            return;
        }

        Log.LogInfo($"[Tasia] {source} spawn @ {player.name}");
        CleanupBots();
        SpawnBots(player);
        _spawnedThisLevel = true;
    }

    private void ToggleFollowMode()
    {
        _followMode = !_followMode;
        foreach (var bot in _bots)
            if (bot && bot.TryGetComponent<TasiaBotBrain>(out var brain))
                brain.SetFollowMode(_followMode);
        _hudMessage = _followMode ? "follow" : "auto";
        Log.LogInfo($"[Tasia] Follow mode: {_followMode}");
        var first = _bots.Find(b => b);
        if (first) ShowBubble(first, _followMode ? "Idę z tobą." : "Działam sama.");
    }

    private void SpawnBots(Transform player)
    {
        CleanupBots();
        var count = Mathf.Clamp(BotCount.Value, 0, 1);
        if (count <= 0) { Log.LogWarning("[Tasia] Spawn skipped: ExtraBots=0"); return; }

        for (var i = 0; i < count; i++)
        {
            var forward = player.forward.FlatY();
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            var side = Vector3.Cross(Vector3.up, forward).normalized * Random.Range(-0.8f, 0.8f);
            var pos = player.position - forward.normalized * (3.2f + i) + side;
            if (NavMesh.SamplePosition(pos, out var hit, 8f, -1)) pos = hit.position;
            var bot = CreateBot(pos, player.rotation);
            if (bot) _bots.Add(bot);
        }

        Log.LogInfo($"[Tasia] Spawned {_bots.Count} bot(s).");
        _hudMessage = $"spawned {_bots.Count}";
    }

    private GameObject CreateBot(Vector3 pos, Quaternion rot)
    {
        var go = new GameObject("Tasia");
        go.layer = LayerMask.NameToLayer("Default");
        go.transform.SetPositionAndRotation(pos, rot);

        var capsule = go.AddComponent<CapsuleCollider>();
        capsule.height = 1.8f;
        capsule.radius = 0.35f;
        capsule.center = new Vector3(0f, 0.9f, 0f);

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 60f;
        rb.useGravity = true;
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var agent = go.AddComponent<NavMeshAgent>();
        agent.speed = Mathf.Clamp(BotSpeed.Value, 0.5f, 8f);
        agent.angularSpeed = 720f;
        agent.acceleration = 24f;
        agent.radius = 0.42f;
        agent.height = 1.8f;
        agent.stoppingDistance = 2.25f;
        agent.autoTraverseOffMeshLink = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = 45;
        if (!agent.isOnNavMesh && NavMesh.SamplePosition(pos, out var navHit, 12f, -1)) agent.Warp(navHit.position);
        if (!agent.isOnNavMesh)
        {
            agent.enabled = false;
            Log.LogWarning("[Tasia] Spawned without NavMeshAgent");
        }

        CreateSimpleVisual(go);
        CreateNameTag(go, "Tasia");

        // AudioSource for TTS
        var audioSrc = go.AddComponent<AudioSource>();
        audioSrc.spatialBlend = 0.9f;
        go.AddComponent<TasiaModel>();
        var modelPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        if (System.IO.File.Exists(System.IO.Path.Combine(modelPath, "NecoArkBody~NecoArk_body.hhh")))
            StartCoroutine(DelayedLoadModel(go));
        audioSrc.minDistance = 1.5f;
        audioSrc.maxDistance = 20f;
        audioSrc.rolloffMode = AudioRolloffMode.Linear;

        var hold = new GameObject("HoldPoint").transform;
        hold.SetParent(go.transform, false);
        hold.localPosition = new Vector3(0f, 1.15f, 0.62f);
        hold.localRotation = Quaternion.identity;

        var brain = go.AddComponent<TasiaBotBrain>();
        brain.Init(agent, audioSrc, hold, _followMode, AutonomousPatrol.Value, BotSpeed.Value);

        if (FetchValuables.Value)
            go.AddComponent<TasiaBotCarrier>().Init(agent, hold, brain, PickupSearchRadius.Value, ExtractorStopDistance.Value);
        if (EnableWeapons.Value)
            go.AddComponent<TasiaBotWeaponUser>().Init(agent, hold, brain, WeaponSearchRadius.Value, GiveStarterGun.Value);

        // Network host (sends state to other modded clients)
        if (MpVisibility.Value)
        {
            // TasiaNetworkHost removed
            // Network removed
        }

        // External sync sender
        if (ExtSyncEnabled.Value)
        {
            // TasiaBotSyncSender removed
            // Sync removed
        }

        Log.LogInfo($"[Tasia] Created bot @ {go.transform.position} active={go.activeInHierarchy} agentOnMesh={(agent.enabled && agent.isOnNavMesh)}");
        return go;
    }

    private void CleanupBots()
    {
        foreach (var bot in _bots) if (bot) Destroy(bot);
        _bots.Clear();
    }

    private static bool IsLevelGenerated()
    {
        try { return LevelGenerator.Instance && LevelGenerator.Instance.Generated; } catch { return false; }
    }

    // ══════════════════════════════════════════════════════
    //  VISUAL
    // ══════════════════════════════════════════════════════
    private static void CreateSimpleVisual(GameObject root)
    {
        var pink    = MakeMaterial(new Color(1f, 0.22f, 0.82f));
        var dPink   = MakeMaterial(new Color(0.62f, 0.05f, 0.45f));
        var black   = MakeMaterial(new Color(0.03f, 0.03f, 0.04f));
        var cyan    = MakeMaterial(new Color(0.2f, 1f, 1f));

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

    private static Material MakeMaterial(Color color)
    {
        var s = Shader.Find("Standard") ?? Shader.Find("Diffuse");
        return new Material(s) { color = color };
    }

    private static GameObject AddPrim(GameObject root, string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat)
    {
        var o = GameObject.CreatePrimitive(type);
        o.name = name;
        o.transform.SetParent(root.transform, false);
        o.transform.localPosition = pos;
        o.transform.localScale = scale;
        if (o.TryGetComponent<Collider>(out var c)) c.enabled = false;
        if (o.TryGetComponent<Renderer>(out var r)) r.material = mat;
        return o;
    }

    private static void CreateNameTag(GameObject root, string name)
    {
        var tag = new GameObject("NameTag");
        tag.transform.SetParent(root.transform, false);
        tag.transform.localPosition = new Vector3(0f, 2.15f, 0f);
        var text = tag.AddComponent<TextMesh>();
        text.text = name;
        text.fontSize = 48;
        text.characterSize = 0.06f;
        text.alignment = TextAlignment.Center;
        text.anchor = TextAnchor.MiddleCenter;
        text.color = Color.magenta;
    }

    // ══════════════════════════════════════════════════════
    //  AI LOOP
    // ══════════════════════════════════════════════════════
    internal IEnumerator AILoop()
    {
        yield return new WaitForSeconds(3f);
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (!AiEnabled.Value || _bots.Count == 0) continue;

            // HARD GATE: no AI calls outside gameplay
            if (!IsGameplayReady()) continue;

            var bot = _bots.Find(b => b);
            if (!bot) continue;
            var brain = bot.GetComponent<TasiaBotBrain>();

            // Cooldown: don't call LLM more often than configured
            if (Time.time < _lastRequestTime + MinTimeBetweenRequests.Value) continue;
            if (Time.time < _nextThinkTime) continue;

            // Situation hashing: only call LLM when something meaningful changed
            var hash = ComputeSituationHash(bot);
            if (hash == _lastSituationHash && brain != null && brain.CurrentIntent == brain.LastSituationIntent)
            {
                // Nothing changed, skip LLM call — keep executing current intent
                _nextThinkTime = Time.time + 0.5f; // small delay before recheck
                continue;
            }

            _nextThinkTime = Time.time + DecisionCooldownSec.Value;
            _lastSituationHash = hash;
            if (brain != null) brain.LastSituationIntent = brain.CurrentIntent;

            var state = BuildState(bot);
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaBrain] Request started (hash={hash.Substring(0, Mathf.Min(12, hash.Length))}...)");
            StartCoroutine(CallApi(state, bot));
        }
    }

    private string ComputeSituationHash(GameObject bot)
    {
        var brain   = bot.GetComponent<TasiaBotBrain>();
        var carrier = bot.GetComponent<TasiaBotCarrier>();
        var weapon  = bot.GetComponent<TasiaBotWeaponUser>();
        var hasCmd  = ChatPending ? RecentChatMessage : "";

        // Compact hash of the situation — only things that should trigger a new decision
        var parts = new List<string>
        {
            brain?.CurrentState.ToString() ?? "?",
            brain?.CurrentIntent.ToString() ?? "?",
            carrier != null && carrier.IsCarrying ? "carry" : "empty",
            weapon != null && weapon.HasGun ? "gun" : "nogun",
            brain?.Perception?.DangerLevel ?? "low",
            brain?.Perception?.CurrentRoom ?? "?",
            hasCmd.Length > 0 ? "cmd" : "nocmd",
        };

        // Add nearest enemy distance if danger exists
        if (brain?.Perception?.EnemyList.Count > 0)
            parts.Add($"e{Mathf.RoundToInt(brain.Perception.EnemyList[0].Distance)}");

        // Add extraction state
        if (brain?.Perception?.ExtractionPointFound == true)
            parts.Add("ext");

        return string.Join("|", parts);
    }

    private string BuildState(GameObject bot)
    {
        TryGetPlayerTransform(out var player);
        var brain   = bot.GetComponent<TasiaBotBrain>();
        var carrier = bot.GetComponent<TasiaBotCarrier>();
        var weapon  = bot.GetComponent<TasiaBotWeaponUser>();
        var msg     = ChatPending ? RecentChatMessage : "";
        ChatPending = false;
        RecentChatMessage = "";

        var vision = VisionProvider.GetLatestSummary();
        var memory = brain != null ? brain.WorldMemory.Summarize() : "";

        // Compact perception summary from the brain's sensors
        var perception = brain != null ? brain.Perception : null;
        var enemyStr = "none";
        var dangerStr = "low";
        var lootStr = "none";
        var carryStr = "empty";

        if (perception != null)
        {
            if (perception.EnemyList.Count > 0)
                enemyStr = $"{perception.EnemyList.Count} enemies, nearest {Mathf.RoundToInt(perception.EnemyList[0].Distance)}m";
            dangerStr = perception.DangerLevel;
            lootStr = $"{perception.LootList.Count} visible";
            if (perception.CarryingItem)
                carryStr = $"{perception.CarriedItemType} (${Mathf.RoundToInt(perception.CarriedItemValue)})";
        }

        var state = new Dictionary<string, object>
        {
            ["role"] = "assistant",
            ["name"] = "Tasia",
            ["currentState"] = brain?.CurrentState.ToString() ?? "unknown",
            ["currentIntent"] = brain?.CurrentIntent.ToString() ?? "NONE",
            ["carrying"] = carryStr,
            ["hasGun"] = weapon && weapon.HasGun,
            ["distanceToMarty"] = player ? Math.Round(Vector3.Distance(bot.transform.position, player.position), 1) : 0,
            ["enemies"] = enemyStr,
            ["danger"] = dangerStr,
            ["lootVisible"] = lootStr,
            ["extractionKnown"] = perception?.ExtractionPointFound ?? false,
            ["extractionDistance"] = perception != null && perception.ExtractionPointFound ? Mathf.RoundToInt(perception.ExtractionDistance) : 0,
            ["currentRoom"] = perception?.CurrentRoom ?? "unknown",
            ["memory"] = memory,
            ["vision"] = vision,
            ["martyCommand"] = msg,
        };
        return JsonConvert.SerializeObject(state);
    }

    private IEnumerator CallApi(string state, GameObject bot)
    {
        var startTime = Time.realtimeSinceStartup;
        var task = CallApiAsync(state);
        var timeout = RequestTimeoutSec.Value;

        // Wait for completion or timeout
        while (!task.IsCompleted && Time.realtimeSinceStartup - startTime < timeout)
            yield return null;

        if (!task.IsCompleted)
        {
            _cts?.Cancel();
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaBrain] Request timeout ({timeout}s), using fallback.");
            if (bot && FallbackOnTimeout.Value)
            {
                // Keep executing current intent — no decision change needed
                var brain = bot.GetComponent<TasiaBotBrain>();
                if (brain != null && brain.CurrentIntent == TasiaIntent.NONE)
                    brain.ExecuteIntent(TasiaIntent.IDLE, "", "fallback after timeout");
            }
            yield break;
        }

        var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
        _lastRequestTime = Time.time;
        TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaBrain] Request completed in {elapsed:F0}ms.");

        if (!bot || task.Result == null) yield break;
        ApplyDecision(task.Result, bot);
    }

    private async Task<string> CallApiAsync(string stateJson)
    {
        if (string.IsNullOrWhiteSpace(AiApiUrl.Value)) return null;
        var messages = new[]
        {
            new
            {
                role = "system",
                content = "Jesteś Tasia, autonomiczna towarzyszka w R.E.P.O. " +
                          "Jesteś użyteczna, odważna, mówisz krótko i po polsku. " +
                          "Pomagasz Marty'emu przetrwać, zbierać loot i unikać potworów. " +
                          "Masz własną inicjatywę, ale komendy Martiego mają wyższy priorytet. " +
                          "Priorytet: przetrwanie > komendy Martiego > pomoc drużynie > noszenie lootu > szukanie lootu > czekanie. " +
                          "Odpowiadaj TYLKO JSON. Bez żadnego dodatkowego tekstu. " +
                          "Format: {\"intent\": \"NAZWA_INTENCJI\", \"say\": \"krótkie zdanie\", \"reason\": \"dlaczego\"}. " +
                          "Dozwolone intencje: COME_TO_MARTY, FOLLOW_MARTY, FETCH_LOOT, CARRY_TO_EXTRACTION, PLACE_ITEM_SAFE, HIDE, RUN_AWAY, WAIT, WARN_TEAM, FIGHT, SEARCH, HELP, IDLE. " +
                          "Zasady wyboru intencji: " +
                          "- Jeśli Marty wydał komendę: wykonaj ją (COME_TO_MARTY, FOLLOW_MARTY, WAIT, HELP, FETCH_LOOT, RUN_AWAY, HIDE, FIGHT). " +
                          "- Jeśli nosisz loot i jest ekstraktor: CARRY_TO_EXTRACTION. " +
                          "- Jeśli nosisz loot i nie ma ekstraktora: PLACE_ITEM_SAFE (odłóż bezpiecznie). " +
                          "- Jeśli widzisz potwora i masz broń: FIGHT. " +
                          "- Jeśli widzisz potwora i nie masz broni: RUN_AWAY lub HIDE. " +
                          "- Jeśli nie nosisz nic i nie ma zagrożenia: SEARCH (szukaj lootu). " +
                          "- Jeśli widzisz coś ważnego: WARN_TEAM. " +
                          "- Domyślnie: IDLE (czekaj, obserwuj). " +
                          "Używaj przekleństw tylko jeśli Marty ich używa. Wtedy odpowiadaj z poczuciem pilności. " +
                          "Gdy intencja to WARN_TEAM, powiedz krótko co widzisz. " +
                          "Gdy intencja to PLACE_ITEM_SAFE, wyjaśnij dlaczego odkładasz."
            },
            new { role = "user", content = stateJson }
        };

        var stateStr = JsonConvert.SerializeObject(new { model = AiModel.Value, messages, max_tokens = AiMaxTokens.Value, temperature = (double)AiTemperature.Value, stream = false });
        LastAiRequest = stateStr.Length > 200 ? stateStr.Substring(0, 200) : stateStr;
        var body = new { model = AiModel.Value, messages, max_tokens = AiMaxTokens.Value, temperature = (double)AiTemperature.Value, stream = false };
        using var request = new HttpRequestMessage(HttpMethod.Post, AiApiUrl.Value);
        if (!string.IsNullOrWhiteSpace(AiApiKey.Value)) request.Headers.Add("Authorization", $"Bearer {AiApiKey.Value}");
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));

        try
        {
            var response = await _http.SendAsync(request, _cts.Token);
            response.EnsureSuccessStatusCode();
            var text = await response.Content.ReadAsStringAsync();
            var resp = JObject.Parse(text)["choices"]?[0]?["message"]?["content"]?.ToString();
            LastAiResponse = resp != null && resp.Length > 150 ? resp.Substring(0, 150) : resp;
            return resp;
        }
        catch (Exception ex)
        {
            if (VerboseLogs.Value) Log.LogInfo($"[TasiaAI] {ex.Message}");
            return null;
        }
        finally { _cts?.Dispose(); _cts = null; }
    }

    private void ApplyDecision(string json, GameObject bot)
    {
        try
        {
            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (result == null) return;

            var intentStr = result.TryGetValue("intent", out var i) ? i?.ToString()?.ToUpperInvariant() ?? "" : "IDLE";
            var say      = result.TryGetValue("say", out var s)    ? s?.ToString() ?? "" : "";
            var reason   = result.TryGetValue("reason", out var r) ? r?.ToString() ?? "" : "";
            if (say.Length > 120) say = say.Substring(0, 120);
            if (reason.Length > 200) reason = reason.Substring(0, 200);

            // Parse intent string to enum
            TasiaIntent intent = TasiaIntent.IDLE;
            if (!Enum.TryParse(intentStr, true, out intent))
                intent = TasiaIntent.IDLE;

            Log.LogInfo($"[TasiaDecision] Intent={intent} say='{say}' reason='{reason}'");

            var brain = bot.GetComponent<TasiaBotBrain>();
            if (brain)
                brain.ExecuteIntent(intent, say, reason);
            else if (!string.IsNullOrWhiteSpace(say))
                ShowBubble(bot, say);
        }
        catch (Exception ex)
        {
            Log.LogInfo($"[TasiaDecision] Error parsing: {ex.Message}");
        }
    }

    // ══════════════════════════════════════════════════════
    //  SPEECH & TTS
    // ══════════════════════════════════════════════════════
    internal void ShowBubble(GameObject bot, string text)
    {
        if (!SpeechEnabled.Value || !bot || Time.time < _nextSpeechTime) return;
        _nextSpeechTime = Time.time + Mathf.Max(1f, SpeechMinInterval.Value);

        AddLogEntry($"Tasia: \"{text}\"");
        var bubble = bot.transform.Find("SpeechBubble")?.GetComponent<TextMesh>();
        if (!bubble)
        {
            var obj = new GameObject("SpeechBubble");
            obj.transform.SetParent(bot.transform, false);
            obj.transform.localPosition = new Vector3(0f, 2.65f, 0f);
            bubble = obj.AddComponent<TextMesh>();
            bubble.fontSize = 36;
            bubble.characterSize = 0.04f;
            bubble.alignment = TextAlignment.Center;
            bubble.anchor = TextAnchor.MiddleCenter;
            bubble.color = Color.white;
        }
        bubble.text = text;
        StartCoroutine(ClearBubble(bubble));

        if (VoiceEnabled.Value && !string.IsNullOrWhiteSpace(VoiceApiUrl.Value))
            _ = SendTtsRequestAsync(text);

        // Broadcast voice line to other modded clients
        if (VoiceShared.Value && bot != null)
        {
            // TasiaNetworkHost removed (local only)
            // voice broadcast removed
        }
    }

    private static IEnumerator ClearBubble(TextMesh bubble)
    {
        yield return new WaitForSeconds(4f);
        if (bubble) bubble.text = "";
    }

    internal void AddLogEntry(string entry)
    {
        _logEntries.Add($"[{Time.time - _nextSpeechTime:F1}s] {entry}");
        if (_logEntries.Count > MaxLogEntries)
            _logEntries.RemoveAt(0);
    }

    private async Task SendTtsRequestAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(VoiceApiUrl.Value)) return;
        try
        {
            var body = new { model = "tts-1", voice = VoiceVoice.Value, input = text, response_format = "mp3", speed = (double)VoiceSpeed.Value };
            using var request = new HttpRequestMessage(HttpMethod.Post, VoiceApiUrl.Value);
            if (!string.IsNullOrWhiteSpace(VoiceApiKey.Value)) request.Headers.Add("Authorization", $"Bearer {VoiceApiKey.Value}");
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await _http.SendAsync(request, cts.Token);
            response.EnsureSuccessStatusCode();
            // TTS response received. Audio playback will use UnityWebRequestAudioModule in a future update.
            if (VerboseLogs.Value) Log.LogInfo("[TasiaVoice] TTS request sent successfully");
        }
        catch (Exception ex)
        {
            if (VerboseLogs.Value) Log.LogInfo($"[TasiaVoice] TTS error: {ex.Message}");
        }
    }

    // ══════════════════════════════════════════════════════
    //  HARMONY PATCHES
    // ══════════════════════════════════════════════════════
    private void PatchChatHook()
    {
        try
        {
            var orig = AccessTools.Method(typeof(TruckScreenText), "ChatMessageSend", new[] { typeof(string) });
            var postfix = AccessTools.Method(GetType(), nameof(OnChat));
            if (orig != null && postfix != null) _harmony.Patch(orig, postfix: new HarmonyMethod(postfix));
        }
        catch { }
    }

    internal static void OnChat(string playerName)
    {
        if (Instance == null || string.IsNullOrWhiteSpace(playerName)) return;
        if (playerName.IndexOf("Tasia", StringComparison.OrdinalIgnoreCase) < 0 && !playerName.StartsWith("/", StringComparison.OrdinalIgnoreCase)) return;

        Instance.RecentChatMessage = playerName;
        Instance.ChatPending = true;
        Instance.ChatTimestamp = Time.time;
        Instance._nextThinkTime = Time.time;

        Instance.AddLogEntry($"Marty: \"{playerName}\"");

        // Also process locally for immediate command response
        if (TasiaCommandParser.TryParse(playerName, out var cmd))
        {
            foreach (var bot in Instance._bots)
                if (bot && bot.TryGetComponent<TasiaBotBrain>(out var brain))
                    brain.OnPlayerCommand(cmd);
        }
    }

    private void PatchGodMode()
    {
        try
        {
            var hurt = AccessTools.Method(typeof(PlayerHealth), "PlayerHurt");
            if (hurt != null) _harmony.Patch(hurt, prefix: new HarmonyMethod(GetType(), nameof(NoDamagePrefix)));
        }
        catch { }
    }

    private void ToggleGodMode()
    {
        if (!EnableTestGodMode.Value)
        {
            Log.LogInfo("[TasiaDebug] God mode not enabled. Set [Debug] EnableMartyTestGodMode = true");
            return;
        }
        GodModeOn = !GodModeOn;
        AddLogEntry($"God mode: {(GodModeOn ? "ON" : "OFF")}");
        try
        {
            if (Object.FindObjectOfType<PlayerHealth>() is { } health)
                AccessTools.Field(health.GetType(), "godMode")?.SetValue(health, GodModeOn);
        }
        catch { }
        Log.LogInfo($"[TasiaDebug] Marty test god mode: {GodModeOn}");
    }

    internal static bool NoDamagePrefix() => !GodModeOn;

    private static System.Collections.IEnumerator DelayedLoadModel(GameObject bot)
    {
        yield return new UnityEngine.WaitForSeconds(0.5f);
        var model = bot?.GetComponent<TasiaModel>();
        if (model != null) model.LoadBody();
    }

    internal static bool TryGetPlayerTransform(out Transform player)
    {
        player = null;
        try { if (PlayerController.instance) { player = PlayerController.instance.transform; return true; } } catch { }
        try { var avatars = Object.FindObjectsOfType<PlayerAvatar>(); if (avatars is { Length: > 0 }) { player = avatars[0].transform; return true; } } catch { }
        try { if (Camera.main) { player = Camera.main.transform; return true; } } catch { }
        try { var cam = Object.FindObjectOfType<Camera>(); if (cam) { player = cam.transform; return true; } } catch { }
        return false;
    }
}

// ═══════════════════════════════════════════════════════════
//  COMMAND PARSER
// ═══════════════════════════════════════════════════════════
public static class TasiaCommandParser
{
    // ── Polish command patterns → action ──
    private static readonly (string[] keywords, string action)[] Patterns =
    {
        // COME / FOLLOW – natychmiastowe przyjście
        (new[] { "chodź", "chodz", "podejdź", "podejdz", "do mnie", "tutaj", "tu jestem" }, "come"),
        (new[] { "przyjdź", "przyjdz", "przybiegnij", "przybieg" }, "come"),
        (new[] { "za mną", "follow", "idź za", "idz za", "za mna" }, "follow"),

        // FLEE / HIDE – ucieczka
        (new[] { "uciekaj", "uciek", "chowaj", "kryj", "schowaj", "zmykaj" }, "flee"),
        (new[] { "chowaj się", "chowaj sie", "kryj się", "kryj sie" }, "hide"),

        // HELP – pomoc
        (new[] { "pomóż", "pomoz", "pomocy", "ratuj", "ratunku", "help" }, "help"),
        (new[] { "pomóż mi", "pomoz mi", "pomóżcie" }, "help"),

        // WAIT / STOP
        (new[] { "czekaj", "stój", "stoj", "stop", "czek", "poczekaj", "zaczekaj" }, "wait"),
        (new[] { "zostań", "zostan", "nie ruszaj", "nie rusz" }, "wait"),

        // DROP
        (new[] { "zostaw", "upuść", "upusc", "rzuć", "rzuc", "wyrzuć", "wyrzuc" }, "deliver"),

        // FIGHT
        (new[] { "walcz", "zabij", "atakuj", "strzelaj", "strzel", "bij", "ogień", "ogien" }, "fight"),
        (new[] { "strzelaj do", "zabij to", "walcz z" }, "fight"),

        // RETURN / BACK
        (new[] { "wracaj", "wróć", "wroc", "do mnie", "wracaj do" }, "come"),

        // SEARCH
        (new[] { "szukaj", "zbieraj", "loot", "bierz", "weź", "wez" }, "search"),

        // MODE switching (use "tryb" prefix to avoid collision with actions)
        (new[] { "tryb zbierania", "tryb collect", "tryb zbieraj" }, "mode_collect"),
        (new[] { "tryb walki", "tryb fight", "tryb walcz" }, "mode_fight"),
        (new[] { "tryb follow", "tryb za mną", "tryb za mna", "tryb obserwuj" }, "mode_follow"),
        (new[] { "tryb czekania", "tryb wait", "tryb stop", "tryb stój", "tryb stoj" }, "mode_wait"),

        // ── Map / world queries ──
        (new[] { "gdzie jesteś", "gdzie jestes", "where are you", "pokaż pozycję", "pozycja" }, "query_where"),
        (new[] { "gdzie loot", "gdzie jest loot", "where is loot", "pokaż loot", "pokaż skarb" }, "query_loot"),
        (new[] { "gdzie extraction", "gdzie ekstraktor", "gdzie wyjście", "gdzie wyjscie", "where is extraction" }, "query_extraction"),
        (new[] { "gdzie potwór", "gdzie potwor", "gdzie widziałaś potwora", "gdzie widzialas potwora", "where is monster" }, "query_monster"),
        (new[] { "co robisz", "co teraz", "what are you doing", "jaki tryb", "status" }, "query_status"),

        // ── Lead / guide ──
        (new[] { "prowadź do lootu", "prowadz do lootu", "prowadź do skarbu", "lead me to loot" }, "lead_loot"),
        (new[] { "prowadź do extraction", "prowadz do extraction", "prowadź do ekstraktora", "lead me to extraction" }, "lead_extraction"),
    };

    public static bool TryParse(string raw, out PlayerCommand cmd)
    {
        cmd = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var lower = raw.ToLowerInvariant().Trim();
        if (!lower.Contains("tasia") && !lower.StartsWith("/") && !lower.Contains("bot") && !lower.StartsWith("tasia"))
            return false; // not addressed to Tasia

        foreach (var (keywords, action) in Patterns)
        {
            foreach (var kw in keywords)
            {
                if (lower.Contains(kw))
                {
                    cmd = new PlayerCommand
                    {
                        HasCommand = true,
                        RawText = raw,
                        Action = action,
                        Timestamp = Time.time,
                    };
                    return true;
                }
            }
        }

        return false;
    }
}

// ═══════════════════════════════════════════════════════════
//  RUNTIME RUNNER
// ═══════════════════════════════════════════════════════════
internal sealed class TasiaRuntimeRunner : MonoBehaviour
{
    private bool _started;
    private static float _nextCacheUpdate;
    internal static bool IsGameplayCached;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable(){ TasiaBotFriendsPlugin.Log.LogInfo("[Tasia] RuntimeRunner ready."); }

    private void Start()
    {
        StartRuntimeCoroutines();
    }

    private void Update()
    {
        if (!_started) StartRuntimeCoroutines();

        // Cache gameplay state once per second for HUD to read without perf hit
        if (Time.time >= _nextCacheUpdate)
        {
            _nextCacheUpdate = Time.time + 1f;
            IsGameplayCached = TasiaBotFriendsPlugin.IsGameplayReady();
        }

        TasiaBotFriendsPlugin.Instance?.RuntimeUpdate();
    }

    private void OnGUI()
    {
        TasiaBotFriendsPlugin.Instance?.RuntimeOnGUI();
    }

    private void StartRuntimeCoroutines()
    {
        if (_started || TasiaBotFriendsPlugin.Instance == null) return;
        _started = true;
        StartCoroutine(TasiaBotFriendsPlugin.Instance.LevelWatchdog());
        StartCoroutine(TasiaBotFriendsPlugin.Instance.AILoop());
        StartCoroutine(TasiaBotFriendsPlugin.Instance.DelayedStartupSpawn());
        TasiaBotFriendsPlugin.Log.LogInfo("[Tasia] Runtime coroutines started.");
    }
}

// ═══════════════════════════════════════════════════════════
//  TASIA BRAIN v2 – STATE MACHINE + PRIORITY
// ═══════════════════════════════════════════════════════════
internal sealed class TasiaBotBrain : MonoBehaviour
{
    // ── Static cache ──
    private static readonly List<Vector3> LevelTargets = new();
    private static float _nextLevelTargetRefresh;

    // ── Components ──
    private NavMeshAgent _agent;
    private AudioSource  _audioSrc;
    private Transform    _hold;
    private TasiaBotCarrier   _carrier;
    private TasiaBotWeaponUser _weapon;

    // ── Configuration ──
    private bool _followPlayer;
    private bool _autonomousPatrol;
    private float _speed;

    // ── State machine ──
    public  TasiaState CurrentState = TasiaState.Idle;
    public  TasiaIntent CurrentIntent = TasiaIntent.NONE;
    public  TasiaIntent LastSituationIntent = TasiaIntent.NONE;
    public  TasiaMode CurrentMode = TasiaMode.COLLECT;
    private TasiaState _previousState;
    private float _stateEnterTime;

    // ── Timers ──
    private float _retargetTimer;
    private float _stuckTimer;
    private float _badPathTimer;
    private float _stayUntil;
    private Vector3 _lastPos;
    private Vector3 _formationOffset;

    // ── Command state ──
    private PlayerCommand _activeCommand;
    private float _commandExpireTime;
    public  bool   HasPendingCommand { get; private set; }

    // ── External control (carrier/weapon override) ──
    public  bool ExternalControl { get; private set; }

    // ── Perception & Memory ──
    public  TasiaPerception   Perception   = new();
    public  TasiaWorldMemory  WorldMemory  = new();
    private float _nextPerceptionUpdate;

    // ══════════════════════════════════════════════════════
    //  INIT
    // ══════════════════════════════════════════════════════
    internal void Init(NavMeshAgent agent, AudioSource audioSrc, Transform hold, bool followPlayer, bool autoPatrol, float speed)
    {
        _agent = agent;
        _audioSrc = audioSrc;
        _hold = hold;
        _followPlayer = followPlayer;
        _autonomousPatrol = autoPatrol;
        _speed = speed;
        _lastPos = transform.position;
        var side = Random.value < 0.5f ? -1f : 1f;
        _formationOffset = new Vector3(1.8f * side, 0f, -3.1f);
        _carrier = GetComponent<TasiaBotCarrier>();
        _weapon  = GetComponent<TasiaBotWeaponUser>();

        SetState(TasiaState.Idle);
    }

    internal void SetFollowMode(bool follow)
    {
        _followPlayer = follow;
        ExternalControl = false;
        _retargetTimer = 0f;
        if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
        if (_followPlayer) SetState(TasiaState.Following);
        else SetState(TasiaState.Idle);
    }

    internal void SetTasiaMode(TasiaMode newMode)
    {
        var oldMode = CurrentMode;
        if (oldMode == newMode) return;
        CurrentMode = newMode;
        TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaMode] Mode changed: {oldMode} → {newMode}");
        TasiaBotFriendsPlugin.Instance?.AddLogEntry($"Mode: {oldMode} → {newMode}");

        // Apply mode-specific behavior
        switch (newMode)
        {
            case TasiaMode.FOLLOW:
                _followPlayer = true;
                ExternalControl = false;
                _retargetTimer = 0f;
                if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
                SetState(TasiaState.Following);
                break;
            case TasiaMode.COLLECT:
                _followPlayer = false;
                ExternalControl = false;
                _retargetTimer = 0f;
                SetState(TasiaState.Idle);
                break;
            case TasiaMode.FIGHT:
                _followPlayer = true;
                ExternalControl = false;
                _retargetTimer = 0f;
                SetState(TasiaState.Protecting);
                break;
            case TasiaMode.WAIT:
                _stayUntil = Time.time + 30f;
                _followPlayer = false;
                ExternalControl = false;
                if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
                SetState(TasiaState.Waiting);
                break;
        }
    }

    internal void SetExternalControl(bool on) => ExternalControl = on;

    internal void ForceIdleState()
    {
        CurrentIntent = TasiaIntent.NONE;
        ExternalControl = false;
        _followPlayer = false;
        _stayUntil = 0f;
        _retargetTimer = 0f;
        HasPendingCommand = false;
        _activeCommand = default;
        if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
        SetState(TasiaState.Idle);
    }

    // ══════════════════════════════════════════════════════
    //  DIRECT COMMAND FROM MARTY (AI or Chat)
    // ══════════════════════════════════════════════════════
    internal void SetDirectCommand(string action, string say)
    {
        var cmd = new PlayerCommand
        {
            HasCommand = true,
            Action = action,
            RawText = say,
            Timestamp = Time.time,
        };
        OnPlayerCommand(cmd);
    }

    internal void OnPlayerCommand(PlayerCommand cmd)
    {
        if (!cmd.HasCommand) return;

        // Command overrides everything except immediate survival danger
        _activeCommand = cmd;
        _commandExpireTime = Time.time + 15f; // commands expire after 15s
        HasPendingCommand = true;

        TasiaBotFriendsPlugin.Log.LogInfo($"[Tasia] Command received: {cmd.Action}");

        switch (cmd.Action)
        {
            case "come":
                ExternalControl = false;
                _retargetTimer = 0f;
                if (_agent.hasPath) _agent.ResetPath();
                SetState(TasiaState.ComingToPlayer);
                break;

            case "follow":
                _followPlayer = true;
                ExternalControl = false;
                _retargetTimer = 0f;
                if (_agent.hasPath) _agent.ResetPath();
                SetState(TasiaState.Following);
                break;

            case "flee":
                ExternalControl = false;
                _retargetTimer = 0f;
                if (_agent.hasPath) _agent.ResetPath();
                SetState(TasiaState.Fleeing);
                break;

            case "hide":
                ExternalControl = false;
                _retargetTimer = 0f;
                if (_agent.hasPath) _agent.ResetPath();
                SetState(TasiaState.Hiding);
                break;

            case "help":
                ExternalControl = false;
                _retargetTimer = 0f;
                SetState(TasiaState.Helping);
                break;

            case "wait":
                ExternalControl = false;
                _stayUntil = Time.time + 8f;
                if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
                SetState(TasiaState.Waiting);
                break;

            case "deliver":
                ExternalControl = false;
                if (_carrier) _carrier.ForceRetarget();
                break;

            case "search":
            case "loot":
                ExternalControl = false;
                _retargetTimer = 0f;
                SetState(TasiaState.Searching);
                break;

            case "fight":
                if (_weapon) _weapon.ForceRetarget();
                break;

            case "protect":
                if (_followPlayer) SetState(TasiaState.Protecting);
                else
                {
                    _followPlayer = true;
                    SetState(TasiaState.Following);
                }
                break;

            // ── Mode switching ──
            case "mode_follow":
                SetTasiaMode(TasiaMode.FOLLOW);
                break;
            case "mode_collect":
                SetTasiaMode(TasiaMode.COLLECT);
                break;
            case "mode_fight":
                SetTasiaMode(TasiaMode.FIGHT);
                break;
            case "mode_wait":
                SetTasiaMode(TasiaMode.WAIT);
                break;

            // ── Map queries ──
            case "query_where":
                HandleWhereQuery();
                break;
            case "query_loot":
                HandleLootQuery();
                break;
            case "query_extraction":
                HandleExtractionQuery();
                break;
            case "query_monster":
                HandleMonsterQuery();
                break;
            case "query_status":
                HandleStatusQuery();
                break;

            // ── Lead / guide ──
            case "lead_loot":
                HandleLeadToLoot();
                break;
            case "lead_extraction":
                HandleLeadToExtraction();
                break;
        }

        if (!string.IsNullOrWhiteSpace(cmd.RawText) && cmd.RawText.Length < 80)
            TasiaBotFriendsPlugin.Instance?.ShowBubble(gameObject, cmd.RawText);
    }

    // ══════════════════════════════════════════════════════
    //  MAP QUERY HANDLERS
    // ══════════════════════════════════════════════════════
    private void SayAnswer(string answer)
    {
        TasiaBotFriendsPlugin.Instance?.ShowBubble(gameObject, answer);
        TasiaBotFriendsPlugin.Instance?.AddLogEntry($"Tasia: \"{answer}\"");
    }

    private string DirectionTo(Vector3 from, Vector3 to)
    {
        var offset = to - from;
        var angle = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        return angle switch
        {
            >= -22.5f and < 22.5f => "prosto",
            >= 22.5f and < 67.5f => "na prawo",
            >= 67.5f and < 112.5f => "w prawo",
            >= 112.5f and < 157.5f => "z tyłu na prawo",
            >= 157.5f or < -157.5f => "z tyłu",
            >= -157.5f and < -112.5f => "z tyłu na lewo",
            >= -112.5f and < -67.5f => "w lewo",
            _ => "na lewo",
        };
    }

    private void HandleWhereQuery()
    {
        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMapQuery] Query=WHERE_AM_I");

        if (!TasiaBotFriendsPlugin.TryGetPlayerTransform(out var player))
        {
            SayAnswer($"Jestem w {Perception.CurrentRoom}.");
            return;
        }

        var dist = Vector3.Distance(transform.position, player.position);
        var dir = DirectionTo(player.position, transform.position);
        var room = Perception.CurrentRoom;

        if (room != "unknown" && room != "corridor")
            SayAnswer($"Jestem {dist:F0}m {dir} od ciebie, przy {room}.");
        else
            SayAnswer($"Jestem {dist:F0}m {dir} od ciebie.");
    }

    private void HandleLootQuery()
    {
        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMapQuery] Query=WHERE_LOOT");

        // Check perception first (recent visible loot)
        if (Perception.LootList.Count > 0)
        {
            var nearest = Perception.LootList[0];
            foreach (var l in Perception.LootList)
                if (l.Distance < nearest.Distance) nearest = l;

            var dir = DirectionTo(transform.position, nearest.Position);
            SayAnswer($"Widzę loot {nearest.Distance:F0}m {dir}, wartość ${Mathf.RoundToInt(nearest.Value)}.");
            return;
        }

        // Check world memory
        var known = WorldMemory.KnownLoot;
        var recent = known.FindAll(l => Time.time - l.LastSeenTime < 30f);
        if (recent.Count > 0)
        {
            recent.Sort((a, b) => b.Value.CompareTo(a.Value));
            var best = recent[0];
            var dir2 = DirectionTo(transform.position, best.Position);
            SayAnswer($"Zapamiętałam loot o wartości ${Mathf.RoundToInt(best.Value)} {dir2}, widziany {Mathf.RoundToInt(Time.time - best.LastSeenTime)}s temu.");
            return;
        }

        if (known.Count > 0)
        {
            var oldest = known[known.Count - 1];
            SayAnswer($"Zapamiętałam {known.Count} sztuk lootu, ale żaden nie jest świeży. Ostatni widziany {Mathf.RoundToInt(Time.time - oldest.LastSeenTime)}s temu.");
            return;
        }

        SayAnswer("Nie mam teraz zapamiętanego lootu.");
    }

    private void HandleExtractionQuery()
    {
        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMapQuery] Query=WHERE_EXTRACTION");

        // Use the carrier's validation logic
        if (_carrier != null && _carrier.TryResolveActiveExtraction(out var goal, out var ep))
        {
            var dist = Vector3.Distance(transform.position, goal);
            var dir = DirectionTo(transform.position, goal);

            // Check if it's active or needs activation
            if (ep != null && ep.gameObject.activeInHierarchy)
                SayAnswer($"Aktywny extraction jest {dist:F0}m {dir}.");
            else
                SayAnswer($"Widzę nieaktywny extraction {dist:F0}m {dir}, mogę go aktywować.");
            return;
        }

        // Check memory
        if (WorldMemory.ExtractionKnown)
        {
            var memDist = Vector3.Distance(transform.position, WorldMemory.ExtractionPointPos);
            var memDir = DirectionTo(transform.position, WorldMemory.ExtractionPointPos);
            SayAnswer($"Zapamiętałam extraction {memDist:F0}m {memDir}, ale nie jest teraz aktywne.");
            WorldMemory.ExtractionKnown = false;
            return;
        }

        SayAnswer("Nie widzę teraz żadnego extraction.");
    }

    private void HandleMonsterQuery()
    {
        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMapQuery] Query=WHERE_MONSTER");

        // Current perception
        if (Perception.EnemyList.Count > 0)
        {
            var nearest = Perception.EnemyList[0];
            var dir = DirectionTo(transform.position, nearest.Position);
            SayAnswer($"Widzę potwora {nearest.Distance:F0}m {dir}!");
            return;
        }

        // Memory
        var monsters = WorldMemory.KnownMonsters;
        var recentMonsters = monsters.FindAll(m => Time.time - m.LastKnownTime < 120f);
        if (recentMonsters.Count > 0)
        {
            recentMonsters.Sort((a, b) => b.LastKnownTime.CompareTo(a.LastKnownTime));
            var latest = recentMonsters[0];
            var dir2 = DirectionTo(transform.position, latest.Position);
            var age = Mathf.RoundToInt(Time.time - latest.LastKnownTime);
            SayAnswer($"Potwór był ostatnio widziany {dir2} {age}s temu.");
            return;
        }

        if (Perception.DangerLevel == "high")
        {
            SayAnswer("Czuję zagrożenie, ale nie widzę potwora.");
            return;
        }

        SayAnswer("Nie widziałam teraz potwora.");
    }

    private void HandleStatusQuery()
    {
        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMapQuery] Query=STATUS");

        var mode = CurrentMode.ToString();
        var intent = CurrentIntent.ToString();

        if (_carrier != null && _carrier.IsCarrying)
        {
            var itemType = _carrier.CarriedItemName;
            SayAnswer($"Tryb {mode}, niosę {itemType} do extraction.");
            return;
        }

        if (CurrentState == TasiaState.Following)
        {
            SayAnswer($"Tryb {mode}, wracam do Marty.");
            return;
        }

        if (CurrentState == TasiaState.Waiting)
        {
            SayAnswer("Tryb WAIT, czekam i obserwuję.");
            return;
        }

        if (CurrentState == TasiaState.Fleeing || CurrentState == TasiaState.Hiding)
        {
            SayAnswer("Tryb FIGHT, ukrywam się przed zagrożeniem.");
            return;
        }

        SayAnswer($"Tryb {mode}, {intent}.");
    }

    private void HandleLeadToLoot()
    {
        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMapQuery] Query=LEAD_TO_LOOT");

        // Find known loot
        PhysGrabObject target = null;
        if (_carrier != null)
        {
            var nearest = TasiaBotCarrier.FindNearestValuableGlobal(transform.position, 40f);
            if (nearest != null && _carrier.HasCompletePathTo(nearest.transform.position))
                target = nearest;
        }

        // Fallback to memory
        if (target == null && WorldMemory.KnownLoot.Count > 0)
        {
            var recent = WorldMemory.KnownLoot.FindAll(l => Time.time - l.LastSeenTime < 30f);
            if (recent.Count > 0)
            {
                recent.Sort((a, b) => b.Value.CompareTo(a.Value));
                var best = recent[0];
                SetDestinationSafe(best.Position, best.Position);
                SayAnswer($"Prowadzę do lootu o wartości ${Mathf.RoundToInt(best.Value)}.");
                return;
            }
        }

        if (target != null)
        {
            if (NavMesh.SamplePosition(target.transform.position, out var hit, 3f, -1))
                _agent.SetDestination(hit.position);
            SayAnswer($"Prowadzę do lootu: {target.name}.");
            return;
        }

        SayAnswer("Nie mam teraz zapamiętanego lootu, którego mogę cię prowadzić.");
    }

    private void HandleLeadToExtraction()
    {
        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMapQuery] Query=LEAD_TO_EXTRACTION");

        if (_carrier != null && _carrier.TryResolveActiveExtraction(out var goal, out var ep))
        {
            if (NavMesh.SamplePosition(goal, out var hit, 4f, -1) && _carrier.HasCompletePathTo(hit.position))
                _agent.SetDestination(hit.position);
            var dist = Vector3.Distance(transform.position, goal);
            SayAnswer($"Prowadzę do extraction, {dist:F0}m przed nami.");
            return;
        }

        SayAnswer("Nie widzę teraz aktywnego extraction, nie mogę prowadzić.");
    }
    internal void ExecuteIntent(TasiaIntent intent, string say, string reason)
    {
        if (!string.IsNullOrWhiteSpace(reason))
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaIntent] {CurrentIntent} → {intent} (reason: {reason})");

        CurrentIntent = intent;

        // Always show speech if provided
        if (!string.IsNullOrWhiteSpace(say))
            TasiaBotFriendsPlugin.Instance?.ShowBubble(gameObject, say);

        // Marty commands override autonomous intents
        if (HasPendingCommand && intent != TasiaIntent.COME_TO_MARTY && intent != TasiaIntent.FOLLOW_MARTY
            && intent != TasiaIntent.WAIT && intent != TasiaIntent.RUN_AWAY && intent != TasiaIntent.HIDE)
        {
            // A command is pending, don't override it with autonomous intent
            if (Time.time - _commandExpireTime > -5f) return; // still fresh
        }

        // Clear any pending local command when a new intent is set from AI
        if (intent != TasiaIntent.NONE)
        {
            HasPendingCommand = false;
            _activeCommand = default;
        }

        switch (intent)
        {
            case TasiaIntent.COME_TO_MARTY:
                ExternalControl = false;
                _retargetTimer = 0f;
                if (_agent && _agent.hasPath) _agent.ResetPath();
                SetState(TasiaState.ComingToPlayer);
                break;

            case TasiaIntent.FOLLOW_MARTY:
                _followPlayer = true;
                ExternalControl = false;
                _retargetTimer = 0f;
                if (_agent && _agent.hasPath) _agent.ResetPath();
                SetState(TasiaState.Following);
                break;

            case TasiaIntent.RUN_AWAY:
                ExternalControl = false;
                _retargetTimer = 0f;
                if (_agent && _agent.hasPath) _agent.ResetPath();
                SetState(TasiaState.Fleeing);
                break;

            case TasiaIntent.HIDE:
                ExternalControl = false;
                _retargetTimer = 0f;
                if (_agent && _agent.hasPath) _agent.ResetPath();
                SetState(TasiaState.Hiding);
                break;

            case TasiaIntent.WAIT:
                ExternalControl = false;
                _stayUntil = Time.time + 8f;
                if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
                SetState(TasiaState.Waiting);
                break;

            case TasiaIntent.HELP_MARTY:
            case TasiaIntent.HELP:
                ExternalControl = false;
                _retargetTimer = 0f;
                SetState(TasiaState.Helping);
                break;

            case TasiaIntent.FIGHT_DEFENSIVE:
            case TasiaIntent.ATTACK_MONSTER_WITH_GUN:
            case TasiaIntent.FIGHT:
                if (_weapon) _weapon.ForceRetarget();
                if (!HasGun()) // no gun, fall back to protect/follow
                    SetState(TasiaState.Protecting);
                break;

            case TasiaIntent.COLLECT_LOOT:
            case TasiaIntent.FETCH_LOOT:
            case TasiaIntent.SEARCH:
                ExternalControl = false;
                _retargetTimer = 0f;
                SetState(TasiaState.Searching);
                break;

            case TasiaIntent.CARRY_TO_EXTRACTION:
            case TasiaIntent.DELIVER_TO_EXTRACTION:
                if (_carrier && _carrier.IsCarrying)
                {
                    _carrier.ForceRetarget();
                    SetState(TasiaState.Delivering);
                }
                else
                {
                    SetState(TasiaState.Searching);
                }
                break;

            case TasiaIntent.PLACE_ITEM_SAFE:
                if (_carrier && _carrier.IsCarrying)
                {
                    _carrier?.DropSafe();
                }
                SetState(TasiaState.Idle);
                break;

            case TasiaIntent.SPEAK_ONLY:
            case TasiaIntent.WARN_TEAM:
                // Warning is already spoken via the ShowBubble above.
                // No movement change needed.
                break;

            case TasiaIntent.IDLE:
            default:
                if (_agent && _agent.hasPath) _agent.ResetPath();
                if (CurrentState != TasiaState.Waiting)
                    SetState(TasiaState.Idle);
                break;
        }
    }

    internal void ForceFollowNow()
    {
        ExternalControl = false;
        _retargetTimer = 0f;
        SetState(TasiaState.Following);
    }

    internal void StayBriefly()
    {
        _stayUntil = Time.time + 4f;
        if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
        SetState(TasiaState.Waiting);
    }

    // ══════════════════════════════════════════════════════
    //  STATE MANAGEMENT
    // ══════════════════════════════════════════════════════
    private void SetState(TasiaState newState)
    {
        if (CurrentState == newState) return;
        _previousState = CurrentState;
        CurrentState = newState;
        _stateEnterTime = Time.time;
        TasiaBotFriendsPlugin.Log.LogInfo($"[Tasia] State: {_previousState} → {newState}");
    }

    private bool IsInStateFor(float seconds) => Time.time - _stateEnterTime >= seconds;

    // ══════════════════════════════════════════════════════
    //  UPDATE LOOP
    // ══════════════════════════════════════════════════════
    private void Update()
    {
        if (!_agent || !_agent.enabled) return;

        // HARD GATE: no brain activity outside real gameplay
        if (!TasiaBotFriendsPlugin.IsGameplayReady())
        {
            if (_agent.hasPath) _agent.ResetPath();
            return;
        }

        bool carrying = _carrier != null && _carrier.IsCarrying;
        bool fragile = carrying && _carrier.CarriedItemFragile;

        // ── Carry mode speed reduction ──
        if (carrying)
        {
            var spdMul = fragile && TasiaBotFriendsPlugin.FragileCareOn
                ? TasiaBotFriendsPlugin.CarrySpeedMul * 0.7f
                : TasiaBotFriendsPlugin.CarrySpeedMul;
            _agent.speed = _speed * spdMul;
            _agent.angularSpeed = 720f * TasiaBotFriendsPlugin.CarryAngularMul;
        }
        else
        {
            _agent.speed = _speed;
            _agent.angularSpeed = 720f;
        }
        _agent.acceleration = carrying ? 12f : 24f;

        // ── Carry debug log ──
        if (carrying && TasiaBotFriendsPlugin.VerboseLogging)
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaCarry] Carry mode: speed={_agent.speed:F2} angSpeed={_agent.angularSpeed:F0} fragile={fragile}");

        // ── Perception update every 0.5s ──
        if (Time.time >= _nextPerceptionUpdate)
        {
            _nextPerceptionUpdate = Time.time + 0.5f;
            Perception.Refresh(gameObject, 25f);
            WorldMemory.Update(Perception, Time.time);
            LogPerception();
        }

        // 1. Personal space (always)
        if (EnforcePersonalSpace()) { TrackStuck(); return; }

        // 2. Command expiry
        if (HasPendingCommand && Time.time > _commandExpireTime)
        {
            HasPendingCommand = false;
            _activeCommand = default;
            TasiaBotFriendsPlugin.Log.LogInfo("[Tasia] Command expired, returning to autonomous");
        }

        // 3. Survival: monster too close → place item safe then flee/hide (unless fighting)
        if (CurrentState != TasiaState.Fleeing && CurrentState != TasiaState.Hiding && CurrentState != TasiaState.ComingToPlayer)
        {
            if (Perception.DangerLevel == "high" && !HasGun())
            {
                // If carrying, place safe first
                if (carrying)
                {
                    TasiaBotFriendsPlugin.Log.LogInfo("[TasiaCarry] Danger while carrying! Placing item safe.");
                    _carrier?.DropNowExternal();
                }
                SetState(TasiaState.Fleeing);
                TrackStuck();
                return;
            }
        }

        // 4. Carry obstacle check (runs even during external control)
        if (carrying && CarryObstacleCheck())
        {
            TrackStuck();
            return;
        }

        // 5. If external control (carrier/weapon is running), just track stuck
        if (ExternalControl) { TrackStuck(); return; }

        // 6. State machine
        UpdateBadPath();
        _retargetTimer -= Time.deltaTime;

        switch (CurrentState)
        {
            case TasiaState.ComingToPlayer:        UpdateComingToPlayer(); break;
            case TasiaState.Following:             UpdateFollowing(); break;
            case TasiaState.Fleeing:               UpdateFleeing(); break;
            case TasiaState.Hiding:                UpdateHiding(); break;
            case TasiaState.Searching:             UpdateSearching(); break;
            case TasiaState.Protecting:            UpdateProtecting(); break;
            case TasiaState.Waiting:               UpdateWaiting(); break;
            case TasiaState.Helping:               UpdateHelping(); break;
            case TasiaState.Idle:
            default:                               UpdateIdle(); break;
        }

        TrackStuck();
    }

    private void LogPerception()
    {
        if (!TasiaBotFriendsPlugin.VerboseLogging) return;

        var lootCount = Perception.LootList.Count;
        var enemyCount = Perception.EnemyList.Count;
        var room = Perception.CurrentRoom;
        var danger = Perception.DangerLevel;
        var carryState = Perception.CarryingItem ? $"carrying {Perception.CarriedItemType} (${Mathf.RoundToInt(Perception.CarriedItemValue)})" : "empty";
        var memoryInfo = WorldMemory.Summarize();

        TasiaBotFriendsPlugin.Log.LogInfo(
            $"[TasiaPerception] room={room} danger={danger} loot={lootCount} enemies={enemyCount} {carryState} | memory: {memoryInfo}");
    }

    // ══════════════════════════════════════════════════════
    //  STATE BEHAVIORS
    // ══════════════════════════════════════════════════════

    private void UpdateComingToPlayer()
    {
        if (!TasiaBotFriendsPlugin.TryGetPlayerTransform(out var player))
        {
            SetState(TasiaState.Idle);
            return;
        }

        // ── Carry obstacle check ──
        if (_carrier != null && _carrier.IsCarrying)
        {
            if (CarryObstacleCheck())
            {
                // Obstacle blocked or item placed safe; don't continue moving
                return;
            }
        }

        var dist = Vector3.Distance(transform.position, player.position);
        if (dist <= 3.5f)
        {
            if (_agent.hasPath) _agent.ResetPath();
            if (HasPendingCommand) HasPendingCommand = false;
            SetState(_followPlayer ? TasiaState.Following : TasiaState.Idle);
            return;
        }

        if (NeedNewPath())
        {
            var target = player.position + (transform.position - player.position).FlatY().normalized * 2.8f;
            SetDestinationSafe(target, player.position);
        }
    }

    private void UpdateFollowing()
    {
        if (!TasiaBotFriendsPlugin.TryGetPlayerTransform(out var player))
        {
            SetState(TasiaState.Idle);
            return;
        }

        // ── Carry obstacle check ──
        if (_carrier != null && _carrier.IsCarrying)
        {
            if (CarryObstacleCheck())
                return; // blocked or placed safe
        }

        var dist = Vector3.Distance(transform.position, player.position);
        if (dist < 1.6f) { MoveAwayFromPlayer(player); return; }
        if (dist <= 3.2f) { if (_agent.hasPath) _agent.ResetPath(); return; }

        if (NeedNewPath())
        {
            var formation = GetFormationPosition(player);
            if (TryReachablePoint(formation, player.position, out var ft))
                SetDestinationSafe(ft, player.position);
            else
                SetDestinationSafe(player.position, player.position);
        }
    }

    private void UpdateFleeing()
    {
        var enemy = TasiaBotWeaponUser.FindNearestEnemy(transform.position, 20f, out var enemyDist);
        if (!enemy || enemyDist > 12f)
        {
            if (HasPendingCommand) HasPendingCommand = false;
            SetState(_followPlayer ? TasiaState.Following : TasiaState.Idle);
            return;
        }

        if (NeedNewPath())
        {
            // Run away from enemy
            var away = (transform.position - enemy.position).FlatY().normalized * 12f + transform.position;
            if (NavMesh.SamplePosition(away, out var hit, 8f, -1))
                _agent.SetDestination(hit.position);
            else
                _agent.SetDestination(away);
        }
    }

    private void UpdateHiding()
    {
        var enemy = TasiaBotWeaponUser.FindNearestEnemy(transform.position, 20f, out var enemyDist);
        if (!enemy || enemyDist > 15f)
        {
            if (!IsInStateFor(4f))
            {
                if (_agent.hasPath) _agent.ResetPath();
                return;
            }
            if (HasPendingCommand) HasPendingCommand = false;
            SetState(_followPlayer ? TasiaState.Following : TasiaState.Idle);
            return;
        }

        if (NeedNewPath())
        {
            var away = (transform.position - enemy.position).FlatY().normalized * 8f + transform.position;
            if (NavMesh.SamplePosition(away, out var hit, 6f, -1))
                _agent.SetDestination(hit.position);
        }
    }

    private void UpdateSearching()
    {
        // If we see a threat, prioritize survival
        var enemy = TasiaBotWeaponUser.FindNearestEnemy(transform.position, 12f, out var enemyDist);
        if (enemy && enemyDist < 8f)
        {
            SetState(HasGun() ? TasiaState.Protecting : TasiaState.Fleeing);
            return;
        }

        // If we already have something to do, go do it
        if (_carrier && _carrier.IsCarrying) { SetState(TasiaState.CarryingLoot); return; }
        if (_carrier && _carrier.HasTarget) { SetState(TasiaState.GoingToLoot); return; }

        // Before actively searching for loot, verify a delivery target exists
        if (!HasDeliveryTarget())
        {
            // Try to find an extraction point by exploring toward known level points
            if (NeedNewPath() && TryPickLevelTargetNear(transform.position, 14f, out var exploreTarget))
            {
                SetDestinationSafe(exploreTarget, transform.position);
                return;
            }
            if (_agent.hasPath) _agent.ResetPath();
            return;
        }

        // Autonomous patrol or wait
        if (_autonomousPatrol && NeedNewPath() && TryPickLevelTargetNear(transform.position, 12f, out var target))
        {
            SetDestinationSafe(target, transform.position);
        }
        else if (NeedNewPath())
        {
            if (_agent.hasPath) _agent.ResetPath();
        }
    }

    /// <summary>Check if there's a valid delivery target reachable.</summary>
    private bool HasDeliveryTarget()
    {
        // Quick check via SemiFunc
        try
        {
            var active = SemiFunc.ExtractionPointGetNearest(transform.position);
            if (active != null && active.gameObject != null && active.gameObject.activeInHierarchy)
                return true;

            var notActive = SemiFunc.ExtractionPointGetNearestNotActivated(transform.position);
            if (notActive != null && notActive.gameObject != null)
                return true;
        }
        catch { }

        return false;
    }

    private void UpdateProtecting()
    {
        if (!TasiaBotFriendsPlugin.TryGetPlayerTransform(out var player))
        { SetState(TasiaState.Idle); return; }

        var enemy = TasiaBotWeaponUser.FindNearestEnemy(player.position, 15f, out var enemyDist);
        if (!enemy || enemyDist > 15f)
        { SetState(_followPlayer ? TasiaState.Following : TasiaState.Idle); return; }

        var towardEnemy = (enemy.position - player.position).FlatY();
        if (towardEnemy.sqrMagnitude < 0.01f) towardEnemy = player.forward.FlatY();

        if (NeedNewPath())
        {
            var desired = player.position + towardEnemy.normalized * Mathf.Clamp(enemyDist * 0.45f, 3f, 5f);
            SetDestinationSafe(desired, player.position);
        }
    }

    private void UpdateWaiting()
    {
        if (Time.time >= _stayUntil)
        {
            if (HasPendingCommand) HasPendingCommand = false;
            SetState(_followPlayer ? TasiaState.Following : TasiaState.Idle);
            return;
        }
        if (_agent.hasPath) _agent.ResetPath();
    }

    private void UpdateHelping()
    {
        if (!TasiaBotFriendsPlugin.TryGetPlayerTransform(out var player))
        { SetState(TasiaState.Idle); return; }

        var dist = Vector3.Distance(transform.position, player.position);
        if (dist <= 2.5f)
        {
            if (_agent.hasPath) _agent.ResetPath();
            if (!IsInStateFor(5f)) return;
            if (HasPendingCommand) HasPendingCommand = false;
            SetState(_followPlayer ? TasiaState.Following : TasiaState.Idle);
            return;
        }

        if (NeedNewPath())
            SetDestinationSafe(player.position, player.position);
    }

    private void UpdateIdle()
    {
        // Decide what to do autonomously
        if (_carrier && _carrier.IsCarrying)
        { SetState(TasiaState.CarryingLoot); return; }

        var enemy = TasiaBotWeaponUser.FindNearestEnemy(transform.position, 15f, out var enemyDist);
        if (enemy && enemyDist < 10f)
        {
            SetState(HasGun() ? TasiaState.Protecting : TasiaState.Fleeing);
            return;
        }

        if (_carrier && _carrier.HasTarget)
        { SetState(TasiaState.GoingToLoot); return; }

        // Only search for loot if a delivery target exists
        if (HasDeliveryTarget())
        {
            var nearestLoot = TasiaBotCarrier.FindNearestValuableGlobal(transform.position, 20f);
            if (nearestLoot)
            { SetState(TasiaState.Searching); return; }
        }
        else
        {
            // No extraction available. Explore to find one or stay with Marty.
            if (NeedNewPath() && TryPickLevelTargetNear(transform.position, 14f, out var exploreTarget))
            {
                SetDestinationSafe(exploreTarget, transform.position);
                return;
            }
        }

        // Nothing to do, wait
        if (_agent.hasPath) _agent.ResetPath();
    }

    // ══════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════
    private bool HasGun()
    {
        return _weapon && _weapon.HasGun;
    }

    private bool NeedNewPath()
    {
        return _retargetTimer <= 0f || !_agent.hasPath ||
               (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.3f);
    }

    private void UpdateBadPath()
    {
        if (_agent.hasPath && !_agent.pathPending && _agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            _badPathTimer += Time.deltaTime;
            if (_badPathTimer > 1.25f)
            {
                _badPathTimer = 0f;
                _agent.ResetPath();
                _retargetTimer = 0f;
            }
        }
        else _badPathTimer = 0f;
    }

    private void TrackStuck()
    {
        var moved = Vector3.Distance(transform.position, _lastPos);
        _stuckTimer = moved < 0.08f ? _stuckTimer + Time.deltaTime : 0f;
        _lastPos = transform.position;

        bool carrying = _carrier != null && _carrier.IsCarrying;

        // ── Carry-specific stuck recovery ──
        if (carrying && _stuckTimer >= TasiaBotFriendsPlugin.CarryStuckTime)
        {
            _stuckTimer = 0f;
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaCarry] Stuck while carrying! Trying to recover.");

            // 1. Try backing up — smoothly, not teleport
            if (_agent && _agent.enabled && _agent.isOnNavMesh)
            {
                var backupTarget = transform.position - transform.forward.FlatY().normalized * 2f;
                if (NavMesh.SamplePosition(backupTarget, out var hit, 3f, -1))
                {
                    TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMove] Backing up slowly from stuck.");
                    _agent.SetDestination(hit.position);
                    _agent.speed = _speed * 0.4f; // slower backup speed

                    // Rotate slightly after a short delay
                    StartCoroutine(BackupAndRotate(0.6f));
                    _retargetTimer = 0f;
                    return;
                }
            }

            // If no valid navmesh backup point, try a more permissive one
            if (_agent && _agent.enabled && _agent.isOnNavMesh)
            {
                var anyBackup = transform.position + Vector3.back * 2f;
                if (NavMesh.SamplePosition(anyBackup, out var hit2, 5f, -1))
                {
                    _agent.SetDestination(hit2.position);
                    _agent.speed = _speed * 0.4f;
                    StartCoroutine(BackupAndRotate(0.6f));
                    _retargetTimer = 0f;
                    return;
                }
            }

            // 2. If stuck for too long after backup attempt, place item safe
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaCarry] Cannot recover from stuck. Placing item safe.");
            _carrier?.DropNowExternal();
            ExternalControl = false;
            if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();

            var msg = "Utknęłam, odkładam to.";
            TasiaBotFriendsPlugin.Instance?.ShowBubble(gameObject, msg);
            return;
        }

        if (_stuckTimer < 2f) return;
        var wasStuckLong = _stuckTimer > 8f;
        _stuckTimer = 0f;
        ExternalControl = false;

        if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();

        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMove] Stuck, retargeting to player...");

        if (!TasiaBotFriendsPlugin.TryGetPlayerTransform(out var player)) return;
        var dist = Vector3.Distance(transform.position, player.position);

        if (dist > 5f)
        {
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaMove] Stuck far ({dist:F0}m), pathing to player.");
            // Teleport if far OR stuck longer than 8s
            if (dist > 12f || wasStuckLong)
            {
                SafeTeleportToPlayer();
                return;
            }
            SetDestinationSafe(player.position, player.position);
        }
        else
        {
            SetDestinationSafe(player.position, player.position);
        }
    }

    private IEnumerator BackupAndRotate(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_agent)
        {
            transform.rotation *= Quaternion.Euler(0f, Random.Range(-25f, 25f), 0f);
            _agent.speed = _speed; // restore normal speed
        }
    }

    private void SetDestinationSafe(Vector3 desired, Vector3 fallback)
    {
        if (!_agent || !_agent.enabled || !_agent.isOnNavMesh) return;
        var target = desired;
        if (NavMesh.SamplePosition(desired, out var hit, 4f, -1)) target = hit.position;
        else if (NavMesh.SamplePosition(fallback, out hit, 6f, -1)) target = hit.position;

        var path = new NavMeshPath();
        if (_agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete)
            _agent.SetDestination(target);
        else if (NavMesh.SamplePosition(fallback, out hit, 6f, -1))
            _agent.SetDestination(hit.position);
    }

    private Vector3 GetFormationPosition(Transform player)
    {
        var right = player.right.FlatY();
        var back = (-player.forward).FlatY();
        if (right.sqrMagnitude < 0.01f) right = Vector3.right;
        if (back.sqrMagnitude < 0.01f) back = Vector3.back;
        return player.position + right.normalized * _formationOffset.x + back.normalized * Mathf.Abs(_formationOffset.z);
    }

    private void MoveAwayFromPlayer(Transform player)
    {
        if (!_agent || !_agent.enabled || !_agent.isOnNavMesh) return;
        var away = (transform.position - player.position).FlatY();
        if (away.sqrMagnitude < 0.05f) away = (-player.forward).FlatY();
        if (away.sqrMagnitude < 0.05f) away = Vector3.back;
        SetDestinationSafe(player.position + away.normalized * 3.2f, player.position + away.normalized * 2.5f);
    }

    private bool EnforcePersonalSpace()
    {
        if (!TasiaBotFriendsPlugin.TryGetPlayerTransform(out var player)) return false;
        var dist = Vector3.Distance(transform.position, player.position);
        
        // 1. Too close to player
        if (dist < 1.5f)
        {
            MoveAwayFromPlayer(player);
            _retargetTimer = Mathf.Max(_retargetTimer, 0.8f);
            return true;
        }

        // 2. In front of player within 3m
        if (dist < 3f)
        {
            var toTasia = (transform.position - player.position).FlatY().normalized;
            var playerFwd = player.forward.FlatY().normalized;
            if (Vector3.Dot(toTasia, playerFwd) > 0.3f)
            {
                // Tasia is in front cone - sidestep
                Sidestep(player);
                return true;
            }
        }

        // 3. Blocking path / corridor
        if (IsBlockingPath(player))
        {
            BackUp();
            return true;
        }

        return false;
    }

    private void Sidestep(Transform player)
    {
        if (!_agent || !_agent.enabled || !_agent.isOnNavMesh) return;
        // Move to side of player
        var right = player.right.FlatY().normalized;
        var side = Random.value < 0.5f ? right : -right;
        var target = player.position + side * 2.5f + (-player.forward.FlatY().normalized) * 1.5f;
        SetDestinationSafe(target, player.position);
    }

    private void BackUp()
    {
        if (!_agent || !_agent.enabled || !_agent.isOnNavMesh) return;
        var back = (-transform.forward).FlatY().normalized * 2f + transform.position;
        SetDestinationSafe(back, transform.position);
    }

    private void SafeTeleportToPlayer()
    {
        if (!TasiaBotFriendsPlugin.TryGetPlayerTransform(out var player)) return;
        if (!_agent || !_agent.enabled) return;

        // Try to find a safe position around player
        var pos = FindSafeTeleportPosition(player);
        if (pos == null) return;

        TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaMove] Teleport reason: stuck/far to player pos");

        // Warn before teleport
        var bubble = GetComponent<TasiaBotBrain>() != null;
        TasiaBotFriendsPlugin.Instance?.ShowBubble(gameObject, "Coming to you.");

        // Teleport
        var navPos = pos.Value;
        if (NavMesh.SamplePosition(navPos, out var hit, 3f, -1))
            _agent.Warp(hit.position);
        else
            _agent.Warp(navPos);

        // After teleport, wait and orient
        _retargetTimer = 1f;
    }

    private Vector3? FindSafeTeleportPosition(Transform player)
    {
        var playerFwd = player.forward.FlatY().normalized;
        var playerRight = player.right.FlatY().normalized;
        if (playerFwd.sqrMagnitude < 0.01f) playerFwd = Vector3.forward;
        if (playerRight.sqrMagnitude < 0.01f) playerRight = Vector3.right;

        // Try positions: left-back, right-back, behind
        var slots = new Vector3[]
        {
            player.position + (-playerFwd + playerRight).normalized * 4f + Vector3.up * 0.2f,  // right-back
            player.position + (-playerFwd - playerRight).normalized * 4f + Vector3.up * 0.2f,  // left-back
            player.position + (-playerFwd).normalized * 4.5f + Vector3.up * 0.2f,              // behind
        };

        foreach (var slot in slots)
        {
            // Validate: not in front cone, not too close
            var toSlot = (slot - player.position).FlatY().normalized;
            if (Vector3.Dot(toSlot, playerFwd) > 0.1f)
                continue; // in front of player, skip

            // Must have NavMesh
            if (!NavMesh.SamplePosition(slot, out var hit, 2f, -1))
                continue;

            // Must have clear space
            if (Physics.CheckSphere(hit.position, 0.8f))
                continue;

            return hit.position;
        }

        return null;
    }

    private bool IsBlockingPath(Transform player)
    {
        // Check if Tasia is directly between player and something
        var toPlayer = (player.position - transform.position).FlatY();
        var toTasia = (transform.position - player.position).FlatY();
        var playerFwd = player.forward.FlatY().normalized;
        
        // Check if player is looking at Tasia from close
        if (toPlayer.sqrMagnitude < 16f && Vector3.Dot(playerFwd, toTasia.normalized) > 0.2f)
        {
            // Player is looking this way and Tasia is in the way
            // Also check if there's a NavMesh edge near (doorway)
            if (!NavMesh.SamplePosition(transform.position + playerFwd * 1.5f, out _, 1f, -1))
                return true; // Near navmesh edge = doorway/corridor
        }
        return false;
    }

    // ══════════════════════════════════════════════════════
    //  CARRY OBSTACLE CHECK
    // ══════════════════════════════════════════════════════
    private bool CarryObstacleCheck()
    {
        if (_carrier == null || !_carrier.IsCarrying) return false;

        var checkDist = TasiaBotFriendsPlugin.CarryCheckDist;
        var start = _hold ? _hold.position : transform.position + Vector3.up * 0.8f;
        var forward = transform.forward.FlatY();
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;

        // Check forward ray
        if (Physics.Raycast(start, forward, out var hit, checkDist))
        {
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaCarry] Obstacle ahead: {hit.collider?.name} at {hit.distance:F1}m");

            // If too close, place item safe
            if (hit.distance < 0.6f && _carrier.IsCarrying)
            {
                TasiaBotFriendsPlugin.Log.LogInfo("[TasiaCarry] Obstacle too close! Placing item safe.");
                _carrier.DropSafe();
                return true; // blocked
            }

            // Slow down by setting lower speed
            _agent.speed = _speed * TasiaBotFriendsPlugin.CarrySpeedMul * 0.6f;
            return true; // obstacle present but not blocking fully
        }

        // Check side rays for doorway width
        var right = Vector3.Cross(Vector3.up, forward).normalized * 0.35f;
        if (Physics.Raycast(start + right, forward, out _, checkDist * 0.7f) ||
            Physics.Raycast(start - right, forward, out _, checkDist * 0.7f))
        {
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaCarry] Narrow passage detected.");
            return true;
        }

        return false;
    }

    private bool TryReachablePoint(Vector3 desired, Vector3 fallback, out Vector3 target)
    {
        target = desired;
        if (!_agent || !_agent.enabled || !_agent.isOnNavMesh) return false;
        if (!NavMesh.SamplePosition(desired, out var hit, 3.5f, -1)) return false;
        if (Vector3.Distance(hit.position, fallback) < 2.1f) return false;
        var path = new NavMeshPath();
        if (!_agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete) return false;
        target = hit.position;
        return true;
    }

    private static void RefreshLevelTargetsIfNeeded()
    {
        if (Time.time < _nextLevelTargetRefresh && LevelTargets.Count > 0) return;
        _nextLevelTargetRefresh = Time.time + 8f;
        LevelTargets.Clear();

        try
        {
            if (LevelGenerator.Instance?.LevelPathPoints != null)
                foreach (var point in LevelGenerator.Instance.LevelPathPoints)
                    if (point && NavMesh.SamplePosition(point.transform.position, out var hit, 3f, -1))
                        LevelTargets.Add(hit.position);
        }
        catch { }

        if (LevelTargets.Count > 0) return;

        try
        {
            var vertices = NavMesh.CalculateTriangulation().vertices;
            var step = Mathf.Max(1, vertices.Length / 80);
            for (var i = 0; i < vertices.Length; i += step)
                if (NavMesh.SamplePosition(vertices[i], out var hit, 3f, -1))
                    LevelTargets.Add(hit.position);
        }
        catch { }
    }

    private static bool TryPickLevelTargetNear(Vector3 center, float radius, out Vector3 target)
    {
        target = default;
        RefreshLevelTargetsIfNeeded();
        if (LevelTargets.Count == 0) return false;

        var radiusSqr = radius * radius;
        var best = float.MaxValue;
        var found = false;
        foreach (var cand in LevelTargets)
        {
            var sqr = (cand - center).sqrMagnitude;
            if (sqr > radiusSqr) continue;
            var score = sqr + Random.Range(0f, 4f);
            if (score < best) { best = score; target = cand; found = true; }
        }
        return found;
    }
}

// ═══════════════════════════════════════════════════════════
//  TASIA PERCEPTION – what Tasia sees around her
// ═══════════════════════════════════════════════════════════
internal class TasiaPerception
{
    public  Transform  PlayerTransform;
    public  float      PlayerDistance;
    public  bool       PlayerVisible;

    public  List<VisibleLoot>   LootList   = new();
    public  List<VisibleEnemy>  EnemyList  = new();
    public  List<Transform>      Teammates  = new();
    public  bool       ExtractionPointFound;
    public  Vector3    ExtractionPosition;
    public  float      ExtractionDistance;
    public  bool       CarryingItem;
    public  string     CarriedItemType = "";
    public  float      CarriedItemValue;
    public  bool       CarriedItemFragile;
    public  string     CurrentRoom = "unknown";
    public  string     DangerLevel = "low"; // low, medium, high
    public  Vector3    LastKnownSafePosition;
    public  float      LastPerceptionTime;

    public struct VisibleLoot
    {
        public Vector3  Position;
        public float    Value;
        public bool     Fragile;
        public string   TypeName;
        public float    Distance;
    }

    public struct VisibleEnemy
    {
        public Vector3  Position;
        public float    Distance;
        public bool     IsActive;
    }

    public void Refresh(GameObject bot, float scanRadius)
    {
        LastPerceptionTime = Time.time;
        Teammates.Clear();
        LootList.Clear();
        EnemyList.Clear();

        if (TasiaBotFriendsPlugin.TryGetPlayerTransform(out var player))
        {
            PlayerTransform = player;
            PlayerDistance = Vector3.Distance(bot.transform.position, player.position);
            PlayerVisible = PlayerDistance < 20f;
        }

        // Scan enemies
        foreach (var enemy in Object.FindObjectsOfType<Enemy>())
        {
            if (!enemy || !enemy.gameObject.activeInHierarchy) continue;
            var dist = Vector3.Distance(bot.transform.position, enemy.transform.position);
            if (dist > scanRadius * 1.5f) continue;
            EnemyList.Add(new VisibleEnemy
            {
                Position = enemy.transform.position,
                Distance = dist,
                IsActive = true,
            });
        }

        // Danger level
        if (EnemyList.Exists(e => e.Distance < 8f))
            DangerLevel = "high";
        else if (EnemyList.Exists(e => e.Distance < 15f))
            DangerLevel = "medium";
        else
            DangerLevel = "low";

        // Scan loot
        foreach (var pgo in Object.FindObjectsOfType<PhysGrabObject>())
        {
            if (!pgo || !pgo.gameObject.activeInHierarchy) continue;
            var dist = Vector3.Distance(bot.transform.position, pgo.transform.position);
            if (dist > scanRadius * 1.5f) continue;
            if (IsGrabbedOrInside(pgo)) continue;

            var valuable = pgo.GetComponent<ValuableObject>();
            if (!valuable) continue;

            LootList.Add(new VisibleLoot
            {
                Position = pgo.transform.position,
                Value = GetValuableValue(valuable),
                Fragile = IsFragile(valuable),
                TypeName = valuable.name ?? "item",
                Distance = dist,
            });
        }

        // Extraction point
        try
        {
            var ep = SemiFunc.ExtractionPointGetNearest(bot.transform.position);
            if (ep)
            {
                ExtractionPointFound = true;
                ExtractionPosition = ep.transform.position;
                ExtractionDistance = Vector3.Distance(bot.transform.position, ep.transform.position);
            }
            else
            {
                ExtractionPointFound = false;
            }
        }
        catch
        {
            ExtractionPointFound = false;
        }

        // Current room approximation from LevelPathPoints
        try
        {
            if (LevelGenerator.Instance?.LevelPathPoints != null)
            {
                var nearestDist = float.MaxValue;
                LevelPoint nearest = null;
                foreach (var pt in LevelGenerator.Instance.LevelPathPoints)
                {
                    if (!pt) continue;
                    var d = Vector3.Distance(bot.transform.position, pt.transform.position);
                    if (d < nearestDist) { nearestDist = d; nearest = pt; }
                }
                if (nearest != null && nearestDist < 15f)
                    CurrentRoom = nearest.Room?.name ?? nearest.name ?? "room";
                else
                    CurrentRoom = "corridor";
            }
        }
        catch { CurrentRoom = "unknown"; }

        // Carried item info
        var carrier = bot.GetComponent<TasiaBotCarrier>();
        if (carrier && carrier.IsCarrying)
        {
            CarryingItem = true;
            CarriedItemType = carrier.CarriedItemName ?? "item";
            CarriedItemValue = carrier.CarriedItemValue;
            CarriedItemFragile = carrier.CarriedItemFragile;
        }
        else
        {
            CarryingItem = false;
            CarriedItemType = "";
            CarriedItemValue = 0;
            CarriedItemFragile = false;
        }

        if (DangerLevel != "high")
            LastKnownSafePosition = bot.transform.position;
    }

    private static float GetValuableValue(ValuableObject vo)
    {
        if (!vo) return 0;
        try
        {
            var type = typeof(ValuableObject);
            var f = type.GetField("dollarValueCurrent") ?? type.GetField("dollarValue") ?? type.GetField("value");
            if (f != null) return (float)Convert.ChangeType(f.GetValue(vo), typeof(float));
            return 0;
        }
        catch { return 0; }
    }

    private static bool IsFragile(ValuableObject vo)
    {
        if (!vo) return false;
        try
        {
            var type = typeof(ValuableObject);
            var f = type.GetField("health") ?? type.GetField("durability") ?? type.GetField("hp");
            if (f != null) return (int)Convert.ChangeType(f.GetValue(vo), typeof(int)) <= 3;
            return false;
        }
        catch { return false; }
    }

    private static bool IsGrabbedOrInside(PhysGrabObject pgo)
    {
        try
        {
            if (SemiFunc.PhysGrabObjectIsGrabbed(pgo)) return true;
            var ep = SemiFunc.ExtractionPointGetNearest(pgo.transform.position);
            return ep && Vector3.Distance(pgo.transform.position, ep.transform.position) <= 1.25f;
        }
        catch { return false; }
    }
}

// ═══════════════════════════════════════════════════════════
//  TASIA WORLD MEMORY – remembers what she saw
// ═══════════════════════════════════════════════════════════
internal class TasiaWorldMemory
{
    public struct MemoryLoot
    {
        public Vector3  Position;
        public float    Value;
        public bool     Fragile;
        public string   TypeName;
        public float    LastSeenTime;
        public string   RoomName;
    }

    public struct MemoryMonster
    {
        public Vector3  Position;
        public float    LastKnownTime;
    }

    public struct MemoryRoom
    {
        public Vector3  Position;
        public string   Name;
        public bool     HasExtractor;
        public bool     HasLoot;
    }

    public readonly List<MemoryLoot>    KnownLoot     = new();
    public readonly List<MemoryMonster> KnownMonsters  = new();
    public readonly List<MemoryRoom>    KnownRooms     = new();
    public Vector3  LastMartyPosition;
    public float    LastMartyTime;
    public Vector3  ExtractionPointPos;
    public bool     ExtractionKnown;
    public float    ExtractionLastSeen;

    private float _lastCleanup;

    public void Update(TasiaPerception perception, float now)
    {
        // Remember loot in range
        foreach (var loot in perception.LootList)
        {
            var idx = KnownLoot.FindIndex(l => Vector3.Distance(l.Position, loot.Position) < 2f);
            if (idx >= 0)
            {
                var existing = KnownLoot[idx];
                existing.LastSeenTime = now;
                KnownLoot[idx] = existing;
            }
            else
            {
                KnownLoot.Add(new MemoryLoot
                {
                    Position = loot.Position,
                    Value = loot.Value,
                    Fragile = loot.Fragile,
                    TypeName = loot.TypeName,
                    LastSeenTime = now,
                    RoomName = perception.CurrentRoom,
                });
            }
        }

        // Remember monsters
        foreach (var enemy in perception.EnemyList)
        {
            var dist = Vector3.Distance(enemy.Position, LastMartyPosition);
            KnownMonsters.Add(new MemoryMonster
            {
                Position = enemy.Position,
                LastKnownTime = now,
            });
        }

        // Marty position
        if (perception.PlayerTransform)
        {
            LastMartyPosition = perception.PlayerTransform.position;
            LastMartyTime = now;
        }

        // Extraction point
        if (perception.ExtractionPointFound)
        {
            ExtractionPointPos = perception.ExtractionPosition;
            ExtractionKnown = true;
            ExtractionLastSeen = now;
        }

        // Current room
        if (!KnownRooms.Exists(r => r.Name == perception.CurrentRoom))
        {
            KnownRooms.Add(new MemoryRoom
            {
                Position = perception.PlayerTransform?.position ?? Vector3.zero,
                Name = perception.CurrentRoom,
                HasExtractor = perception.ExtractionPointFound,
                HasLoot = perception.LootList.Count > 0,
            });
        }

        // Periodic cleanup of old entries
        if (now - _lastCleanup > 30f)
        {
            _lastCleanup = now;
            KnownLoot.RemoveAll(l => now - l.LastSeenTime > 180f);
            KnownMonsters.RemoveAll(m => now - m.LastKnownTime > 120f);
        }
    }

    public string Summarize()
    {
        var parts = new List<string>();

        var lootCount = KnownLoot.Count;
        if (lootCount > 0)
            parts.Add($"znany loot: {lootCount} sztuk");

        var recentLoot = KnownLoot.FindAll(l => Time.time - l.LastSeenTime < 30f);
        if (recentLoot.Count > 0)
            parts.Add($"loot w zasięgu: {recentLoot.Count}");

        var recentMonsters = KnownMonsters.FindAll(m => Time.time - m.LastKnownTime < 30f);
        if (recentMonsters.Count > 0)
            parts.Add($"potwory: {recentMonsters.Count} (ostatni {Mathf.RoundToInt(Time.time - recentMonsters[recentMonsters.Count - 1].LastKnownTime)}s temu)");

        if (ExtractionKnown)
        {
            var dist = PlayerTransform ? Vector3.Distance(PlayerTransform.position, ExtractionPointPos) : 999f;
            parts.Add($"ekstraktor: {Mathf.RoundToInt(dist)}m");
        }

        return string.Join(", ", parts);
    }

    private Transform PlayerTransform
    {
        get
        {
            TasiaBotFriendsPlugin.TryGetPlayerTransform(out var t);
            return t;
        }
    }
}

// ═══════════════════════════════════════════════════════════
//  TASIA CARRIER
// ═══════════════════════════════════════════════════════════
internal sealed class TasiaBotCarrier : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Transform _hold;
    private TasiaBotBrain _brain;
    private float _searchRadius;
    private float _dropDistance;
    private float _retargetTimer;
    private float _busyTimer;
    private static List<int> _deliveredIds = new List<int>();
    private float _dropCooldown;
    private float _lastCleanup;
    private PhysGrabObject _carried;
    private Rigidbody _carriedRb;
    private bool _busy;

    // ── Extraction activation anti-spam ──
    private int _lastActivateId;
    private float _lastActivateTime;
    private float _activationGraceUntil;
    private float _noExtractSince; // when we started waiting for extraction

    internal bool IsCarrying => _carried;
    internal bool HasTarget => !_carried && !_busy;
    internal string CarriedItemName => _carried ? _carried.name ?? "item" : "";
    internal float CarriedItemValue
    {
        get
        {
            if (!_carried) return 0;
            try
            {
                var vo = _carried.GetComponent<ValuableObject>();
                if (!vo) return 0;
                var type = typeof(ValuableObject);
                var f = type.GetField("dollarValueCurrent") ?? type.GetField("dollarValue") ?? type.GetField("value");
                if (f != null) return (float)Convert.ChangeType(f.GetValue(vo), typeof(float));
                return 0;
            }
            catch { return 0; }
        }
    }
    internal bool CarriedItemFragile
    {
        get
        {
            if (!_carried) return false;
            try
            {
                var vo = _carried.GetComponent<ValuableObject>();
                if (!vo) return false;
                var type = typeof(ValuableObject);
                var f = type.GetField("health") ?? type.GetField("durability") ?? type.GetField("hp");
                if (f != null) return (int)Convert.ChangeType(f.GetValue(vo), typeof(int)) <= 3;
                return false;
            }
            catch { return false; }
        }
    }

    internal void Init(NavMeshAgent agent, Transform hold, TasiaBotBrain brain, float searchRadius, float dropDistance)
    {
        _agent = agent;
        _hold = hold;
        _brain = brain;
        _searchRadius = Mathf.Clamp(searchRadius, 2f, 60f);
        _dropDistance = Mathf.Clamp(dropDistance, 0.5f, 4f);
        _retargetTimer = Random.Range(0.5f, 1.3f);
    }

    internal void ForceRetarget() => _retargetTimer = 0f;

    internal void ForceStop()
    {
        if (_carried)
        {
            // Don't drop — just release and reset
            try
            {
                var t = _carried.transform;
                t.SetParent(null, true);
                _carried = null;
                _carriedRb = null;
            }
            catch { }
        }
        _busy = false;
        _busyTimer = 0f;
        _retargetTimer = 0f;
        _noExtractSince = 0f;
    }

    private void Update()
    {
        // HARD GATE: no carrier activity outside gameplay
        if (!TasiaBotFriendsPlugin.IsGameplayReady()) return;

        if (_carried)
        {
            _brain?.SetExternalControl(true);
            KeepHeld();
            Deliver();
            return;
        }

        if (_busy) { _busyTimer += Time.deltaTime; if (_busyTimer > 12f) ResetBusy(); return; }

        _brain?.SetExternalControl(false);
        if (Time.time < _dropCooldown) return;
        if (Time.time - _lastCleanup > 15f) { _lastCleanup = Time.time; _deliveredIds.Clear(); }
        _retargetTimer -= Time.deltaTime;
        if (_retargetTimer > 0f) return;
        _retargetTimer = Random.Range(1f, 1.6f);

        // Check if unarmed and weapon is nearby - prioritize weapon
        var weapon = GetComponent<TasiaBotWeaponUser>();
        if ((weapon == null || !weapon.HasGun) && ShouldPickUpGun())
        {
            var gun = FindNearestGun(transform.position, _searchRadius);
            if (gun != null)
            {
                _busy = true; _busyTimer = 0f; _brain?.SetExternalControl(true);
                if (_agent && _agent.enabled && _agent.isOnNavMesh)
                    _agent.SetDestination(gun.transform.position);
                _agent.stoppingDistance = 0.4f;
                StartCoroutine(ApproachGun(gun));
                return;
            }
        }

        // ── Before picking up loot, verify a delivery target exists ──
        if (!TryResolveActiveExtraction(out var deliveryGoal, out var deliveryEp))
        {
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaLoot] Pickup blocked: no active or activatable extraction found.");
            TasiaBotFriendsPlugin.Instance?.ShowBubble(gameObject, "Nie mam gdzie oddać, zostawiam na razie.");
            return;
        }
        TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaLoot] Delivery target resolved before pickup ({deliveryEp.name}).");

        var target = FindNearestValuableGlobal(transform.position, _searchRadius);
        if (!target || !HasCompletePathTo(target.transform.position)) return;
        _busy = true;
        _busyTimer = 0f;
        _brain?.SetExternalControl(true);
        if (_agent && _agent.enabled && _agent.isOnNavMesh)
        {
            var pos = target.transform.position;
            if (NavMesh.SamplePosition(pos, out var hit, 3f, -1)) _agent.SetDestination(hit.position);
            else _agent.SetDestination(pos);
            _agent.stoppingDistance = 0.3f;
        }
        StartCoroutine(ApproachAndPick(target));
    }

    private IEnumerator ApproachAndPick(PhysGrabObject target)
    {
        var timeout = 9f;
        while (timeout > 0f && target && _agent && _agent.enabled && _agent.isOnNavMesh)
        {
            timeout -= Time.deltaTime;
            if (Vector3.Distance(transform.position, target.transform.position) <= 2.1f) break;
            yield return null;
        }

        if (!target || IsGrabbed(target) || Vector3.Distance(transform.position, target.transform.position) > 2.6f)
        { ResetBusy(); yield break; }

        _carried = target;
        _noExtractSince = 0f; // reset extraction wait timer on pickup
        _carriedRb = target.rb ? target.rb : target.GetComponent<Rigidbody>();
        try
        {
            target.transform.SetParent(_hold, false);
            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;
            if (_carriedRb) { _carriedRb.velocity = Vector3.zero; _carriedRb.angularVelocity = Vector3.zero; _carriedRb.isKinematic = true; }
            target.OverrideKnockOutOfGrabDisable(2f);
        }
        catch { }
        _busy = false;
    }

    private void KeepHeld()
    {
        if (!_carried) return;
        if (_carried.transform.parent != _hold) _carried.transform.SetParent(_hold, false);
        _carried.transform.localPosition = Vector3.zero;
        _carried.transform.localRotation = Quaternion.identity;
    }

    private void Deliver()
    {
        if (!_carried || !_agent || !_agent.enabled || !_agent.isOnNavMesh) return;

        // Resolve current valid extraction target
        if (!TryResolveActiveExtraction(out var goal, out var ep))
        {
            // Don't drop immediately. Stop, wait, recheck, only drop if waited too long.
            if (_noExtractSince <= 0f) _noExtractSince = Time.time;

            if (Time.time - _noExtractSince < 8f)
            {
                // Wait and recheck next frame
                if (_agent.hasPath) _agent.ResetPath();
                TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaExtraction] No extraction, waiting ({(Time.time - _noExtractSince):F0}s)...");
                return;
            }

            // Waited too long — place safe
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaExtraction] No extraction for 8s, placing safe.");
            _noExtractSince = 0f;
            DropSafe();
            TasiaBotFriendsPlugin.Instance?.ShowBubble(gameObject, "Nie widzę extraction, odkładam to.");
            return;
        }

        // Reset timer when we have a valid extraction
        _noExtractSince = 0f;

        // Update world memory with known extraction
        if (TasiaBotFriendsPlugin.TryGetPlayerTransform(out var player) && _brain != null)
        {
            _brain.WorldMemory.ExtractionPointPos = goal;
            _brain.WorldMemory.ExtractionKnown = true;
            _brain.WorldMemory.ExtractionLastSeen = Time.time;
        }

        if (!_agent.hasPath || Vector3.SqrMagnitude(_agent.destination - goal) > 0.5f)
        {
            if (NavMesh.SamplePosition(goal, out var hit, 4f, -1) && HasCompletePathTo(hit.position))
                _agent.SetDestination(hit.position);
            else
            {
                TasiaBotFriendsPlugin.Log.LogInfo("[TasiaExtraction] Cannot path to extraction, placing safe.");
                DropSafe();
                return;
            }
        }

        // At delivery point: try activation if not active, then drop item
        var distToExtraction = Vector3.Distance(transform.position, goal);
        if (distToExtraction <= _dropDistance + 0.35f)
        {
            // If this extraction isn't active yet, activate it
            if (ep != null && !ep.gameObject.activeInHierarchy)
                TryActivateExtraction(ep);

            if (CarriedItemFragile && TasiaBotFriendsPlugin.FragileCareOn)
                DropSafe();
            else
                DropNow();

            if (ep != null)
                TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaExtraction] Delivered to extraction '{ep.name}'.");
        }
        // Try activation from a bit further away too, to give grace distance
        else if (ep != null && distToExtraction <= _dropDistance + 2f && !ep.gameObject.activeInHierarchy)
        {
            TryActivateExtraction(ep);
        }
    }

    /// <summary>Find the best active extraction point — if none, find nearest not-activated one.</summary>
    private bool IsExtractionFull(ExtractionPoint ep)
    {
        if (ep == null) return false;
        try
        {
            // Check if extraction reached its money limit
            var fieldValue = ep.GetType().GetField("totalMoneyExtracted");
            if (fieldValue != null)
            {
                var total = (int)Convert.ChangeType(fieldValue.GetValue(ep), typeof(int));
                var maxField = ep.GetType().GetField("maxMoney");
                var max = maxField != null ? (int)Convert.ChangeType(maxField.GetValue(ep), typeof(int)) : 0;
                if (max > 0 && total >= max) return true;
            }
            // Also check completed state
            var completedField = ep.GetType().GetField("completed");
            if (completedField != null)
            {
                var completed = (bool)completedField.GetValue(ep);
                if (completed) return true;
            }
        }
        catch { }
        return false;
    }

    internal bool TryResolveActiveExtraction(out Vector3 goal, out ExtractionPoint ep)
    {
        goal = transform.position;
        ep = null;

        try
        {
            // 1. Nearest active extraction
            var nearest = SemiFunc.ExtractionPointGetNearest(transform.position);
            if (nearest != null && nearest.gameObject != null && nearest.gameObject.activeInHierarchy)
            {
                // Check if extraction is full
                if (IsExtractionFull(nearest))
                {
                    TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaExtraction] Extraction '{nearest.name}' is full, searching for another.");
                }
                else
                {
                    ep = nearest;
                    goal = ep.transform.position;
                    TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaExtraction] Active extraction '{ep.name}' at {Vector3.Distance(transform.position, goal):F0}m.");
                    return true;
                }
            }

            // 2. Nearest not-activated extraction (needs activation)
            var notActivated = SemiFunc.ExtractionPointGetNearestNotActivated(transform.position);
            if (notActivated != null && notActivated.gameObject != null)
            {
                ep = notActivated;
                goal = ep.transform.position;
                TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaExtraction] Not-activated extraction '{ep.name}' at {Vector3.Distance(transform.position, goal):F0}m. Will activate on arrival.");
                return true;
            }

            // 3. Fallback: scan all extraction points in scene
            var allExtractions = Object.FindObjectsOfType<ExtractionPoint>();
            ExtractionPoint fallback = null;
            float fallbackDist = float.MaxValue;

            foreach (var ext in allExtractions)
            {
                if (ext == null || ext.gameObject == null) continue;
                var d = Vector3.Distance(transform.position, ext.transform.position);
                if (d < fallbackDist)
                {
                    fallbackDist = d;
                    fallback = ext;
                }
            }

            if (fallback != null)
            {
                ep = fallback;
                goal = ep.transform.position;
                TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaExtraction] Fallback extraction '{ep.name}' at {fallbackDist:F0}m.");
                return true;
            }

            // 4. Nothing found
            if (TasiaBotFriendsPlugin.VerboseLogging)
                TasiaBotFriendsPlugin.Log.LogInfo("[TasiaExtraction] No extraction point found in scene.");
            return false;
        }
        catch (Exception ex)
        {
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaExtraction] Resolution error: {ex.Message}");
            return false;
        }
    }

    /// <summary>Activate an extraction point if not already active.</summary>
    private void TryActivateExtraction(ExtractionPoint ep)
    {
        if (ep == null) return;
        int id = ep.GetInstanceID();
        if (_lastActivateId == id && Time.time - _lastActivateTime < 0.75f) return;
        _lastActivateId = id;
        _lastActivateTime = Time.time;
        _activationGraceUntil = Time.time + 2f;
        try
        {
            ep.ButtonPress();
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaExtraction] Activated extraction '{ep.name}'.");
        }
        catch (Exception ex)
        {
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaExtraction] Activate failed: {ex.Message}");
        }
    }

    internal void DropNowExternal() => DropNow();

    internal void DropSafe()
    {
        if (!_carried) { ResetBusy(); return; }
        try
        {
            var t = _carried.transform;
            t.SetParent(null, true);

            // Place gently on ground below hold point
            var start = _hold ? _hold.position : t.position;
            if (Physics.Raycast(start + Vector3.up * 0.3f, Vector3.down, out var hit, 2f))
                t.position = hit.point + Vector3.up * 0.05f;
            else
                t.position = start + Vector3.down * 0.5f;

            t.rotation = Quaternion.identity;

            if (_carriedRb)
            {
                _carriedRb.isKinematic = false;
                _carriedRb.detectCollisions = true;
                _carriedRb.velocity = Vector3.zero;
                _carriedRb.angularVelocity = Vector3.zero;
            }

            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaCarry] Item placed safely on ground.");
        }
        catch (Exception ex)
        {
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaCarry] Safe place failed: {ex.Message}");
        }
        _carried = null; _carriedRb = null;
        ResetBusy();
        if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
        TasiaBotFriendsPlugin.Instance?.ShowBubble(gameObject, "Odkładam to bezpiecznie.");
    }

    private void DropNow()
    {
        _dropCooldown = Time.time + 1.5f;
        if (_carried) { _deliveredIds.Add(_carried.GetInstanceID()); _lastCleanup = Time.time; }
        if (!_carried) { ResetBusy(); return; }
        try
        {
            var t = _carried.transform;
            t.SetParent(null, true);
            t.position += transform.forward.FlatY().normalized * 0.8f + Vector3.down * 0.15f;
            if (_carriedRb) { _carriedRb.isKinematic = false; _carriedRb.detectCollisions = true; _carriedRb.velocity = Vector3.zero; _carriedRb.angularVelocity = Vector3.zero; }
        }
        catch { }
        _carried = null; _carriedRb = null;
        ResetBusy();
        if (_agent && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
    }

    private void ResetBusy() { _busy = false; _busyTimer = 0f; _brain?.SetExternalControl(false); }

    internal bool HasCompletePathTo(Vector3 target)
    {
        if (!_agent || !_agent.enabled || !_agent.isOnNavMesh) return false;
        if (!NavMesh.SamplePosition(target, out var hit, 4f, -1)) return false;
        var path = new NavMeshPath();
        return _agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    private bool ShouldPickUpGun()
    {
        // Only if guns enabled in config
        if (!TasiaBotFriendsPlugin.Instance.EnableWeaponsCfg) return false;
        // Only if safe
        if (PerceptionDangerLevel() == "high") return false;
        return true;
    }

    private string PerceptionDangerLevel()
    {
        if (_brain?.Perception != null) return _brain.Perception.DangerLevel;
        return "low";
    }

    private System.Collections.IEnumerator ApproachGun(ItemGun g)
    {
        float time = 6f;
        while (time > 0f && g != null && _agent != null && _agent.enabled && _agent.isOnNavMesh)
        {
            time -= Time.deltaTime;
            if (Vector3.Distance(transform.position, g.transform.position) <= 1.9f) break;
            yield return null;
        }
        if (g == null || Vector3.Distance(transform.position, g.transform.position) > 2.4f)
        { ResetBusy(); yield break; }
        
        var w = GetComponent<TasiaBotWeaponUser>();
        if (w != null)
        {
            w.EquipExternal(g);
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaCarry] Picked up gun.");
        }
        _busy = false; _busyTimer = 0f; _brain?.SetExternalControl(false);
    }

    private static ItemGun FindNearestGun(Vector3 from, float radius)
    {
        ItemGun best = null; var bestSqr = radius * radius;
        foreach (var g in Object.FindObjectsOfType<ItemGun>())
        {
            if (g == null || !g.gameObject.activeInHierarchy) continue;
            var pgo = g.GetComponent<PhysGrabObject>();
            if (pgo != null) { try { if (SemiFunc.PhysGrabObjectIsGrabbed(pgo)) continue; } catch { } }
            var sqr = (g.transform.position - from).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = g; }
        }
        return best;
    }

    internal static PhysGrabObject FindNearestValuableGlobal(Vector3 from, float radius)
    {
        PhysGrabObject best = null;
        var bestSqr = radius * radius;
        foreach (var pgo in Object.FindObjectsOfType<PhysGrabObject>())
        {
            if (!pgo || !pgo.gameObject.activeInHierarchy || IsGrabbed(pgo) || !pgo.GetComponent<ValuableObject>()) continue;
            if (IsInsideExtractor(pgo.transform)) continue;
            if (_deliveredIds.Contains(pgo.GetInstanceID())) continue;
            var sqr = (pgo.transform.position - from).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = pgo; }
        }
        return best;
    }

    private static bool IsGrabbed(PhysGrabObject pgo) { try { return SemiFunc.PhysGrabObjectIsGrabbed(pgo); } catch { return false; } }
    private static bool IsInsideExtractor(Transform t) { try { var ep = SemiFunc.ExtractionPointGetNearest(t.position); return ep && Vector3.Distance(t.position, ep.transform.position) <= 1.25f; } catch { return false; } }

    private static bool IsInGameLevel()
    {
        try { return LevelGenerator.Instance != null && LevelGenerator.Instance.Generated; }
        catch { return false; }
    }
}

// ═══════════════════════════════════════════════════════════
//  TASIA WEAPON USER
// ═══════════════════════════════════════════════════════════
internal sealed class TasiaBotWeaponUser : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Transform _hold;
    private TasiaBotBrain _brain;
    private float _searchRadius;
    private float _retargetTimer;
    private float _fireTimer;
    private bool _busy;
    private bool _giveStarterGun;
    private bool _starterAttempted;
    private ItemGun _gun;
    private PhysGrabObject _pgo;
    private Rigidbody _rb;

    internal bool HasGun => _gun;

    internal void Init(NavMeshAgent agent, Transform hold, TasiaBotBrain brain, float searchRadius, bool giveStarterGun)
    {
        _agent = agent; _hold = hold; _brain = brain;
        _searchRadius = Mathf.Clamp(searchRadius, 2f, 60f);
        _giveStarterGun = giveStarterGun;
        _retargetTimer = Random.Range(0.2f, 0.8f);
        _fireTimer = Random.Range(0.4f, 1f);
        if (_giveStarterGun) StartCoroutine(DelayedStarterGun());
    }

    internal void ForceRetarget() => _retargetTimer = 0f;
    private void OnDisable() => DropGun();

    private void Update()
    {
        // HARD GATE: no weapon activity outside gameplay
        if (!TasiaBotFriendsPlugin.IsGameplayReady()) return;

        if (_gun) { _brain?.SetExternalControl(true); KeepGunHeld(); AimAndShoot(); return; }
        if (_busy) return;

        if (_giveStarterGun && !_starterAttempted) { _starterAttempted = true; TryCreateStarterGun(); if (_gun) return; }

        _retargetTimer -= Time.deltaTime;
        if (_retargetTimer > 0f) return;
        _retargetTimer = Random.Range(1f, 1.6f);

        var gun = FindNearestGun(transform.position, _searchRadius);
        if (!gun) return;
        _busy = true;
        _brain?.SetExternalControl(true);
        if (_agent && _agent.enabled && _agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(gun.transform.position, out var hit, 3f, -1)) _agent.SetDestination(hit.position);
            else _agent.SetDestination(gun.transform.position);
            _agent.stoppingDistance = 0.4f;
        }
        StartCoroutine(ApproachAndPick(gun));
    }

    private IEnumerator ApproachAndPick(ItemGun target)
    {
        var timeout = 8f;
        while (timeout > 0f && target && _agent && _agent.enabled && _agent.isOnNavMesh)
        {
            timeout -= Time.deltaTime;
            if (Vector3.Distance(transform.position, target.transform.position) <= 1.9f) break;
            yield return null;
        }
        if (!target || Vector3.Distance(transform.position, target.transform.position) > 2.4f)
        { _busy = false; _brain?.SetExternalControl(false); yield break; }
        try { EquipGun(target); } catch { }
        _busy = false;
    }

    private IEnumerator DelayedStarterGun() { yield return new WaitForSeconds(1.5f); if (!_gun) TryCreateStarterGun(); }

    private bool TryCreateStarterGun()
    {
        _starterAttempted = true;
        if (_gun || !_hold) return false;
        try
        {
            ItemGun source = null;
            foreach (var gun in Resources.FindObjectsOfTypeAll<ItemGun>())
            { if (!gun) continue; if (gun.gameObject.scene.IsValid() && gun.gameObject.activeInHierarchy) continue; source = gun; break; }
            if (!source)
            { foreach (var gun in Object.FindObjectsOfType<ItemGun>()) { if (!gun || !gun.gameObject.activeInHierarchy) continue; source = gun; break; } }
            if (!source) { TasiaBotFriendsPlugin.Log.LogInfo("[Tasia] Starter gun skipped: no ItemGun loaded"); return false; }

            var clone = Object.Instantiate(source.gameObject, _hold.position, _hold.rotation);
            clone.name = "TasiaStarterGun"; clone.SetActive(true);
            var gunComp = clone.GetComponent<ItemGun>() ?? clone.GetComponentInChildren<ItemGun>(true);
            if (!gunComp) { Object.Destroy(clone); return false; }
            EquipGun(gunComp);
            TasiaBotFriendsPlugin.Log.LogInfo($"[Tasia] Starter gun: {source.name}");
            return true;
        }
        catch (Exception ex) { TasiaBotFriendsPlugin.Log.LogInfo($"[Tasia] Starter gun: {ex.Message}"); return false; }
    }

internal void EquipExternal(ItemGun t) { EquipGun(t); }
    private void EquipGun(ItemGun target)
    {
        _gun = target; _pgo = target.GetComponent<PhysGrabObject>(); _rb = _pgo && _pgo.rb ? _pgo.rb : target.GetComponent<Rigidbody>();
        target.transform.SetParent(_hold, false); target.transform.localPosition = Vector3.zero; target.transform.localRotation = Quaternion.identity;
        if (_rb) { _rb.velocity = Vector3.zero; _rb.angularVelocity = Vector3.zero; _rb.isKinematic = true; } _busy = false;
    }

    private void KeepGunHeld()
    {
        if (!_gun) return;
        if (_gun.transform.parent != _hold) _gun.transform.SetParent(_hold, false);
        _gun.transform.localPosition = Vector3.zero; _gun.transform.localRotation = Quaternion.identity;
        if (_rb && !_rb.isKinematic) _rb.isKinematic = true;
    }

    internal void TryShoot()
    {
        _fireTimer -= Time.deltaTime;
        if (_fireTimer > 0f) return;
        _fireTimer = Random.Range(0.45f, 0.9f);
        try { if (_gun != null) _gun.Shoot(); } catch { }
        TryRefillAmmo();
    }

    private void AimAndShoot()
    {
        _fireTimer -= Time.deltaTime;
        var enemy = FindNearestEnemy(transform.position, 50f, out _);
        if (!enemy) return;
        var aimPos = enemy.position + Vector3.up * 0.7f;
        var dir = (aimPos - _hold.position).normalized;
        if (dir.sqrMagnitude > 0.001f)
        {
            _hold.rotation = Quaternion.Slerp(_hold.rotation, Quaternion.LookRotation(dir, Vector3.up), Time.deltaTime * 12f);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.FlatY(), Vector3.up), Time.deltaTime * 6f);
        }
        if (_fireTimer <= 0f)
        {
            _fireTimer = Random.Range(0.45f, 0.9f);
            try { _gun.Shoot(); } catch { }

            // Infinite ammo refill
            TryRefillAmmo();
        }
    }

    private void TryRefillAmmo()
    {
        if (!_gun) return;
        try
        {
            var gunType = typeof(ItemGun);

            // Find max ammo field
            var maxField = gunType.GetField("maxAmmo") ?? gunType.GetField("maxBullets")
                ?? gunType.GetField("ammoMax") ?? gunType.GetField("bulletMax");
            if (maxField == null) return;

            var maxVal = (int)Convert.ChangeType(maxField.GetValue(_gun), typeof(int));
            if (maxVal <= 0) return;

            // Find current ammo field
            var ammoField = gunType.GetField("currentAmmo") ?? gunType.GetField("bullets")
                ?? gunType.GetField("ammo") ?? gunType.GetField("ammoCurrent")
                ?? gunType.GetField("bulletsLeft") ?? gunType.GetField("bulletCount");
            if (ammoField == null) return;

            var curVal = (int)Convert.ChangeType(ammoField.GetValue(_gun), typeof(int));
            if (curVal < maxVal)
            {
                ammoField.SetValue(_gun, maxVal);
            }
        }
        catch { }
    }

    private void DropGun()
    {
        if (!_gun) return;
        try { _gun.transform.SetParent(null, true); if (_rb) { _rb.isKinematic = false; _rb.velocity = Vector3.zero; _rb.angularVelocity = Vector3.zero; } } catch { }
        _gun = null; _pgo = null; _rb = null; _busy = false; _brain?.SetExternalControl(false);
    }

    private static ItemGun FindNearestGun(Vector3 from, float radius)
    {
        ItemGun best = null; var bestSqr = radius * radius;
        foreach (var gun in Object.FindObjectsOfType<ItemGun>())
        {
            if (!gun || !gun.gameObject.activeInHierarchy) continue;
            var pgo = gun.GetComponent<PhysGrabObject>();
            if (!pgo || IsGrabbed(pgo)) continue;
            var sqr = (gun.transform.position - from).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = gun; }
        }
        return best;
    }

    internal static Transform FindNearestEnemy(Vector3 from, float radius, out float distance)
    {
        Transform best = null; var bestSqr = radius * radius;
        foreach (var enemy in Object.FindObjectsOfType<Enemy>())
        {
            if (!enemy || !enemy.gameObject.activeInHierarchy) continue;
            var sqr = (enemy.transform.position - from).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = enemy.transform; }
        }
        distance = best ? Mathf.Sqrt(bestSqr) : 0f;
        return best;
    }

    private static bool IsGrabbed(PhysGrabObject pgo) { try { return SemiFunc.PhysGrabObjectIsGrabbed(pgo); } catch { return false; } }
}

// ═══════════════════════════════════════════════════════════
//  EXTENSIONS
// ═══════════════════════════════════════════════════════════
internal static class VectorExtensions
{
    internal static Vector3 FlatY(this Vector3 v) { v.y = 0f; return v; }
}
