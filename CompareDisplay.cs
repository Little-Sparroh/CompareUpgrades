using System;
using System.Reflection;
using HarmonyLib;
using Pigeon;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns the cloned HoverInfoDisplay used for the locked upgrade tooltip.
/// Restores the game singleton after Instantiate and positions beside the main tooltip.
/// </summary>
public static class CompareDisplay
{
    private const float Gap = 24f;
    private const float EdgePadding = 20f;

    private static HoverInfoDisplay clone;
    private static LockedUpgradeProxy proxy;
    private static GameObject proxyRoot;
    private static bool visible;
    private static UpgradeInstance shownUpgrade;

    public static HoverInfoDisplay Clone => clone;
    public static bool IsVisible => visible;

    public static bool IsCompareInstance(HoverInfoDisplay display)
    {
        return clone != null && display == clone;
    }

    public static void EnsureCreated()
    {
        if (clone != null)
            return;

        var main = HoverInfoDisplay.Instance;
        if (main == null)
            return;

        var parent = main.transform.parent;
        if (parent == null)
            return;

        // Instantiate runs Awake, which steals HoverInfoDisplay.Instance.
        clone = UnityEngine.Object.Instantiate(main, parent);
        clone.gameObject.name = "CompareUpgrades_HoverInfoDisplay";

        RestoreMainInstance(main);

        // Comparison panel must never capture pointer / UI raycasts.
        DisableRaycasts(clone.gameObject);

        // Start hidden; Deactivate is safe if already inactive.
        try
        {
            clone.Deactivate();
        }
        catch
        {
            clone.gameObject.SetActive(false);
        }

        EnsureProxy();
        CompareUpgradesPlugin.Log?.LogInfo("Created comparison hover display.");
    }

    public static void Show(UpgradeInstance upgrade)
    {
        if (upgrade == null)
            return;

        EnsureCreated();
        if (clone == null || proxy == null)
            return;

        bool needsRefresh = !visible || shownUpgrade != upgrade || !clone.gameObject.activeSelf;
        proxy.SetLockedUpgrade(upgrade);
        shownUpgrade = upgrade;

        if (needsRefresh)
        {
            try
            {
                clone.ShowInfo(proxy);
            }
            catch (Exception ex)
            {
                CompareUpgradesPlugin.Log?.LogWarning($"ShowInfo on compare display failed: {ex.Message}");
                return;
            }
        }

        visible = clone.gameObject.activeSelf;
        if (visible)
            PositionBesideMain();
    }

    public static void RefreshIfVisible()
    {
        if (!visible || clone == null || proxy == null || shownUpgrade == null)
            return;

        try
        {
            clone.Refresh();
            PositionBesideMain();
        }
        catch (Exception ex)
        {
            CompareUpgradesPlugin.Log?.LogWarning($"Refresh compare display failed: {ex.Message}");
        }
    }

    public static void Hide()
    {
        if (clone == null)
        {
            visible = false;
            shownUpgrade = null;
            return;
        }

        if (clone.gameObject.activeSelf)
        {
            try
            {
                clone.Deactivate();
            }
            catch
            {
                clone.gameObject.SetActive(false);
            }
        }

        visible = false;
        shownUpgrade = null;
    }

    public static void TickPosition()
    {
        if (!visible || clone == null || !clone.gameObject.activeSelf)
            return;

        PositionBesideMain();
    }

    public static void Destroy()
    {
        Hide();

        if (clone != null)
        {
            UnityEngine.Object.Destroy(clone.gameObject);
            clone = null;
        }

        if (proxyRoot != null)
        {
            UnityEngine.Object.Destroy(proxyRoot);
            proxyRoot = null;
            proxy = null;
        }

        shownUpgrade = null;
        visible = false;
    }

    private static void EnsureProxy()
    {
        if (proxy != null)
            return;

        proxyRoot = new GameObject("CompareUpgrades_LockedProxy");
        UnityEngine.Object.DontDestroyOnLoad(proxyRoot);
        proxyRoot.hideFlags = HideFlags.HideAndDontSave;

        // Keep active so HoverInfoDisplay.Update on the clone does not auto-deactivate.
        proxy = proxyRoot.AddComponent<LockedUpgradeProxy>();
    }

    private static void RestoreMainInstance(HoverInfoDisplay main)
    {
        try
        {
            PropertyInfo prop = AccessTools.Property(typeof(HoverInfoDisplay), "Instance");
            if (prop != null)
            {
                prop.SetValue(null, main, null);
                return;
            }
        }
        catch (Exception ex)
        {
            CompareUpgradesPlugin.Log?.LogWarning($"Failed to restore HoverInfoDisplay.Instance via AccessTools: {ex.Message}");
        }

        // Fallback reflection
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
        for (int i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = false;

        var groups = root.GetComponentsInChildren<CanvasGroup>(true);
        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].blocksRaycasts = false;
            groups[i].interactable = false;
        }
    }

    private static void PositionBesideMain()
    {
        var main = HoverInfoDisplay.Instance;
        if (main == null || clone == null)
            return;

        // Never position relative to ourselves if Instance was stolen somehow.
        if (main == clone)
            return;

        if (!main.gameObject.activeSelf)
            return;

        RectTransform mainRect = main.RectTransform;
        RectTransform cloneRect = clone.RectTransform;
        if (mainRect == null || cloneRect == null)
            return;

        RectTransform canvasRect = mainRect.parent as RectTransform;
        if (canvasRect == null)
            return;

        Vector2 mainPos = mainRect.anchoredPosition;
        Vector2 mainSize = mainRect.sizeDelta;
        Vector2 cloneSize = cloneRect.sizeDelta;
        Vector2 bounds = canvasRect.rect.size;

        // Prefer right of main tooltip; fall back to left; then clamp.
        Vector2 right = new Vector2(mainPos.x + (mainSize.x + cloneSize.x) * 0.5f + Gap, mainPos.y);
        Vector2 left = new Vector2(mainPos.x - (mainSize.x + cloneSize.x) * 0.5f - Gap, mainPos.y);

        Vector2 chosen = Fits(right, cloneSize, bounds) ? right
            : Fits(left, cloneSize, bounds) ? left
            : Clamp(right, cloneSize, bounds);

        // Keep vertical alignment with main, clamped to canvas.
        chosen.y = ClampY(chosen.y, cloneSize.y, bounds.y);

        cloneRect.anchoredPosition = chosen;
    }

    private static bool Fits(Vector2 center, Vector2 size, Vector2 bounds)
    {
        float halfW = bounds.x * 0.5f;
        float halfH = bounds.y * 0.5f;
        float left = center.x - size.x * 0.5f;
        float right = center.x + size.x * 0.5f;
        float bottom = center.y - size.y * 0.5f;
        float top = center.y + size.y * 0.5f;

        return left >= -halfW + EdgePadding
            && right <= halfW - EdgePadding
            && bottom >= -halfH + EdgePadding
            && top <= halfH - EdgePadding;
    }

    private static Vector2 Clamp(Vector2 center, Vector2 size, Vector2 bounds)
    {
        float halfW = bounds.x * 0.5f;
        float halfH = bounds.y * 0.5f;
        center.x = Mathf.Clamp(center.x, -halfW + size.x * 0.5f + EdgePadding, halfW - size.x * 0.5f - EdgePadding);
        center.y = Mathf.Clamp(center.y, -halfH + size.y * 0.5f + EdgePadding, halfH - size.y * 0.5f - EdgePadding);
        return center;
    }

    private static float ClampY(float y, float height, float boundsY)
    {
        float halfH = boundsY * 0.5f;
        return Mathf.Clamp(y, -halfH + height * 0.5f + EdgePadding, halfH - height * 0.5f - EdgePadding);
    }
}
