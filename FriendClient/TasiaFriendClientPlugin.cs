using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace TasiaFriendClient;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class TasiaFriendClientPlugin : BaseUnityPlugin
{
    public const string PluginGuid    = "Tasia.FriendClient";
    public const string PluginName    = "TasiaFriendClient";
    public const string PluginVersion = "1.1.0";

    internal static TasiaFriendClientPlugin Instance { get; private set; }
    internal static ManualLogSource Log => Instance.Logger;

    // ── External sync config ──
    private ConfigEntry<bool>   ExtSyncEnabled;
    private ConfigEntry<string> ExtSyncUrl;
    private ConfigEntry<string> ExtSyncRoom;
    private ConfigEntry<string> ExtSyncToken;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Config
        ExtSyncEnabled = Config.Bind("SyncServer", "Enabled", false, "Enable external WebSocket sync.");
        ExtSyncUrl     = Config.Bind("SyncServer", "ServerUrl", "ws://127.0.0.1:24222/ws", "WebSocket server URL.");
        ExtSyncRoom    = Config.Bind("SyncServer", "RoomId", "tasia-default", "Room ID for sync session.");
        ExtSyncToken   = Config.Bind("SyncServer", "RoomToken", "", "Optional room token.");

        Log.LogInfo("[TasiaFriendClient] v1.1.0 loaded — lightweight sync receiver only.");

        // Create Photon RaiseEvent receiver (legacy)
        var remote = new GameObject("TasiaFriendClientAvatar");
        DontDestroyOnLoad(remote);
        remote.AddComponent<TasiaFriendAvatar>();
        Log.LogInfo("[TasiaFriendClient] Photon avatar receiver created.");

        // External WebSocket sync (friend/client role)
        if (ExtSyncEnabled.Value)
        {
            // Configure and create external sync as client
            var syncGo = new GameObject("TasiaFriendSync");
            DontDestroyOnLoad(syncGo);
            var sync = syncGo.AddComponent<TasiaFriendSync>();
            sync.Configure(ExtSyncUrl.Value, ExtSyncRoom.Value, ExtSyncToken.Value);
            Log.LogInfo($"[TasiaFriendClient] External sync enabled (client) → {ExtSyncUrl.Value}");
        }
    }

    private void OnDestroy() { }
}
