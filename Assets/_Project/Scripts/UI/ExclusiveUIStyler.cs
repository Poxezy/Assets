using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Runtime dark-gold restyle for existing UGUI trees.
/// Call Apply(root) once from menu / pause / leaderboard Start.
/// </summary>
public static class ExclusiveUIStyler
{
    public static void Apply(Transform root)
    {
        if (root == null) return;

        foreach (var img in root.GetComponentsInChildren<Image>(true))
            StyleImage(img);

        foreach (var btn in root.GetComponentsInChildren<Button>(true))
            StyleButton(btn);

        foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
            StyleText(tmp);
    }

    static void StyleImage(Image img)
    {
        if (img == null) return;
        string n = img.gameObject.name;

        if (n.IndexOf("Background", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            img.color = UITheme.BgDeep;
            return;
        }

        if (img.GetComponent<Button>() != null)
        {
            img.color = Color.white;
            return;
        }

        // Quest surfaces — keep intentional hierarchy
        if (n.IndexOf("QuestAccent", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("ToastAccent", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("HudAccent", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("RewardAccent", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("CompassNeedle", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            img.color = UITheme.Gold;
            return;
        }

        if (n.IndexOf("QuestInner", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("CardInner", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("NextStepChip", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            img.color = UITheme.CardInner;
            return;
        }

        if (n.IndexOf("QuestCard", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            img.color = UITheme.PanelDark;
            EnsureOutline(img.gameObject, UITheme.Gold, 1.5f);
            return;
        }

        if (n.IndexOf("QuestToast", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("HDD", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("HUD", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("ProfileChip", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("CompassPanel", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            img.color = UITheme.HudPanel;
            EnsureOutline(img.gameObject, UITheme.Gold, 1.25f);
            return;
        }

        if (n.IndexOf("ProgressTrack", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            img.color = UITheme.ProgressTrack;
            return;
        }

        if (n.IndexOf("ProgressFill", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            img.color = UITheme.ProgressFill;
            return;
        }

        if (n.IndexOf("Reward", System.StringComparison.OrdinalIgnoreCase) >= 0
            && n.IndexOf("Panel", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            img.color = UITheme.PanelDark;
            EnsureOutline(img.gameObject, UITheme.Gold, 1.5f);
            return;
        }

        if (n.IndexOf("Panel", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("Menupanel", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("Card", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            bool fullOverlay = n.IndexOf("Pause", System.StringComparison.OrdinalIgnoreCase) >= 0;
            img.color = fullOverlay
                ? UITheme.DimOverlay
                : UITheme.PanelDark;
            EnsureOutline(img.gameObject, UITheme.Gold, fullOverlay ? 0f : 1.75f);
        }
    }

    static void StyleButton(Button btn)
    {
        if (btn == null) return;

        btn.transition = Selectable.Transition.ColorTint;
        btn.colors = UITheme.ButtonColors();

        var img = btn.targetGraphic as Image;
        if (img != null) img.color = Color.white;

        EnsureOutline(btn.gameObject, UITheme.Gold, 1.25f);
        EnsurePressFeedback(btn);

        foreach (var tmp in btn.GetComponentsInChildren<TMP_Text>(true))
        {
            tmp.color = UITheme.Cream;
            tmp.fontStyle = FontStyles.Bold;
            UITheme.FitText(tmp, tmp.fontSize > 0 ? tmp.fontSize : UITheme.SizeButton, wrap: false);
        }
    }

    static void EnsurePressFeedback(Button btn)
    {
        if (btn.GetComponent<UIButtonPressFx>() != null) return;
        btn.gameObject.AddComponent<UIButtonPressFx>();
    }

    static void StyleText(TMP_Text tmp)
    {
        if (tmp == null) return;
        if (tmp.GetComponentInParent<Button>() != null) return;
        if (tmp.GetComponentInParent<TMP_InputField>() != null) return;

        string n = tmp.gameObject.name;
        float size = tmp.fontSize > 0 ? tmp.fontSize : 16f;

        if (n.IndexOf("Title", System.StringComparison.OrdinalIgnoreCase) >= 0
            || size >= 48f)
        {
            tmp.color = UITheme.Gold;
            tmp.fontStyle = FontStyles.Bold;
            UITheme.FitText(tmp, Mathf.Min(size, 36f), wrap: false);
            return;
        }

        if (n.IndexOf("Header", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("Subtitle", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            tmp.color = UITheme.GoldSoft;
            UITheme.FitText(tmp, size, wrap: false);
            return;
        }

        if (n.IndexOf("Score", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("Level", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("Badge", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("Reward", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("ProfileName", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("QuestBody", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("QuestDesc", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("QuestObj", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            tmp.color = UITheme.Cream;
            bool wrap = n.IndexOf("Reward", System.StringComparison.OrdinalIgnoreCase) >= 0
                || n.IndexOf("QuestBody", System.StringComparison.OrdinalIgnoreCase) >= 0
                || n.IndexOf("QuestDesc", System.StringComparison.OrdinalIgnoreCase) >= 0
                || n.IndexOf("QuestObj", System.StringComparison.OrdinalIgnoreCase) >= 0;
            UITheme.FitText(tmp, Mathf.Min(size, 20f), wrap);
            return;
        }

        if (n.IndexOf("QuestHeader", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("QuestTitle", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("NextStep", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            tmp.color = UITheme.Gold;
            tmp.fontStyle = FontStyles.Bold;
            UITheme.FitText(tmp, Mathf.Min(size, 18f), false);
            return;
        }

        if (n.IndexOf("Hint", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("Footer", System.StringComparison.OrdinalIgnoreCase) >= 0
            || n.IndexOf("Muted", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            tmp.color = UITheme.Muted;
            UITheme.FitText(tmp, Mathf.Min(size, 13f), true);
            return;
        }

        // Force light text if author left dark-on-dark
        if (tmp.color.r < 0.45f && tmp.color.g < 0.45f && tmp.color.b < 0.45f)
            tmp.color = UITheme.Cream;

        if (!tmp.enableAutoSizing)
            UITheme.FitText(tmp, size, wrap: true);
    }

    static void EnsureOutline(GameObject go, Color color, float distance)
    {
        if (distance <= 0f) return;
        var outline = go.GetComponent<Outline>();
        if (outline == null) outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(color.r, color.g, color.b, 0.75f);
        outline.effectDistance = new Vector2(distance, -distance);
        outline.useGraphicAlpha = true;
    }
}

/// <summary>Hover + press scale feedback (unscaled). Clear interactive states.</summary>
public class UIButtonPressFx : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rt;
    Button btn;
    bool hovered;
    bool pressed;
    const float HoverScale = 1.035f;
    const float PressScale = 0.96f;

    void Awake()
    {
        rt = transform as RectTransform;
        btn = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactable()) return;
        hovered = true;
        if (!pressed) ApplyScale(HoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        pressed = false;
        ApplyScale(1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable()) return;
        pressed = true;
        ApplyScale(PressScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
        ApplyScale(hovered && Interactable() ? HoverScale : 1f);
    }

    bool Interactable()
    {
        return btn == null || btn.IsInteractable();
    }

    void ApplyScale(float s)
    {
        if (rt == null) return;
        rt.localScale = Vector3.one * s;
    }

    void OnDisable()
    {
        hovered = false;
        pressed = false;
        if (rt != null) rt.localScale = Vector3.one;
    }
}
