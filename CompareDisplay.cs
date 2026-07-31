using System;
using System.Reflection;
using HarmonyLib;
using Pigeon;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CompareUpgrades;

public static class CompareDisplay
{
    private static LockedUpgradeProxy proxy;
    private static GameObject proxyRoot;
    private static UpgradeInstance shownUpgrade;

    public static HoverInfoDisplay Clone { get; private set; }

    public static bool IsVisible { get; private set; }

    public static bool IsCompareInstance(HoverInfoDisplay display)
    {
        return Clone != null && display == Clone;
    }

    public static void EnsureCreated()
    {
        if (Clone != null)
            return;

        var main = HoverInfoDisplay.Instance;
        if (main == null)
            return;

        var parent = main.transform.parent;
        if (parent == null)
            return;

        Clone = Object.Instantiate(main, parent);
        Clone.gameObject.name = "CompareUpgrades_HoverInfoDisplay";

        RestoreMainInstance(main);

        DisableRaycasts(Clone.gameObject);

        try
        {
            Clone.Deactivate();
        }
        catch
        {
            Clone.gameObject.SetActive(false);
        }

        EnsureProxy();
        CompareUpgradesPlugin.Log?.LogInfo("Created comparison hover display.");
    }

    public static void Show(UpgradeInstance upgrade)
    {
        if (upgrade == null)
            return;

        EnsureCreated();
        if (Clone == null || proxy == null)
            return;

        var needsRefresh = !IsVisible || shownUpgrade != upgrade || !Clone.gameObject.activeSelf;
        proxy.SetLockedUpgrade(upgrade);
        shownUpgrade = upgrade;

        if (needsRefresh)
            try
            {
                Clone.ShowInfo(proxy);
            }
            catch (Exception ex)
            {
                CompareUpgradesPlugin.Log?.LogWarning($"ShowInfo on compare display failed: {ex.Message}");
                return;
            }

        IsVisible = Clone.gameObject.activeSelf;
        if (IsVisible)
            CompareTooltipPositioning.PositionBesideMain(HoverInfoDisplay.Instance, Clone);
    }

    public static void RefreshIfVisible()
    {
        if (!IsVisible || Clone == null || proxy == null || shownUpgrade == null)
            return;

        try
        {
            Clone.Refresh();
            CompareTooltipPositioning.PositionBesideMain(HoverInfoDisplay.Instance, Clone);
        }
        catch (Exception ex)
        {
            CompareUpgradesPlugin.Log?.LogWarning($"Refresh compare display failed: {ex.Message}");
        }
    }

    public static void Hide()
    {
        if (Clone == null)
        {
            IsVisible = false;
            shownUpgrade = null;
            return;
        }

        if (Clone.gameObject.activeSelf)
            try
            {
                Clone.Deactivate();
            }
            catch
            {
                Clone.gameObject.SetActive(false);
            }

        IsVisible = false;
        shownUpgrade = null;
    }

    public static void TickPosition()
    {
        if (!IsVisible || Clone == null || !Clone.gameObject.activeSelf)
            return;

        CompareTooltipPositioning.PositionBesideMain(HoverInfoDisplay.Instance, Clone);
    }

    public static void Destroy()
    {
        Hide();

        if (Clone != null)
        {
            Object.Destroy(Clone.gameObject);
            Clone = null;
        }

        if (proxyRoot != null)
        {
            Object.Destroy(proxyRoot);
            proxyRoot = null;
            proxy = null;
        }

        shownUpgrade = null;
        IsVisible = false;
    }

    private static void EnsureProxy()
    {
        if (proxy != null)
            return;

        proxyRoot = new GameObject("CompareUpgrades_LockedProxy");
        Object.DontDestroyOnLoad(proxyRoot);
        proxyRoot.hideFlags = HideFlags.HideAndDontSave;

        proxy = proxyRoot.AddComponent<LockedUpgradeProxy>();
    }

    private static void RestoreMainInstance(HoverInfoDisplay main)
    {
        try
        {
            var prop = AccessTools.Property(typeof(HoverInfoDisplay), "Instance");
            if (prop != null)
            {
                prop.SetValue(null, main, null);
                return;
            }
        }
        catch (Exception ex)
        {
            CompareUpgradesPlugin.Log?.LogWarning(
                $"Failed to restore HoverInfoDisplay.Instance via AccessTools: {ex.Message}");
        }

        try
        {
            typeof(HoverInfoDisplay)
                .GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)
                ?.SetValue(null, main, null);
        }
        catch (Exception ex)
        {
            CompareUpgradesPlugin.Log?.LogError($"Failed to restore HoverInfoDisplay.Instance: {ex}");
        }
    }

    private static void DisableRaycasts(GameObject root)
    {
        var graphics = root.GetComponentsInChildren<Graphic>(true);
        for (var i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = false;

        var groups = root.GetComponentsInChildren<CanvasGroup>(true);
        for (var i = 0; i < groups.Length; i++)
        {
            groups[i].blocksRaycasts = false;
            groups[i].interactable = false;
        }
    }
}