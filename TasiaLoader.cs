using System; using System.IO; using System.Reflection; using BepInEx; using UnityEngine;
namespace TasiaLoader;
[BepInPlugin("Tasia.Loader", "TasiaLoader", "1.0.0")]
public sealed class TasiaLoaderPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        Logger.LogInfo("[TasiaLoader] Watching for Photon...");
        var go = new GameObject("TasiaLoaderWatch");
        DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<TasiaLoaderWatch>().SetConfig(Logger, this);
    }
}
internal sealed class TasiaLoaderWatch : MonoBehaviour
{
    private float _nextCheck; private bool _loaded;
    private BepInEx.Logging.ManualLogSource _log;

    internal void SetConfig(BepInEx.Logging.ManualLogSource log, TasiaLoaderPlugin plugin)
    {
        _log = log;
    }

    private void Update()
    {
        if (_loaded || Time.time < _nextCheck) return;
        _nextCheck = Time.time + 5f;
        try
        {
            // Check Photon by scanning already-loaded assemblies (safe, no loading)
            System.Type pn = null;
            foreach (var aAsm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!aAsm.FullName.StartsWith("PhotonUnityNetworking")) continue;
                pn = aAsm.GetType("Photon.Pun.PhotonNetwork");
                break;
            }
            if (pn == null) return;
            var c = (bool)pn.GetProperty("IsConnected", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
            if (!c) return;
            _log.LogInfo("[TasiaLoader] Photon ready! Loading...");
            var path = Path.Combine(Paths.PluginPath, "TasiaNetwork_Core.dll");
            if (!File.Exists(path)) { _log.LogInfo("[TasiaLoader] Core DLL not found"); return; }
            var asm = Assembly.LoadFrom(path);
            asm.GetType("TasiaNetwork.TasiaNetworkPlugin")?.GetMethod("LateLoad", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
            _loaded = true;
            _log.LogInfo("[TasiaLoader] TasiaNetwork loaded!");
            Destroy(gameObject);
        }
        catch (Exception ex) { if (_log != null) _log.LogInfo($"[TasiaLoader] {ex.Message}"); }
    }
}
