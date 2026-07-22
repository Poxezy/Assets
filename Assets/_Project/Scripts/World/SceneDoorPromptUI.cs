using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Bottom-center prompt for SceneDoor — premium chip, text clamped.</summary>
public class SceneDoorPromptUI : MonoBehaviour
{
    Canvas canvas;
    TMP_Text label;
    GameObject root;

    void Awake()
    {
        Build();
        Hide();
    }

    public void Show(string message)
    {
        if (root == null) Build();
        if (label != null)
        {
            label.text = message;
            UITheme.FitText(label, 20f, wrap: false);
        }
        root.SetActive(true);
        var cg = root.GetComponent<CanvasGroup>();
        if (cg == null) cg = root.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        UIMotion.FadeCanvas(cg, 1f, 0.12f);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    void Build()
    {
        root = new GameObject("PromptRoot", typeof(RectTransform));
        root.transform.SetParent(transform, false);

        canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(root.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0.12f);
        prt.anchorMax = new Vector2(0.5f, 0.12f);
        prt.sizeDelta = new Vector2(460, 52);
        var img = panel.AddComponent<Image>();
        img.color = UITheme.HudPanel;
        panel.AddComponent<RectMask2D>();

        var accent = new GameObject("Accent", typeof(RectTransform));
        accent.transform.SetParent(panel.transform, false);
        var art = accent.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(0f, 0f);
        art.anchorMax = new Vector2(0f, 1f);
        art.pivot = new Vector2(0f, 0.5f);
        art.sizeDelta = new Vector2(4f, 0f);
        art.anchoredPosition = Vector2.zero;
        accent.AddComponent<Image>().color = UITheme.Gold;

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(panel.transform, false);
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(16, 6);
        trt.offsetMax = new Vector2(-16, -6);
        label = textGo.AddComponent<TextMeshProUGUI>();
        label.color = UITheme.GoldSoft;
        label.alignment = TextAlignmentOptions.Center;
        label.fontStyle = FontStyles.Bold;
        UITheme.FitText(label, 20f, wrap: false);
    }
}
