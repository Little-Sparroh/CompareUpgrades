using HarmonyLib;
using Pigeon;

/// <summary>
/// Minimal Harmony surface:
/// - Freeze positioning on the comparison clone (we place it ourselves)
/// - Reset lock when the gear details window opens/closes
/// - Keep locked panel content in sync when the main tooltip refreshes
/// </summary>
internal static class ComparePatches
{
    [HarmonyPatch(typeof(HoverInfoDisplay), "UpdatePosition")]
    [HarmonyPrefix]
    private static bool UpdatePositionPrefix(HoverInfoDisplay __instance)
    {
        // Let the game position the real tooltip; we own the clone.
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
