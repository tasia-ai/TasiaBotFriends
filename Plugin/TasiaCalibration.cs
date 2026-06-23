using UnityEngine;

namespace TasiaBotFriends;

internal sealed class TasiaCalibration : MonoBehaviour
{
    internal static TasiaCalibration Instance;
    internal static bool IsActive;

    private float _scale = 0.8f, _offX, _offY, _offZ, _rotX, _rotY, _rotZ;
    private Rect _winRect = new Rect(50, 50, 320, 380);
    private GameObject _bot;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        name = "TasiaCalibration";
        LoadFromConfig();
    }

    internal void Toggle()
    {
        IsActive = !IsActive;
        if (IsActive) LoadFromConfig();
    }

    private void OnGUI()
    {
        if (!IsActive) return;
        _winRect = GUI.Window(500, _winRect, DrawWindow, "Tasia Avatar Calibration", GUI.skin.window);
    }

    private void DrawWindow(int id)
    {
        _bot = GetBot();
        if (_bot == null || _bot.GetComponent<TasiaModel>() == null)
        {
            GUILayout.Label("Spawn Tasia first (F8)");
            GUI.DragWindow();
            return;
        }

        GUILayout.Space(10);

        // Scale
        GUILayout.Label($"Scale: {_scale:F2}");
        _scale = GUILayout.HorizontalSlider(_scale, 0.1f, 2f);
        GUILayout.Space(5);

        // Position offsets
        _offY = Slider("Y (height)", _offY, -2f, 2f);
        _offX = Slider("X (left-right)", _offX, -2f, 2f);
        _offZ = Slider("Z (forward-back)", _offZ, -2f, 2f);
        GUILayout.Space(5);

        // Rotation
        _rotY = Slider("Rotate Y", _rotY, -180f, 180f);

        // Apply live
        Apply();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Save", GUILayout.Height(30)))
        {
            SaveToConfig();
            IsActive = false;
        }

        if (GUILayout.Button("Reset", GUILayout.Height(30)))
        {
            _scale = 0.5f; _offX = _offY = _offZ = _rotX = _rotY = _rotZ = 0f;
            Apply();
        }

        if (GUILayout.Button("Close", GUILayout.Height(30)))
            IsActive = false;

        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    private float Slider(string label, float value, float min, float max)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {value:F2}", GUILayout.Width(180));
        value = GUILayout.HorizontalSlider(value, min, max);
        GUILayout.EndHorizontal();
        return value;
    }

    private void Apply()
    {
        if (_bot == null) return;
        var model = _bot.GetComponent<TasiaModel>();
        if (model != null) model.ApplyLiveCalibration(_scale, _offX, _offY, _offZ, _rotX, _rotY, _rotZ);
    }

    private void LoadFromConfig()
    {
        var p = TasiaBotFriendsPlugin.Instance;
        if (p == null || p.Config == null) return;
        _scale = p.Config.Bind("Avatar", "ModelScale", 0.8f, "").Value;
        _offX = p.Config.Bind("Avatar", "OffsetX", 0f, "").Value;
        _offY = p.Config.Bind("Avatar", "OffsetY", 0.65f, "").Value;
        _offZ = p.Config.Bind("Avatar", "OffsetZ", 0f, "").Value;
        _rotX = p.Config.Bind("Avatar", "RotX", 0f, "").Value;
        _rotY = p.Config.Bind("Avatar", "RotY", 0f, "").Value;
        _rotZ = p.Config.Bind("Avatar", "RotZ", 0f, "").Value;
    }

    private void SaveToConfig()
    {
        var p = TasiaBotFriendsPlugin.Instance;
        if (p == null || p.Config == null) return;
        p.Config.Bind("Avatar", "ModelScale", 0.8f, "").Value = _scale;
        p.Config.Bind("Avatar", "OffsetX", 0f, "").Value = _offX;
        p.Config.Bind("Avatar", "OffsetY", 0.65f, "").Value = _offY;
        p.Config.Bind("Avatar", "OffsetZ", 0f, "").Value = _offZ;
        p.Config.Bind("Avatar", "RotX", 0f, "").Value = _rotX;
        p.Config.Bind("Avatar", "RotY", 0f, "").Value = _rotY;
        p.Config.Bind("Avatar", "RotZ", 0f, "").Value = _rotZ;
        p.Config.Save();
        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaCalibration] Saved!");
    }

    private static GameObject GetBot()
    {
        var inst = TasiaBotFriendsPlugin.Instance;
        return inst?.GetBotList().Count > 0 ? inst.GetBotList()[0] : null;
    }
}
