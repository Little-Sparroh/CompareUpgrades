using Pigeon;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Display-only HoverInfo backed by a stable UpgradeInstance.
/// Avoids locking a pooled GearUpgradeUI reference and suppresses interactive bindings
/// so the comparison tooltip cannot steal scrap/favorite/unlock input.
/// </summary>
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

    public override int GetAdditionalBindingCount() => 0;

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
