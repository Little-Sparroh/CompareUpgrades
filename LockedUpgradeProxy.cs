using UnityEngine.InputSystem;

namespace CompareUpgrades;

public sealed class LockedUpgradeProxy : HoverInfoUpgrade
{
    public override bool EnableHoverInfoEvents => false;

    public override bool GetPrimaryBinding(out InputAction binding, out string label)
    {
        binding = null;
        label = null;
        return false;
    }

    public override bool GetSecondaryBinding(out InputAction binding, out string label)
    {
        binding = null;
        label = null;
        return false;
    }

    public override int GetAdditionalBindingCount()
    {
        return 0;
    }

    public override bool HasUnlockAction(out UnlockActionParams data)
    {
        data = default;
        return false;
    }

    public override bool HasUnlockInfo(out string text)
    {
        text = null;
        return false;
    }

    public void SetLockedUpgrade(UpgradeInstance upgrade)
    {
        Upgrade = upgrade;
    }
}