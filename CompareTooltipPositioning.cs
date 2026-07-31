using Pigeon;
using UnityEngine;

namespace CompareUpgrades;

public static class CompareTooltipPositioning
{
    private const float Gap = 24f;
    private const float EdgePadding = 20f;

    public static void PositionBesideMain(HoverInfoDisplay main, HoverInfoDisplay clone)
    {
        if (main == null || clone == null)
            return;

        if (main == clone)
            return;

        if (!main.gameObject.activeSelf)
            return;

        var mainRect = main.RectTransform;
        var cloneRect = clone.RectTransform;
        if (mainRect == null || cloneRect == null)
            return;

        var canvasRect = mainRect.parent as RectTransform;
        if (canvasRect == null)
            return;

        var mainPos = mainRect.anchoredPosition;
        var mainSize = mainRect.sizeDelta;
        var cloneSize = cloneRect.sizeDelta;
        var bounds = canvasRect.rect.size;

        var right = new Vector2(mainPos.x + (mainSize.x + cloneSize.x) * 0.5f + Gap, mainPos.y);
        var left = new Vector2(mainPos.x - (mainSize.x + cloneSize.x) * 0.5f - Gap, mainPos.y);

        var chosen = Fits(right, cloneSize, bounds) ? right
            : Fits(left, cloneSize, bounds) ? left
            : Clamp(right, cloneSize, bounds);

        chosen.y = ClampY(chosen.y, cloneSize.y, bounds.y);

        cloneRect.anchoredPosition = chosen;
    }

    private static bool Fits(Vector2 center, Vector2 size, Vector2 bounds)
    {
        var halfW = bounds.x * 0.5f;
        var halfH = bounds.y * 0.5f;
        var left = center.x - size.x * 0.5f;
        var right = center.x + size.x * 0.5f;
        var bottom = center.y - size.y * 0.5f;
        var top = center.y + size.y * 0.5f;

        return left >= -halfW + EdgePadding
               && right <= halfW - EdgePadding
               && bottom >= -halfH + EdgePadding
               && top <= halfH - EdgePadding;
    }

    private static Vector2 Clamp(Vector2 center, Vector2 size, Vector2 bounds)
    {
        var halfW = bounds.x * 0.5f;
        var halfH = bounds.y * 0.5f;
        center.x = Mathf.Clamp(center.x, -halfW + size.x * 0.5f + EdgePadding, halfW - size.x * 0.5f - EdgePadding);
        center.y = Mathf.Clamp(center.y, -halfH + size.y * 0.5f + EdgePadding, halfH - size.y * 0.5f - EdgePadding);
        return center;
    }

    private static float ClampY(float y, float height, float boundsY)
    {
        var halfH = boundsY * 0.5f;
        return Mathf.Clamp(y, -halfH + height * 0.5f + EdgePadding, halfH - height * 0.5f - EdgePadding);
    }
}