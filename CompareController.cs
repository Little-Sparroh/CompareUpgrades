using Pigeon;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Comparison state machine:
/// - Hotkey while unlocked + hovering an upgrade => lock that upgrade
/// - Hotkey while locked (same, different, or empty hover) => unlock
/// - While locked, hovering a different upgrade shows the locked tooltip beside the main one
/// </summary>
public static class CompareController
{
    private static UpgradeInstance lockedUpgrade;
    private static SelectionMark lockMark;
    private static GearUpgradeUI markedUi;

    public static UpgradeInstance LockedUpgrade => lockedUpgrade;
    public static bool HasLock => lockedUpgrade != null;

    public static void Tick()
    {
        if (!IsGearDetailsOpen(out _))
        {
            if (HasLock)
                ClearLock();
            return;
        }

        // Drop lock if the upgrade instance disappeared (scrapped/destroyed).
        if (lockedUpgrade != null && lockedUpgrade.Upgrade == null)
        {
            ClearLock();
            return;
        }

        HandleHotkey();
        UpdateComparisonView();
        UpdateLockMark();
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
        // Extra-info toggle etc. — keep locked panel in sync if showing.
        if (HasLock && CompareDisplay.IsVisible)
            CompareDisplay.RefreshIfVisible();
    }

    private static void HandleHotkey()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        Key key = CompareUpgradesPlugin.CompareKey;
        if (key == Key.None)
            return;

        // keyboard[key] returns a ButtonControl; avoid KeyControl type name resolution issues.
        var keyControl = keyboard[key];
        if (keyControl == null || !keyControl.wasPressedThisFrame)
            return;

        // Don't steal typing from the search box.
        if (IsSearchFocused())
            return;

        if (HasLock)
        {
            // Any compare press while locked unlocks (same / different / empty).
            ClearLock();
            return;
        }

        GearUpgradeUI hovered = GetHoveredUpgradeUi();
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

        GearUpgradeUI hovered = GetHoveredUpgradeUi();
        bool hoveringOther = hovered != null
            && hovered.Upgrade != null
            && !ReferenceEquals(hovered.Upgrade, lockedUpgrade);

        var main = HoverInfoDisplay.Instance;
        bool mainShowingUpgrade = main != null
            && main.gameObject.activeSelf
            && !CompareDisplay.IsCompareInstance(main)
            && main.SelectedInfo != null;

        if (hoveringOther && mainShowingUpgrade)
        {
            CompareDisplay.Show(lockedUpgrade);
            CompareDisplay.TickPosition();
        }
        else
        {
            // Keep the lock; only hide the companion panel.
            CompareDisplay.Hide();
        }
    }

    private static void Lock(UpgradeInstance upgrade, GearUpgradeUI ui)
    {
        lockedUpgrade = upgrade;
        CompareDisplay.EnsureCreated();
        ApplyLockMark(ui);
        CompareUpgradesPlugin.Log?.LogInfo($"Locked upgrade for comparison: {GetUpgradeName(upgrade)}");
    }

    private static void ClearLock()
    {
        if (lockedUpgrade != null)
            CompareUpgradesPlugin.Log?.LogInfo("Unlocked upgrade comparison.");

        lockedUpgrade = null;
        CompareDisplay.Hide();
        ReleaseLockMark();
    }

    private static void UpdateLockMark()
    {
        if (!HasLock)
        {
            ReleaseLockMark();
            return;
        }

        // Re-bind the mark if the list recycled the previous UI element.
        GearUpgradeUI ui = FindUiForLocked();
        if (ui != markedUi)
            ApplyLockMark(ui);
    }

    private static void ApplyLockMark(GearUpgradeUI ui)
    {
        ReleaseLockMark();
        markedUi = ui;
        if (ui == null || Menu.Instance == null)
            return;

        try
        {
            lockMark = Menu.Instance.GetSelectionMark();
            lockMark.Setup((RectTransform)ui.transform, Vector2.zero, autoSize: true);
        }
        catch
        {
            lockMark = null;
        }
    }

    private static void ReleaseLockMark()
    {
        if (lockMark != null)
        {
            try
            {
                lockMark.Release();
            }
            catch
            {
                // ignored
            }

            lockMark = null;
        }

        markedUi = null;
    }

    private static GearUpgradeUI FindUiForLocked()
    {
        if (lockedUpgrade == null)
            return null;

        // Prefer the currently hovered row if it is the locked upgrade.
        GearUpgradeUI hovered = GetHoveredUpgradeUi();
        if (hovered != null && ReferenceEquals(hovered.Upgrade, lockedUpgrade))
            return hovered;

        // Fall back to a raycast-free scan is not available; keep previous mark parent if still valid.
        if (markedUi != null && markedUi && markedUi.gameObject.activeInHierarchy
            && ReferenceEquals(markedUi.Upgrade, lockedUpgrade))
            return markedUi;

        return null;
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
        // TMP_InputField / InputField focus — avoid compare key while typing in search.
        var selected = UnityEngine.EventSystems.EventSystem.current != null
            ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject
            : null;

        if (selected == null)
            return false;

        if (selected.GetComponent<InputField>() != null)
            return true;

        // TextMeshPro input without a hard compile dependency on the component name path.
        var behaviours = selected.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] == null)
                continue;
            string typeName = behaviours[i].GetType().Name;
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
            // ignored
        }

        return "(unknown)";
    }
}
