using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;
using Pigeon;
using UnityEngine.InputSystem;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[MycoMod(null, ModFlags.IsClientSide)]
public class CompareUpgradesPlugin : BaseUnityPlugin
{
    public const string PluginGUID = "sparroh.compareupgrades";
    public const string PluginName = "CompareUpgrades";
    public const string PluginVersion = "1.0.0";

    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        var harmony = new Harmony(PluginGUID);
        harmony.PatchAll(typeof(CompareUpgradesPlugin));
        Logger.LogInfo($"{PluginName} loaded successfully.");
    }

    private static GearUpgradeUI LockedUpgrade = null;
    private static GearUpgradeUI HoveredUpgrade = null;
    private static HoverInfoDisplay LockedDisplayInstance = null;
    private static bool IsComparisonModeActive = false;
    private static bool IsDisplayCreated = false;

    private static bool JustCalledShowInfo = false;

    [HarmonyPatch(typeof(HoverInfoDisplay), "ShowInfo")]
    [HarmonyPrefix]
    private static void HoverInfoDisplayShowInfoPrefix(HoverInfoDisplay __instance, HoverInfo info)
    {
        if (__instance == LockedDisplayInstance)
        {
            JustCalledShowInfo = true;
        }
    }

    [HarmonyPatch(typeof(HoverInfoDisplay), "ShowInfo")]
    [HarmonyPostfix]
    private static void HoverInfoDisplayShowInfoPostfix(HoverInfoDisplay __instance, HoverInfo info)
    {
        if (__instance == LockedDisplayInstance)
        {
            JustCalledShowInfo = false;
        }
    }

    [HarmonyPatch(typeof(HoverInfoDisplay), "UpdatePosition")]
    [HarmonyPrefix]
    private static bool HoverInfoDisplayUpdatePositionPrefix(HoverInfoDisplay __instance)
    {
        if (__instance == LockedDisplayInstance && IsComparisonModeActive && !JustCalledShowInfo)
        {
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(GearDetailsWindow), "Update")]
    [HarmonyPostfix]
    private static void GearDetailsWindowUpdatePostfix(GearDetailsWindow __instance)
    {

        UpdateCurrentlyHoveredUpgrade(__instance);

        HandleComparisonLogic(__instance);

    if (IsComparisonModeActive && LockedDisplayInstance != null && LockedUpgrade != null)
    {
        LockedDisplayInstance.ShowInfo(LockedUpgrade);

        PositionLockedDisplaySynchronously();
    }
    }

    [HarmonyPatch(typeof(GameManager), "Update")]
    [HarmonyPostfix]
    private static void GameManagerUpdatePostfix()
    {
        if (Menu.Instance != null &&
            Menu.Instance.WindowSystem.GetTop() is GearDetailsWindow &&
            Keyboard.current != null &&
            Keyboard.current.cKey.wasPressedThisFrame)
        {
            HandleCompareToggle((Menu.Instance.WindowSystem.GetTop() as GearDetailsWindow));
        }
    }

    private static void HandleCompareToggle(GearDetailsWindow gearDetailsWindow)
    {
        GearUpgradeUI currentlyHovered = GetCurrentlyHoveredUpgrade(gearDetailsWindow);

        if (currentlyHovered != null)
        {
            if (LockedUpgrade == null)
            {
                LockedUpgrade = currentlyHovered;
                Logger.LogInfo($"Locked upgrade for comparison: {LockedUpgrade.Upgrade.Upgrade.GetInstanceName(LockedUpgrade.Upgrade.Seed)}");
                IsComparisonModeActive = false;
            }
            else if (LockedUpgrade == currentlyHovered)
            {
                LockedUpgrade = null;
                Logger.LogInfo("Unlocked upgrade comparison");
                IsComparisonModeActive = false;
            }
            else
            {
                IsComparisonModeActive = true;
            }
        }
    }

    private static GearUpgradeUI GetCurrentlyHoveredUpgrade(GearDetailsWindow gearDetailsWindow)
    {
        if (UIRaycaster.RaycastForComponent<GearUpgradeUI>(out var hoveredUpgrade))
        {
            return hoveredUpgrade;
        }
        return null;
    }

    private static void UpdateCurrentlyHoveredUpgrade(GearDetailsWindow gearDetailsWindow)
    {
        HoveredUpgrade = GetCurrentlyHoveredUpgrade(gearDetailsWindow);
    }

    private static void HandleComparisonLogic(GearDetailsWindow gearDetailsWindow)
    {
        if (LockedUpgrade != null && HoveredUpgrade != null && LockedUpgrade != HoveredUpgrade)
        {
            if (!IsComparisonModeActive)
            {
                IsComparisonModeActive = true;

                if (!IsDisplayCreated)
                {
                    LockedDisplayInstance = GameObject.Instantiate(HoverInfoDisplay.Instance, HoverInfoDisplay.Instance.transform.parent);
                    IsDisplayCreated = true;

                    PositionLockedDisplayCompanion();
                }
            }
        }
        else if (LockedUpgrade == null)
        {
            if (IsComparisonModeActive)
            {
                IsComparisonModeActive = false;
                if (LockedDisplayInstance != null)
                {
                    LockedDisplayInstance.Deactivate();
                    LockedDisplayInstance = null;
                }
                IsDisplayCreated = false;
            }
        }
    }

    private static void PositionLockedDisplayCompanion()
    {
        var mainRect = HoverInfoDisplay.Instance.RectTransform;
        var canvasRect = mainRect.parent as RectTransform;

        if (canvasRect == null)
        {
            LockedDisplayInstance.RectTransform.anchoredPosition = mainRect.anchoredPosition + new Vector2(mainRect.sizeDelta.x + 50f, 0f);
            return;
        }

        Vector2 canvasSize = canvasRect.rect.size;

        Vector2 preferredPos = mainRect.anchoredPosition + new Vector2(mainRect.sizeDelta.x + 50f, 0f);

        if (IsCompanionPositionValid(preferredPos, LockedDisplayInstance.RectTransform.sizeDelta, canvasSize, mainRect))
        {
            LockedDisplayInstance.RectTransform.anchoredPosition = preferredPos;
            return;
        }

        Vector2[] fallbackPositions = new Vector2[]
        {
            mainRect.anchoredPosition - new Vector2(LockedDisplayInstance.RectTransform.sizeDelta.x + 50f, 0f),
            mainRect.anchoredPosition + new Vector2(0f, mainRect.sizeDelta.y + 50f),
            mainRect.anchoredPosition - new Vector2(0f, LockedDisplayInstance.RectTransform.sizeDelta.y + 50f),
        };

        foreach (var testPos in fallbackPositions)
        {
            if (IsCompanionPositionValid(testPos, LockedDisplayInstance.RectTransform.sizeDelta, canvasSize, mainRect))
            {
                LockedDisplayInstance.RectTransform.anchoredPosition = testPos;
                return;
            }
        }

        preferredPos = ClampToScreenBoundsCompanion(preferredPos, LockedDisplayInstance.RectTransform.sizeDelta, canvasSize, mainRect);
        LockedDisplayInstance.RectTransform.anchoredPosition = preferredPos;
    }

    private static bool IsCompanionPositionValid(Vector2 position, Vector2 size, Vector2 canvasSize, RectTransform mainRect)
    {
        if (position.x < -canvasSize.x / 2f || position.x + size.x > canvasSize.x / 2f ||
            position.y - size.y < -canvasSize.y / 2f || position.y > canvasSize.y / 2f)
        {
            return false;
        }

        Rect lockedRect = new Rect(position - size / 2f, size);
        Rect mainRectBounds = new Rect(mainRect.anchoredPosition - mainRect.sizeDelta / 2f, mainRect.sizeDelta);
        mainRectBounds = new Rect(mainRectBounds.xMin - 30f, mainRectBounds.yMin - 30f,
                                 mainRectBounds.width + 60f, mainRectBounds.height + 60f);

        return !lockedRect.Overlaps(mainRectBounds);
    }

    private static Vector2 ClampToScreenBoundsCompanion(Vector2 position, Vector2 size, Vector2 canvasSize, RectTransform mainRect)
    {
        float halfCanvasX = canvasSize.x / 2f;
        float halfCanvasY = canvasSize.y / 2f;

        Vector2 mainCenter = mainRect.anchoredPosition;

        position.x = Mathf.Clamp(position.x, -halfCanvasX, halfCanvasX - size.x);
        position.y = Mathf.Clamp(position.y, -halfCanvasY + size.y, halfCanvasY);

        if (Mathf.Abs(position.x - mainCenter.x) > 200f)
        {
            if (mainCenter.y + size.y + 100f <= halfCanvasY)
            {
                position.y = mainCenter.y + 100f;
            }
            else if (mainCenter.y - size.y - 100f >= -halfCanvasY)
            {
                position.y = mainCenter.y - 100f;
            }
        }

        return position;
    }



    private static void PositionLockedDisplaySynchronously()
    {
        var canvasRect = HoverInfoDisplay.Instance.RectTransform.parent as RectTransform;

        if (canvasRect == null)
        {
            return;
        }

        Vector2 canvasSize = new Vector2(canvasRect.rect.width, canvasRect.rect.height);

        Vector2 screenPoint = PlayerInput.Controls.Menu.Point.ReadValue<Vector2>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, HoverInfoDisplay.Instance.RectTransform.GetComponentInParent<Canvas>().worldCamera, out var localPoint);

        localPoint.y += (HoverInfoDisplay.Instance.RectTransform.sizeDelta.y * 0.5f) + 80f;

        Vector2 bounds = new Vector2(canvasRect.rect.width, canvasRect.rect.height);
        if (localPoint.y + (HoverInfoDisplay.Instance.RectTransform.sizeDelta.y * 0.5f) + 80f > bounds.y * 0.5f)
        {
            float num = localPoint.y - (HoverInfoDisplay.Instance.RectTransform.sizeDelta.y * 0.5f) - 80f;
            num -= (HoverInfoDisplay.Instance.RectTransform.sizeDelta.y * 0.5f) + 40f;
            if (num - (HoverInfoDisplay.Instance.RectTransform.sizeDelta.y * 0.5f) - 80f < (0f - bounds.y) * 0.5f)
            {
                num = localPoint.y - (localPoint.y + (HoverInfoDisplay.Instance.RectTransform.sizeDelta.y * 0.5f) + 80f - bounds.y * 0.5f);
            }
            localPoint.y = num;
        }

        Vector2 preferredPos = localPoint + new Vector2(HoverInfoDisplay.Instance.RectTransform.sizeDelta.x + 50f, 0f);
        Vector2 companionSize = LockedDisplayInstance.RectTransform.sizeDelta;

        if (IsCompanionPositionValidSync(preferredPos, companionSize, canvasSize, localPoint, HoverInfoDisplay.Instance.RectTransform.sizeDelta))
        {
            LockedDisplayInstance.RectTransform.anchoredPosition = preferredPos;
            return;
        }

        Vector2 leftPos = localPoint - new Vector2(companionSize.x + 50f, 0f);
        if (IsCompanionPositionValidSync(leftPos, companionSize, canvasSize, localPoint, HoverInfoDisplay.Instance.RectTransform.sizeDelta))
        {
            LockedDisplayInstance.RectTransform.anchoredPosition = leftPos;
            return;
        }

        Vector2 abovePos = localPoint + new Vector2(0f, HoverInfoDisplay.Instance.RectTransform.sizeDelta.y + 50f);
        if (IsCompanionPositionValidSync(abovePos, companionSize, canvasSize, localPoint, HoverInfoDisplay.Instance.RectTransform.sizeDelta))
        {
            LockedDisplayInstance.RectTransform.anchoredPosition = abovePos;
            return;
        }

        Vector2 belowPos = localPoint - new Vector2(0f, companionSize.y + 50f);
        if (IsCompanionPositionValidSync(belowPos, companionSize, canvasSize, localPoint, HoverInfoDisplay.Instance.RectTransform.sizeDelta))
        {
            LockedDisplayInstance.RectTransform.anchoredPosition = belowPos;
            return;
        }

        preferredPos = ClampToScreenBoundsSync(preferredPos, companionSize, canvasSize, localPoint, HoverInfoDisplay.Instance.RectTransform.sizeDelta);
        LockedDisplayInstance.RectTransform.anchoredPosition = preferredPos;
    }

    private static bool IsCompanionPositionValidSync(Vector2 position, Vector2 size, Vector2 canvasSize, Vector2 mainDisplayPos, Vector2 mainDisplaySize)
    {
        if (position.x < -canvasSize.x / 2f || position.x + size.x > canvasSize.x / 2f ||
            position.y - size.y < -canvasSize.y / 2f || position.y > canvasSize.y / 2f)
        {
            return false;
        }

        Rect companionRect = new Rect(position - size / 2f, size);
        Rect mainRectBounds = new Rect(mainDisplayPos - mainDisplaySize / 2f, mainDisplaySize);
        mainRectBounds = new Rect(mainRectBounds.xMin - 30f, mainRectBounds.yMin - 30f,
                                 mainRectBounds.width + 60f, mainRectBounds.height + 60f);

        return !companionRect.Overlaps(mainRectBounds);
    }

    private static Vector2 ClampToScreenBoundsSync(Vector2 position, Vector2 size, Vector2 canvasSize, Vector2 mainDisplayPos, Vector2 mainDisplaySize)
    {
        float halfCanvasX = canvasSize.x / 2f;
        float halfCanvasY = canvasSize.y / 2f;

        position.x = Mathf.Clamp(position.x, -halfCanvasX, halfCanvasX - size.x);
        position.y = Mathf.Clamp(position.y, -halfCanvasY + size.y, halfCanvasY);

        if (Mathf.Abs(position.x - mainDisplayPos.x) > 150f)
        {
            if (mainDisplayPos.y + mainDisplaySize.y + size.y + 100f <= halfCanvasY)
            {
                position.y = mainDisplayPos.y + mainDisplaySize.y + 50f;
                position.x = Mathf.Clamp(position.x, -halfCanvasX, halfCanvasX - size.x);
            }
            else if (mainDisplayPos.y - size.y - 100f >= -halfCanvasY)
            {
                position.y = mainDisplayPos.y - size.y - 50f;
                position.x = Mathf.Clamp(position.x, -halfCanvasX, halfCanvasX - size.x);
            }
        }

        return position;
    }



    [HarmonyPatch(typeof(GearDetailsWindow), "Setup")]
    [HarmonyPostfix]
    private static void GearDetailsWindowSetupPostfix()
    {
        LockedUpgrade = null;
        IsComparisonModeActive = false;
        if (LockedDisplayInstance != null)
        {
            LockedDisplayInstance.Deactivate();
            LockedDisplayInstance = null;
        }
        IsDisplayCreated = false;
    }

    [HarmonyPatch(typeof(GearDetailsWindow), "OnCloseCallback")]
    [HarmonyPostfix]
    private static void GearDetailsWindowOnCloseCallbackPostfix()
    {
        LockedUpgrade = null;
        IsComparisonModeActive = false;
        if (LockedDisplayInstance != null)
        {
            LockedDisplayInstance.Deactivate();
            LockedDisplayInstance = null;
        }
        IsDisplayCreated = false;
    }

    [HarmonyPatch(typeof(GearUpgradeUI), "OnPointerEnter")]
    [HarmonyPostfix]
    private static void GearUpgradeUIOnPointerEnterPostfix(GearUpgradeUI __instance)
    {
        HoveredUpgrade = __instance;
    }

    [HarmonyPatch(typeof(GearUpgradeUI), "OnPointerExit")]
    [HarmonyPostfix]
    private static void GearUpgradeUIOnPointerExitPostfix()
    {
        HoveredUpgrade = null;
    }
}
