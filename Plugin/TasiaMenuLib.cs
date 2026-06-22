using MenuLib;
using MenuLib.MonoBehaviors;
using UnityEngine;
using System.Collections;

namespace TasiaBotFriends;

internal static class TasiaMenuLib
{
    private static REPOPopupPage _page;
    private static bool _showRequested, _initialized;

    internal static void Process()
    {
        if (_showRequested)
        {
            _showRequested = false;
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMenu] Toggle requested, initializing...");
            Toggle();
        }
    }

    internal static void RequestToggle()
    {
        _showRequested = true;
        if (!_initialized)
        {
            _initialized = true;
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMenu] MenuLib ready.");
        }
    }

    private static void Toggle()
    {
        if (MenuManager.instance == null)
        {
            TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMenu] MenuManager.instance is NULL");
            return;
        }

        if (_page != null && _page.isActiveAndEnabled)
        {
            Close();
            return;
        }

        TasiaBotFriendsPlugin.Log.LogInfo("[TasiaMenu] Opening menu...");
        var page = MenuAPI.CreateREPOPopupPage("Tasia Control", REPOPopupPage.PresetSide.Right, false, true);

        page.AddElement(p => { MenuAPI.CreateREPOButton("Spawn (F8)", () => TasiaBotFriendsPlugin.Instance?.ManualSpawn("Menu"), p, new Vector2(250, 28)); });
        page.AddElement(p => { MenuAPI.CreateREPOButton("Despawn (F9)", () => TasiaBotFriendsPlugin.Instance?.RemoveAllBots(), p, new Vector2(250, 28)); });
        page.AddElementToScrollView(sv => { return MenuAPI.CreateREPOLabel("-- Mode --", sv).rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Collect", () => { var br = GetBot()?.GetComponent<TasiaBotBrain>(); if (br != null) br.SetFollowMode(false); }, sv); return b.rectTransform; });
        page.AddElementToScrollView(sv => { var b = MenuAPI.CreateREPOButton("Follow", () => { var br = GetBot()?.GetComponent<TasiaBotBrain>(); if (br != null) br.SetFollowMode(true); }, sv); return b.rectTransform; });
        page.AddElement(p => { MenuAPI.CreateREPOButton("Close", () => Close(), p, new Vector2(270, 24)); });

        Open(page);
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
