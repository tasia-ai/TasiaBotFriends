using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace TasiaBotFriends;

[BepInPlugin("Tasia.BotFriends", "TasiaBotFriends", "1.2.0")]
public sealed class TasiaLoader : BaseUnityPlugin
{
    internal static TasiaLoader Instance { get; private set; }
    internal static ManualLogSource Log => Instance.Logger;

    private void Awake()
    {
        Instance = this;
        Logger.LogInfo("[TasiaLoader] v1.2.0 loaded.");

        // Create a persistent runner object that own Update/OnGUI always work
        var go = new GameObject("TasiaLoaderRunner");
        DontDestroyOnLoad(go);
        go.AddComponent<TasiaLoaderComponent>();
    }
}

internal sealed class TasiaLoaderComponent : MonoBehaviour
{
    private bool _coreLoaded;
    private bool _coreLoadFailed;

    private void Awake()
    {
        TasiaLoader.Log.LogInfo("[TasiaLoaderComponent] Runner active — press F8 to activate Tasia.");
    }

    private void Update()
    {
        if (_coreLoaded || _coreLoadFailed) return;

        if (Input.GetKeyDown(KeyCode.F8))
        {
            TasiaLoader.Log.LogInfo("[TasiaLoader] F8 pressed — loading core...");
            LoadCore();
        }
    }

    private void OnGUI()
    {
        if (_coreLoaded) return;

        // "Tasia" button in top-right corner
        var btnRect = new Rect(Screen.width - 95, 12, 83, 28);
        if (GUI.Button(btnRect, "Tasia"))
        {
            TasiaLoader.Log.LogInfo("[TasiaLoader] Button clicked — loading core...");
            LoadCore();
        }

        if (_coreLoadFailed)
            GUI.Label(new Rect(Screen.width - 250, 48, 240, 20), "Tasia load failed, check log.");
    }

    private void LoadCore()
    {
        if (_coreLoaded) return;
        try
        {
            var path = Path.Combine(Paths.PluginPath, "TasiaBotFriends_Core.dll");
            TasiaLoader.Log.LogInfo("[TasiaLoader] Loading core...");
            var asm = Assembly.LoadFrom(path);
            var type = asm.GetType("TasiaBotFriends.CoreBootstrap");
            type?.GetMethod("Startup", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new object[] {
                TasiaLoader.Instance,
                TasiaLoader.Log
            });
            _coreLoaded = true;
            TasiaLoader.Log.LogInfo("[TasiaLoader] Tasia activated!");
            Destroy(gameObject); // cleanup this loader component
        }
        catch (Exception ex)
        {
            _coreLoadFailed = true;
            TasiaLoader.Log.LogError($"[TasiaLoader] Failed: {ex.Message}");
        }
    }
}
