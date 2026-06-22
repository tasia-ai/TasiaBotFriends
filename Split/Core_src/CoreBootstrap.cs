using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace TasiaBotFriends;

public static class CoreBootstrap
{
    public static void Startup(BaseUnityPlugin loader, ManualLogSource log)
    {
        log.LogInfo("[CoreBootstrap] Starting Tasia Core...");

        TasiaBotFriendsPlugin.Loader = loader;
        TasiaBotFriendsPlugin.LogRef = log;

        var go = new GameObject("TasiaCoreHost");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<TasiaBotFriendsPlugin>();
    }
}
