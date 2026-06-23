using MenuLib; using MenuLib.MonoBehaviors; using UnityEngine.AI; using Object = UnityEngine.Object; using UnityEngine; using System.Collections;
namespace TasiaBotFriends;
internal static class TasiaMenuLib
{
    private static REPOPopupPage _page;
    private static bool _showRequested, _initialized, _showDebug, _showCalibration;

    internal static void Process() { if (_showRequested) { _showRequested = false; Toggle(); } }
    internal static void RequestToggle() { _showRequested = true; }

    private static void LoadCalFromConfig()
    {
        var p = TasiaBotFriendsPlugin.Instance;
        if (p == null || p.Config == null) return;
        _calScale = p.Config.Bind("Avatar", "ModelScale", 0.5f, "").Value;
        _calOffX = p.Config.Bind("Avatar", "OffsetX", 0f, "").Value;
        _calOffY = p.Config.Bind("Avatar", "OffsetY", 0f, "").Value;
        _calOffZ = p.Config.Bind("Avatar", "OffsetZ", 0f, "").Value;
    }

    private static void Toggle()
    {
        if (MenuManager.instance == null) return;
        if (_page != null && _page.isActiveAndEnabled) { Close(); return; }

        var page = MenuAPI.CreateREPOPopupPage("Tasia Control", REPOPopupPage.PresetSide.Left, false, true);
        var bot = GetBot(); var brain = bot?.GetComponent<TasiaBotBrain>(); var carrier = bot?.GetComponent<TasiaBotCarrier>();
        page.AddElementToScrollView(sv => { return MenuAPI.CreateREPOLabel($"Mode: {brain?.CurrentMode} | Carry: {(carrier?.IsCarrying == true ? "YES" : "no")}", sv).rectTransform; });

        page.AddElementToScrollView(sv => { return MenuAPI.CreateREPOLabel("-- Actions --", sv).rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Spawn (F8)", () => { TasiaBotFriendsPlugin.Instance?.ManualSpawn("Menu"); Close(); }, sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Despawn (F9)", () => { TasiaBotFriendsPlugin.Instance?.RemoveAllBots(); Close(); }, sv); return b.rectTransform; });

        page.AddElementToScrollView(sv => { return MenuAPI.CreateREPOLabel("-- Modes --", sv).rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton(mk("Collect", TasiaMode.COLLECT, brain), () => { DoMode(TasiaMode.COLLECT); Close(); }, sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton(mk("Follow", TasiaMode.FOLLOW, brain), () => { DoMode(TasiaMode.FOLLOW); Close(); }, sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton(mk("Fight", TasiaMode.FIGHT, brain), () => { DoMode(TasiaMode.FIGHT); Close(); }, sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton(mk("Wait", TasiaMode.WAIT, brain), () => { DoMode(TasiaMode.WAIT); Close(); }, sv); return b.rectTransform; });

        page.AddElementToScrollView(sv => { return MenuAPI.CreateREPOLabel("-- Tools --", sv).rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Debug Safety", () => { TasiaBotFriendsPlugin.Instance?.ToggleGodModeExternal(); Close(); }, sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Toggle Debug HUD", () => { _showDebug = !_showDebug; Close(); }, sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { return MenuAPI.CreateREPOLabel("-- Avatar --", sv).rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Avatar (F6)", () => { if (TasiaCalibration.Instance != null) TasiaCalibration.Instance.Toggle(); Close(); }, sv); return b.rectTransform; });

        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Reset Avatar", () => { _calScale = 0.5f; _calOffX = _calOffY = _calOffZ = _calRotX = _calRotY = _calRotZ = 0f; var b2 = GetBot(); if (b2 != null) ApplyCalibration(b2); Close(); }, sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Close", () => Close(), sv); return b.rectTransform; });
        Open(page);
    }

    private static string mk(string label, TasiaMode mode, TasiaBotBrain brain) { return (brain != null && brain.CurrentMode == mode) ? $"> {label} <" : label; }

    private static void DoMode(TasiaMode mode)
    {
        var bot = GetBot(); var brain = bot?.GetComponent<TasiaBotBrain>();
        if (brain != null) { brain.SetTasiaMode(mode); TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaMenu] Mode -> {mode}"); }
        else TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMenu] No bot for mode");
    }

    internal static void DrawCalibrationOnly()
    {
        if (!_showCalibration) return;
        var x = 10f; var y = 45f; var bot = GetBot();
        if (bot == null || bot.GetComponent<TasiaModel>() == null) { GUI.Label(new Rect(x, y, 400, 20), "Spawn Tasia first (F8)"); return; }
        
        GUI.Label(new Rect(x, y, 500, 20), "-- Avatar Calibration --"); y += 24f;
        float step = 0.05f, bigStep = 0.1f;
        
        GUI.Label(new Rect(x, y, 80, 20), "Scale:"); 
        if (GUI.Button(new Rect(x + 80, y, 25, 20), "-b")) _calScale -= bigStep;
        if (GUI.Button(new Rect(x + 108, y, 25, 20), "-")) _calScale -= step;
        if (GUI.Button(new Rect(x + 138, y, 25, 20), "+")) _calScale += step;
        if (GUI.Button(new Rect(x + 166, y, 25, 20), "+b")) _calScale += bigStep;
        GUI.Label(new Rect(x + 195, y, 100, 20), $"{_calScale:F2}"); y += 24f;

        GUI.Label(new Rect(x, y, 80, 20), "OffY:"); 
        if (GUI.Button(new Rect(x + 80, y, 25, 20), "-b")) _calOffY -= bigStep;
        if (GUI.Button(new Rect(x + 108, y, 25, 20), "-")) _calOffY -= step;
        if (GUI.Button(new Rect(x + 138, y, 25, 20), "+")) _calOffY += step;
        if (GUI.Button(new Rect(x + 166, y, 25, 20), "+b")) _calOffY += bigStep;
        GUI.Label(new Rect(x + 195, y, 100, 20), $"{_calOffY:F2}"); y += 24f;

        GUI.Label(new Rect(x, y, 80, 20), "OffX:"); 
        if (GUI.Button(new Rect(x + 80, y, 25, 20), "-b")) _calOffX -= bigStep;
        if (GUI.Button(new Rect(x + 108, y, 25, 20), "-")) _calOffX -= step;
        if (GUI.Button(new Rect(x + 138, y, 25, 20), "+")) _calOffX += step;
        if (GUI.Button(new Rect(x + 166, y, 25, 20), "+b")) _calOffX += bigStep;
        GUI.Label(new Rect(x + 195, y, 100, 20), $"{_calOffX:F2}"); y += 24f;

        ApplyCalibration(bot);
        
        if (GUI.Button(new Rect(x, y, 100, 24), "Save")) { SaveCalibration(TasiaBotFriendsPlugin.Instance); _showCalibration = false; }
        if (GUI.Button(new Rect(x + 110, y, 100, 24), "Close")) _showCalibration = false;
    }

    internal static void DrawDebug()
    {
        if (!_showDebug) return; var p = TasiaBotFriendsPlugin.Instance; if (p == null) return;
        float y = 45f; var x = 10f; var bot = GetBot(); var brain = bot?.GetComponent<TasiaBotBrain>(); var carrier = bot?.GetComponent<TasiaBotCarrier>(); var weapon = bot?.GetComponent<TasiaBotWeaponUser>();
        GUI.Label(new Rect(x, y, 600, 22), $"Tasia Debug v{TasiaBotFriendsPlugin.PluginVersion}"); y += 22f;
        GUI.Label(new Rect(x, y, 600, 20), $"Mode: {brain?.CurrentMode} Intent: {brain?.CurrentIntent} State: {brain?.CurrentState}"); y += 20f;
        GUI.Label(new Rect(x, y, 600, 20), $"Carry: {(carrier?.IsCarrying == true ? "YES" : "no")} Gun: {(weapon?.HasGun == true ? "YES" : "no")}"); y += 20f;
        if (bot != null) {
            int tE = 0, nE = 0; float nD = 999f;
            foreach (var e in Object.FindObjectsOfType<Enemy>()) { if (e == null || !e.gameObject.activeInHierarchy) continue; tE++; var d = Vector3.Distance(bot.transform.position, e.transform.position); if (d < nD) nD = d; if (d < 20f) nE++; }
            GUI.Label(new Rect(x, y, 600, 20), $"Monsters: {tE} total, {nE} near, nearest {nD:F1}m"); y += 20f;
        }
        if (!string.IsNullOrEmpty(p.LastAiResponse)) { GUI.Label(new Rect(x, y, 600, 20), $"AI: {p.LastAiResponse}"); y += 20f; }
        if (bot != null) { var bub = bot.transform.Find("SpeechBubble")?.GetComponent<TextMesh>(); if (bub != null && !string.IsNullOrEmpty(bub.text)) { GUI.Label(new Rect(x, y, 600, 20), $"Say: \"{bub.text}\""); y += 20f; } }
    }

    private static void Open(REPOPopupPage page) { if (MenuManager.instance == null) return; _page = page; MenuManager.instance.StartCoroutine(OpenRoutine(page)); }
    private static IEnumerator OpenRoutine(REPOPopupPage page) { yield return new WaitForSeconds(0.05f); if (MenuManager.instance == null) yield break; MenuManager.instance.PageCloseAll(); page.OpenPage(false); }
    private static float _calScale = 0.5f, _calOffX = 0f, _calOffY = 0f, _calOffZ = 0f, _calRotX = 0f, _calRotY = 0f, _calRotZ = 0f;

    private static void ApplyCalibration(GameObject bot)
    {
        var model = bot.GetComponent<TasiaModel>();
        if (model != null) model.ApplyLiveCalibration(_calScale, _calOffX, _calOffY, _calOffZ, _calRotX, _calRotY, _calRotZ);
    }

    private static void SaveCalibration(TasiaBotFriendsPlugin p)
    {
        if (p == null || p.Config == null) return;
        p.Config.Bind("Avatar", "ModelScale", 0.5f, "").Value = _calScale;
        p.Config.Bind("Avatar", "OffsetX", 0f, "").Value = _calOffX;
        p.Config.Bind("Avatar", "OffsetY", 0f, "").Value = _calOffY;
        p.Config.Bind("Avatar", "OffsetZ", 0f, "").Value = _calOffZ;
        p.Config.Bind("Avatar", "RotX", 0f, "").Value = _calRotX;
        p.Config.Bind("Avatar", "RotY", 0f, "").Value = _calRotY;
        p.Config.Bind("Avatar", "RotZ", 0f, "").Value = _calRotZ;
        p.Config.Save();
        TasiaBotFriendsPlugin.Log.LogInfo($"[TasiaModel] Calibration saved");
    }

    private static void Close() { if (_page == null || MenuManager.instance == null) return; MenuManager.instance.PageCloseAll(); _page.ClosePage(true); if (_page.menuPage != null) MenuManager.instance.PageRemove(_page.menuPage); _page = null; }
    private static GameObject GetBot() { var inst = TasiaBotFriendsPlugin.Instance; return inst?.GetBotList().Count > 0 ? inst.GetBotList()[0] : null; }
}
