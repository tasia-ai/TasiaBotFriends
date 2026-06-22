using MenuLib;
using MenuLib.MonoBehaviors;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using UnityEngine;
using System.Collections;

namespace TasiaBotFriends;

internal static class TasiaMenuLib
{
    private static REPOPopupPage _page;
    private static bool _showRequested, _initialized;

    internal static void Process()
    {
        if (_showRequested) { _showRequested = false; Toggle(); }
    }

    internal static void RequestToggle() { _showRequested = true; }

    private static void Toggle()
    {
        if (MenuManager.instance == null) return;
        if (_page != null && _page.isActiveAndEnabled) { Close(); return; }

        var page = MenuAPI.CreateREPOPopupPage("Tasia Control", REPOPopupPage.PresetSide.Right, false, true);

        page.AddElement(p => { MenuAPI.CreateREPOButton("Spawn (F8)", () => TasiaBotFriendsPlugin.Instance?.ManualSpawn("Menu"), p, new Vector2(250, 28)); });
        page.AddElement(p => { MenuAPI.CreateREPOButton("Despawn (F9)", () => TasiaBotFriendsPlugin.Instance?.RemoveAllBots(), p, new Vector2(250, 28)); });

        page.AddElementToScrollView(sv => { return MenuAPI.CreateREPOLabel("-- Modes --", sv).rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Collect", () => DoMode(TasiaMode.COLLECT), sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Follow", () => DoMode(TasiaMode.FOLLOW), sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Fight", () => DoMode(TasiaMode.FIGHT), sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Wait", () => DoMode(TasiaMode.WAIT), sv); return b.rectTransform; });

        page.AddElementToScrollView(sv => { return MenuAPI.CreateREPOLabel("-- Tools --", sv).rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Toggle Gun", () => ToggleGun(), sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("God Mode", () => TasiaBotFriendsPlugin.Instance?.ToggleGodModeExternal(), sv); return b.rectTransform; });

        page.AddElement(p => { MenuAPI.CreateREPOButton("Close", () => Close(), p, new Vector2(270, 24)); });
        Open(page);
    }

    private static void DoMode(TasiaMode mode)
    {
        var bot = GetBot();
        var brain = bot?.GetComponent<TasiaBotBrain>();
        if (brain != null) brain.SetTasiaMode(mode);
    }

    private static void ToggleGun()
    {
        var bot = GetBot();
        if (bot == null) return;
        var weapon = bot.GetComponent<TasiaBotWeaponUser>();
        if (weapon != null && weapon.HasGun)
        {
            var gun = bot.transform.Find("HoldPoint")?.GetComponentInChildren<ItemGun>();
            if (gun != null) Object.Destroy(gun.gameObject);
            Object.Destroy(weapon);
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMenu] Gun removed.");
        }
        else
        {
            var hold = bot.transform.Find("HoldPoint");
            if (hold != null)
            {
                var w = bot.AddComponent<TasiaBotWeaponUser>();
                var agent = bot.GetComponent<NavMeshAgent>();
                var brain = bot.GetComponent<TasiaBotBrain>();
                w.Init(agent, hold, brain, 35f, true);
                TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMenu] Gun added.");
            }
        }
    }

    private static void Open(REPOPopupPage page)
    {
        if (MenuManager.instance == null) return;
        _page = page;
        MenuManager.instance.StartCoroutine(OpenRoutine(page));
    }

    private static IEnumerator OpenRoutine(REPOPopupPage page)
    {
        yield return new WaitForSeconds(0.05f);
        if (MenuManager.instance == null) yield break;
        MenuManager.instance.PageCloseAll();
        page.OpenPage(false);
    }

    private static void Close()
    {
        if (_page == null || MenuManager.instance == null) return;
        MenuManager.instance.PageCloseAll();
        _page.ClosePage(true);
        if (_page.menuPage != null) MenuManager.instance.PageRemove(_page.menuPage);
        _page = null;
    }

    private static GameObject GetBot()
    {
        var inst = TasiaBotFriendsPlugin.Instance;
        return inst?.GetBotList().Count > 0 ? inst.GetBotList()[0] : null;
    }
}
