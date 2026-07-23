using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Exclusive Help overlay. Built at runtime under Canvas.</summary>
public class HelpPanel : MonoBehaviour
{
    [SerializeField] GameObject panelRoot;
    [SerializeField] Button closeButton;

    bool built;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    void Awake()
    {
        if (panelRoot == null)
            BuildRuntimeUI();
        Hide();
    }

    public void Show()
    {
        EnsurePanel();
        if (panelRoot == null) return;
        panelRoot.SetActive(true);
        panelRoot.transform.SetAsLastSibling();
        ExclusiveUIStyler.Apply(panelRoot.transform);
        StyleClose();
        ExclusiveMenuUI.ForceAllButtons(panelRoot.transform);
        var cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg == null) cg = panelRoot.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
        cg.ignoreParentGroups = true;
        UIMotion.FadeCanvas(cg, 1f, 0.18f);
        var card = panelRoot.transform.Find("HelpCard") as RectTransform;
        if (card != null) UIMotion.PopIn(card, 0.2f);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void EnsurePanel()
    {
        if (panelRoot == null)
        {
            built = false;
            closeButton = null;
            BuildRuntimeUI();
            return;
        }
        var host = ResolveHostCanvas();
        if (host != null && panelRoot.transform.parent != host.transform)
        {
            panelRoot.transform.SetParent(host.transform, false);
            Stretch(panelRoot.GetComponent<RectTransform>());
        }
    }

    void StyleClose()
    {
        if (closeButton == null) return;
        closeButton.transition = Selectable.Transition.ColorTint;
        closeButton.colors = new ColorBlock
        {
            normalColor = UITheme.Gold,
            highlightedColor = UITheme.GoldSoft,
            pressedColor = UITheme.ButtonPressed,
            selectedColor = UITheme.GoldSoft,
            disabledColor = UITheme.ButtonDisabled,
            colorMultiplier = 1f,
            fadeDuration = 0.12f
        };
        var t = closeButton.GetComponentInChildren<TMP_Text>(true);
        if (t != null)
        {
            t.text = "MENGERTI";
            t.color = UITheme.TextOnGold;
            t.fontStyle = FontStyles.Bold;
        }
        if (closeButton.GetComponent<UIButtonPressFx>() == null)
            closeButton.gameObject.AddComponent<UIButtonPressFx>();
    }

    public void Toggle()
    {
        if (IsOpen) Hide();
        else Show();
    }

    void BuildRuntimeUI()
    {
        if (built && panelRoot != null) return;
        built = true;

        Canvas canvas = ResolveHostCanvas();
        if (canvas == null) { built = false; return; }

        panelRoot = new GameObject("HelpPanel", typeof(RectTransform));
        panelRoot.transform.SetParent(canvas.transform, false);
        Stretch(panelRoot.GetComponent<RectTransform>());
        var dim = panelRoot.AddComponent<Image>();
        dim.color = UITheme.DimOverlay;
        dim.raycastTarget = true;

        var card = new GameObject("HelpCard", typeof(RectTransform));
        card.transform.SetParent(panelRoot.transform, false);
        var cardRt = card.GetComponent<RectTransform>();
        cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(560, 520);
        card.AddComponent<Image>().color = UITheme.PanelDark;
        card.AddComponent<RectMask2D>();

        var accent = new GameObject("HelpAccent", typeof(RectTransform));
        accent.transform.SetParent(card.transform, false);
        var art = accent.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(0f, 1f);
        art.anchorMax = new Vector2(1f, 1f);
        art.pivot = new Vector2(0.5f, 1f);
        art.sizeDelta = new Vector2(0f, 4f);
        accent.AddComponent<Image>().color = UITheme.Gold;

        var titleGo = new GameObject("HelpTitle", typeof(RectTransform));
        titleGo.transform.SetParent(card.transform, false);
        var title = titleGo.AddComponent<TextMeshProUGUI>();
        title.text = "BANTUAN";
        title.color = UITheme.Gold;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        UITheme.FitText(title, 24f, false);
        var titleRt = title.rectTransform;
        titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -22);
        titleRt.sizeDelta = new Vector2(500, 34);

        var bodyGo = new GameObject("HelpBody", typeof(RectTransform));
        bodyGo.transform.SetParent(card.transform, false);
        var body = bodyGo.AddComponent<TextMeshProUGUI>();
        body.text =
            "<color=#F5E08A><b>KONTROL</b></color>\n" +
            "WASD gerak · Mouse lihat · Space lompat\n" +
            "E masuk pintu · Esc pause · J panel misi\n\n" +
            "<color=#F5E08A><b>MISI</b></color>\n" +
            "Panel kanan atas: tujuan, langkah, progres, hadiah.\n" +
            "Kompas atas: arah + jarak ke target.\n" +
            "Ikuti beacon emas. Selesaikan objektif berurutan.\n\n" +
            "<color=#F5E08A><b>BUKU & KUIS</b></color>\n" +
            "Dekati buku bersinar → jawab kuis.\n" +
            "Benar = poin · ≥70% = bonus XP.\n\n" +
            "<color=#F5E08A><b>PINTU & PROFIL</b></color>\n" +
            "Pintu Classroom (beacon) → tekan E.\n" +
            "Profil di Main Menu: nama & avatar.";
        body.richText = true;
        body.color = UITheme.Cream;
        body.alignment = TextAlignmentOptions.TopLeft;
        UITheme.FitText(body, 14f, true);
        body.raycastTarget = false;
        var bodyRt = body.rectTransform;
        bodyRt.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRt.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRt.anchoredPosition = new Vector2(0, 12);
        bodyRt.sizeDelta = new Vector2(500, 360);

        var closeGo = new GameObject("CloseHelp", typeof(RectTransform));
        closeGo.transform.SetParent(card.transform, false);
        var closeRt = closeGo.GetComponent<RectTransform>();
        closeRt.anchorMin = closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.pivot = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(0, 20);
        closeRt.sizeDelta = new Vector2(180, 46);
        var closeImg = closeGo.AddComponent<Image>();
        closeImg.color = Color.white;
        closeButton = closeGo.AddComponent<Button>();
        closeButton.targetGraphic = closeImg;
        closeButton.onClick.AddListener(Hide);

        var closeTxtGo = new GameObject("Text", typeof(RectTransform));
        closeTxtGo.transform.SetParent(closeGo.transform, false);
        Stretch(closeTxtGo.GetComponent<RectTransform>());
        var closeTxt = closeTxtGo.AddComponent<TextMeshProUGUI>();
        closeTxt.text = "MENGERTI";
        closeTxt.color = UITheme.TextOnGold;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.alignment = TextAlignmentOptions.Center;
        UITheme.FitText(closeTxt, 16f, false);

        ExclusiveUIStyler.Apply(panelRoot.transform);
        StyleClose();
    }

    static Canvas ResolveHostCanvas()
    {
        var pause = GameObject.Find("ExclusivePauseRoot");
        if (pause != null && pause.activeInHierarchy)
        {
            var pc = pause.GetComponent<Canvas>();
            if (pc != null) return pc;
        }
        var all = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
        Canvas best = null;
        int bestSort = int.MinValue;
        for (int i = 0; i < all.Length; i++)
        {
            var c = all[i];
            if (c == null || !c.isActiveAndEnabled) continue;
            if (c.renderMode != RenderMode.ScreenSpaceOverlay) continue;
            if (c.sortingOrder >= bestSort)
            {
                bestSort = c.sortingOrder;
                best = c;
            }
        }
        return best != null ? best : Object.FindAnyObjectByType<Canvas>();
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
