using UnityEngine;

namespace CompareUpgrades;

public static class CompareLockMark
{
    private static SelectionMark lockMark;
    private static GearUpgradeUI markedUi;

    public static void Update(UpgradeInstance lockedUpgrade, GearUpgradeUI hoveredUi)
    {
        if (lockedUpgrade == null)
        {
            Release();
            return;
        }

        var ui = FindUiForLocked(lockedUpgrade, hoveredUi);
        if (ui != markedUi)
            Apply(ui);
    }

    public static void Apply(GearUpgradeUI ui)
    {
        Release();
        markedUi = ui;
        if (ui == null || Menu.Instance == null)
            return;

        try
        {
            lockMark = Menu.Instance.GetSelectionMark();
            lockMark.Setup((RectTransform)ui.transform, Vector2.zero, true);
        }
        catch
        {
            lockMark = null;
        }
    }

    public static void Release()
    {
        if (lockMark != null)
        {
            try
            {
                lockMark.Release();
            }
            catch
            {
            }

            lockMark = null;
        }

        markedUi = null;
    }

    private static GearUpgradeUI FindUiForLocked(UpgradeInstance lockedUpgrade, GearUpgradeUI hoveredUi)
    {
        if (lockedUpgrade == null)
            return null;

        if (hoveredUi != null && ReferenceEquals(hoveredUi.Upgrade, lockedUpgrade))
            return hoveredUi;

        if (markedUi != null && markedUi && markedUi.gameObject.activeInHierarchy
            && ReferenceEquals(markedUi.Upgrade, lockedUpgrade))
            return markedUi;

        return null;
    }
}