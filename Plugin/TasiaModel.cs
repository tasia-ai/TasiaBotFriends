using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TasiaBotFriends;

internal sealed class TasiaModel : MonoBehaviour
{
    private List<GameObject> _parts = new();

    internal void LoadBody()
    {
        try
        {
            // Step 1: Aggressively hide ALL old renderers (including root)
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
            foreach (var tm in GetComponentsInChildren<TextMesh>())
                tm.gameObject.SetActive(false);
            // Also disable name tag text
            foreach (var txt in GetComponentsInChildren<TextMesh>())
                txt.text = "";

            // Log old bounds
            var oldBounds = GetOldBounds();
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaModel] Old visual bounds: center={oldBounds.center} size={oldBounds.size} height={oldBounds.size.y}");

            // Step 2: Read config
            var cfg = TasiaBotFriendsPlugin.Instance?.Config;
            if (cfg == null) return;

            float cfgScale = cfg.Bind("Avatar", "ModelScale", 0.5f, "").Value;
            float ox = cfg.Bind("Avatar", "OffsetX", 0f, "").Value;
            float oy = cfg.Bind("Avatar", "OffsetY", 0f, "").Value;
            float oz = cfg.Bind("Avatar", "OffsetZ", 0f, "").Value;
            float rx = cfg.Bind("Avatar", "RotX", 0f, "").Value;
            float ry = cfg.Bind("Avatar", "RotY", 0f, "").Value;
            float rz = cfg.Bind("Avatar", "RotZ", 0f, "").Value;

            // Step 3: Load parts
            // Each part at its body position + global offset
            var parts = new (string file, float py)[] {
                ("NecoArkBody~NecoArk_body.hhh", 0.85f),
                ("NecoArkHead~NecoArk_neck.hhh", 1.55f),
                ("NecoArkHip~NecoArk_hip.hhh", 0.45f),
                ("NecoArkLeftArm~NecoArk_leftarm.hhh", 0.95f),
                ("NecoArkRightArm~NecoArk_rightarm.hhh", 0.95f),
                ("NecoArkLeftLeg~NecoArk_leftleg.hhh", 0.25f),
                ("NecoArkRightLeg~NecoArk_rightleg.hhh", 0.25f),
            };
            foreach (var (f, py) in parts)
                LoadPart(f, cfgScale, ox, oy + py, oz, rx, ry, rz);

            // Step 4: Log new bounds
            var newBounds = GetAvatarBounds();
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaModel] Avatar bounds: center={newBounds.center} size={newBounds.size} scale={cfgScale}");
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaModel] Final settings: scale={cfgScale} off=({ox},{oy},{oz}) rot=({rx},{ry},{rz})");
        }
        catch (System.Exception ex)
        {
            TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaModel] Load failed (fallback): {ex.Message}");
        }
    }

    private Bounds GetOldBounds()
    {
        var bounds = new Bounds(transform.position, Vector3.zero);
        bool hasBounds = false;
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled) continue;
            if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
            else bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    private Bounds GetAvatarBounds()
    {
        var bounds = new Bounds(transform.position, Vector3.zero);
        bool hasBounds = false;
        foreach (var p in _parts)
        {
            if (p == null) continue;
            foreach (var r in p.GetComponentsInChildren<Renderer>())
            {
                if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
                else bounds.Encapsulate(r.bounds);
            }
        }
        return bounds;
    }

    private void LoadPart(string file, float scale, float px, float py, float pz, float rx, float ry, float rz)
    {
        var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        if (dir == null) return;
        var path = Path.Combine(dir, file);
        if (!File.Exists(path)) return;

        try
        {
            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null) return;

            foreach (var assetName in bundle.GetAllAssetNames())
            {
                var go = bundle.LoadAsset<GameObject>(assetName);
                if (go == null) continue;

                var inst = Instantiate(go);
                inst.transform.SetParent(transform, false);
                inst.transform.localPosition = new Vector3(px, py, pz);
                inst.transform.localRotation = Quaternion.Euler(rx, ry, rz);
                inst.transform.localScale = Vector3.one * scale;

                foreach (var r in inst.GetComponentsInChildren<Renderer>())
                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                        if (r.sharedMaterials[i] == null || r.sharedMaterials[i].shader == null || !r.sharedMaterials[i].shader.isSupported)
                            r.sharedMaterials[i] = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse")) { color = Color.white };

                _parts.Add(inst);
            }
            bundle.Unload(false);
        }
        catch { }
    }

    internal void ApplyLiveCalibration(float scale, float ox, float oy, float oz, float rx, float ry, float rz)
    {
        foreach (var p in _parts)
        {
            if (p == null) continue;
            p.transform.localScale = Vector3.one * scale;
            p.transform.localPosition = new Vector3(ox, oy, oz);
            p.transform.localRotation = Quaternion.Euler(rx, ry, rz);
        }
    }

    private void OnDestroy()
    {
        foreach (var g in _parts) if (g != null) Destroy(g);
        _parts.Clear();
    }
}
