using Pigeon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CompareUpgrades;

public static class CompareController
{
    public static UpgradeInstance LockedUpgrade { get; private set; }

    public static bool HasLock => LockedUpgrade != null;

    public static void Tick()
    {
        if (!IsGearDetailsOpen(out _))
        {
            if (HasLock)
                ClearLock();
            return;
        }

        if (LockedUpgrade != null && LockedUpgrade.Upgrade == null)
        {
            ClearLock();
            return;
        }

        HandleHotkey();
        UpdateComparisonView();
        CompareLockMark.Update(LockedUpgrade, GetHoveredUpgradeUi());
    }

    public static void OnGearWindowReset()
    {
        ClearLock();
    }

    public static void Shutdown()
    {
        ClearLock();
        CompareDisplay.Destroy();
    }

    public static void OnMainHoverRefreshed()
    {
        if (HasLock && CompareDisplay.IsVisible)
            CompareDisplay.RefreshIfVisible();
    }

    private static void HandleHotkey()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        var key = ConfigManager.CompareKey != null ? ConfigManager.CompareKey.Value : Key.C;
        if (key == Key.None)
            return;

        var keyControl = keyboard[key];
        if (keyControl == null || !keyControl.wasPressedThisFrame)
            return;

        if (IsSearchFocused())
            return;

        if (HasLock)
        {
            ClearLock();
            return;
        }

        var hovered = GetHoveredUpgradeUi();
        if (hovered != null && hovered.Upgrade != null)
            Lock(hovered.Upgrade, hovered);
    }

    private static void UpdateComparisonView()
    {
        if (!HasLock)
        {
            CompareDisplay.Hide();
            return;
        }

        var hovered = GetHoveredUpgradeUi();
        var hoveringOther = hovered != null
                            && hovered.Upgrade != null
                            && !ReferenceEquals(hovered.Upgrade, LockedUpgrade);

        var main = HoverInfoDisplay.Instance;
        var mainShowingUpgrade = main != null
                                 && main.gameObject.activeSelf
                                 && !CompareDisplay.IsCompareInstance(main)
                                 && main.SelectedInfo != null;

        if (hoveringOther && mainShowingUpgrade)
        {
            CompareDisplay.Show(LockedUpgrade);
            CompareDisplay.TickPosition();
        }
        else
        {
            CompareDisplay.Hide();
        }
    }

    private static void Lock(UpgradeInstance upgrade, GearUpgradeUI ui)
    {
        LockedUpgrade = upgrade;
        CompareDisplay.EnsureCreated();
        CompareLockMark.Apply(ui);
        CompareUpgradesPlugin.Log?.LogInfo($"Locked upgrade for comparison: {GetUpgradeName(upgrade)}");
    }

    private static void ClearLock()
    {
        if (LockedUpgrade != null)
            CompareUpgradesPlugin.Log?.LogInfo("Unlocked upgrade comparison.");

        LockedUpgrade = null;
        CompareDisplay.Hide();
        CompareLockMark.Release();
    }

    private static GearUpgradeUI GetHoveredUpgradeUi()
    {
        if (UIRaycaster.RaycastForComponent<GearUpgradeUI>(out var ui) && ui != null)
            return ui;
        return null;
    }

    private static bool IsGearDetailsOpen(out GearDetailsWindow window)
    {
        window = null;
        if (Menu.Instance == null || !Menu.Instance.IsOpen)
            return false;

        if (Menu.Instance.WindowSystem == null)
            return false;

        window = Menu.Instance.WindowSystem.GetTop() as GearDetailsWindow;
        return window != null;
    }

    private static bool IsSearchFocused()
    {
        var selected = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject
            : null;

        if (selected == null)
            return false;

        if (selected.GetComponent<InputField>() != null)
            return true;

        var behaviours = selected.GetComponents<MonoBehaviour>();
        for (var i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] == null)
                continue;
            var typeName = behaviours[i].GetType().Name;
            if (typeName == "TMP_InputField" || typeName == "InputField")
                return true;
        }

        return false;
    }

    private static string GetUpgradeName(UpgradeInstance upgrade)
    {
        try
        {
            if (upgrade?.Upgrade != null)
                return upgrade.Upgrade.GetInstanceName(upgrade);
        }
        catch
        {
        }

        return "(unknown)";
    }
}