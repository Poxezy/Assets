using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dark gold luxury palette — high contrast text on deep panels.
/// </summary>
public static class UITheme
{
    // Spacing tokens
    public const float SpaceXs = 6f;
    public const float SpaceSm = 8f;
    public const float SpaceMd = 12f;
    public const float SpaceLg = 16f;
    public const float SpaceXl = 24f;

    // Type sizes
    public const float SizeTitle = 22f;
    public const float SizeHeader = 16f;
    public const float SizeBody = 14f;
    public const float SizeHint = 12f;
    public const float SizeButton = 16f;

    // Deep backgrounds (never light — keeps cream/gold readable)
    public static readonly Color BgDeep = new Color(0.035f, 0.030f, 0.05f, 0.94f);
    public static readonly Color PanelDark = new Color(0.07f, 0.06f, 0.10f, 0.97f);
    public static readonly Color PanelSoft = new Color(0.09f, 0.08f, 0.12f, 0.90f);
    public static readonly Color HudPanel = new Color(0.055f, 0.05f, 0.08f, 0.95f);
    public static readonly Color DimOverlay = new Color(0.02f, 0.02f, 0.03f, 0.84f);

    // Text hierarchy — cream on dark = max readability
    public static readonly Color Gold = new Color(0.90f, 0.76f, 0.32f, 1f);
    public static readonly Color GoldSoft = new Color(0.96f, 0.88f, 0.58f, 1f);
    public static readonly Color Cream = new Color(0.98f, 0.96f, 0.92f, 1f);
    public static readonly Color TextOnGold = new Color(0.08f, 0.06f, 0.04f, 1f);
    public static readonly Color Muted = new Color(0.78f, 0.74f, 0.66f, 1f);
    public static readonly Color Danger = new Color(0.92f, 0.38f, 0.36f, 1f);
    public static readonly Color Success = new Color(0.48f, 0.86f, 0.58f, 1f);
    public static readonly Color AccentLine = new Color(0.90f, 0.76f, 0.32f, 0.65f);

    // Surfaces
    public static readonly Color CardInner = new Color(0.10f, 0.09f, 0.13f, 1f);
    public static readonly Color RowBg = new Color(0.12f, 0.11f, 0.15f, 0.95f);
    public static readonly Color ProgressTrack = new Color(0.18f, 0.16f, 0.22f, 1f);
    public static readonly Color ProgressFill = new Color(0.90f, 0.76f, 0.32f, 1f);
    public static readonly Color ButtonNormal = new Color(0.14f, 0.12f, 0.18f, 1f);
    public static readonly Color ButtonPressed = new Color(0.50f, 0.40f, 0.12f, 1f);
    public static readonly Color ButtonDisabled = new Color(0.22f, 0.20f, 0.24f, 0.55f);

    public static ColorBlock ButtonColors()
    {
        return new ColorBlock
        {
            normalColor = ButtonNormal,
            highlightedColor = Gold,
            pressedColor = ButtonPressed,
            selectedColor = GoldSoft,
            disabledColor = ButtonDisabled,
            colorMultiplier = 1f,
            fadeDuration = 0.10f
        };
    }

    /// <summary>Clamp TMP so text never spills card bounds.</summary>
    public static void FitText(TMPro.TMP_Text tmp, float maxSize, bool wrap = true)
    {
        if (tmp == null) return;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = maxSize;
        tmp.fontSizeMin = Mathf.Max(10f, maxSize * 0.55f);
        tmp.overflowMode = TMPro.TextOverflowModes.Ellipsis;
        tmp.textWrappingMode = wrap
            ? TMPro.TextWrappingModes.Normal
            : TMPro.TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
    }

    public static void StyleBody(TMPro.TMP_Text tmp, float size = SizeBody)
    {
        if (tmp == null) return;
        tmp.color = Cream;
        FitText(tmp, size, true);
    }

    public static void StyleTitle(TMPro.TMP_Text tmp, float size = SizeTitle)
    {
        if (tmp == null) return;
        tmp.color = Gold;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        FitText(tmp, size, false);
    }

    public static void StyleMuted(TMPro.TMP_Text tmp, float size = SizeHint)
    {
        if (tmp == null) return;
        tmp.color = Muted;
        FitText(tmp, size, true);
    }

    public static void StyleHeader(TMPro.TMP_Text tmp, float size = SizeHeader)
    {
        if (tmp == null) return;
        tmp.color = GoldSoft;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        FitText(tmp, size, false);
    }

    public static void ApplyStandardScaler(CanvasScaler scaler)
    {
        if (scaler == null) return;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }
}
