using HarmonyLib;
using Pigeon;

namespace CompareUpgrades;

internal static class ComparePatches
{
    [HarmonyPatch(typeof(HoverInfoDisplay), "UpdatePosition")]
    [HarmonyPrefix]
    private static bool UpdatePositionPrefix(HoverInfoDisplay __instance)
    {
        if (CompareDisplay.IsCompareInstance(__instance))
            return false;
        return true;
    }

    [HarmonyPatch(typeof(HoverInfoDisplay), nameof(HoverInfoDisplay.Refresh))]
    [HarmonyPostfix]
    private static void RefreshPostfix(HoverInfoDisplay __instance)
    {
        if (CompareDisplay.IsCompareInstance(__instance))
            return;

        CompareController.OnMainHoverRefreshed();
    }

    [HarmonyPatch(typeof(GearDetailsWindow), "Setup")]
    [HarmonyPostfix]
    private static void GearDetailsSetupPostfix()
    {
        CompareController.OnGearWindowReset();
    }

    [HarmonyPatch(typeof(GearDetailsWindow), "OnCloseCallback")]
    [HarmonyPostfix]
    private static void GearDetailsClosePostfix()
    {
        CompareController.OnGearWindowReset();
    }
}