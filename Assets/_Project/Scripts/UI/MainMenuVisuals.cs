using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime premium layout for MainMenu — dark-gold MetaEdu identity.
/// Layout only; OnClick wiring stays on scene buttons.
/// </summary>
public static class MainMenuVisuals
{
    struct BtnSpec
    {
        public string name;
        public string label;
        public string icon; // single-glyph prefix
        public bool primary;
        public bool danger;
        public bool secondaryGroup; // after divider
    }

    public static void Apply(Canvas canvas)
    {
        if (canvas == null) return;

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        UITheme.ApplyStandardScaler(scaler);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Responsive card size (reference 1920x1080)
        float cardW = Mathf.Clamp(420f, 360f, 480f);
        float cardH = 560f;

        BuildBackground(canvas.transform);
        BuildBrand(canvas.transform, cardH);
        var panelRt = BuildMenuCard(canvas.transform, cardW, cardH);
        if (panelRt != null)
            LayoutButtons(panelRt, canvas.transform, cardW);
        BuildWelcomeChip(canvas.transform, cardH);
        BuildFooter(canvas.transform);

        ExclusiveUIStyler.Apply(canvas.transform);
        StylePrimaryDanger(canvas.transform);
        EnsureButtonFx(canvas.transform);
    }

    static void BuildBackground(Transform canvasRoot)
    {
        var bg = FindDeep(canvasRoot, "Background");
        if (bg == null)
        {
            var go = new GameObject("Background", typeof(RectTransform));
            go.transform.SetParent(canvasRoot, false);
            bg = go.transform;
        }
        Stretch(bg as RectTransform);
        bg.SetAsFirstSibling();
        var img = bg.GetComponent<Image>() ?? bg.gameObject.AddComponent<Image>();
        img.color = UITheme.BgDeep;
        img.raycastTarget = false;

        // Gradient-ish strips (top gold wash + bottom depth) — flat images, cheap
        EnsureStrip(canvasRoot, "MenuTopWash", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, 0f), new Vector2(0f, 220f), new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.06f), 1);
        EnsureStrip(canvasRoot, "MenuBottomWash", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 0f), new Vector2(0f, 180f), new Color(0f, 0f, 0f, 0.35f), 2);

        EnsureStrip(canvasRoot, "MenuVignette", Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, new Color(0.02f, 0.015f, 0.03f, 0.28f), 3);
    }

    static void EnsureStrip(Transform parent, string name, Vector2 aMin, Vector2 aMax,
        Vector2 offsetMin, Vector2 sizeOrOffsetMax, Color color, int sibling)
    {
        Transform t = parent.Find(name);
        GameObject go;
        if (t == null)
        {
            go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
        }
        else go = t.gameObject;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        if (aMin == Vector2.zero && aMax == Vector2.one)
        {
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        else if (aMin.y >= 0.99f)
        {
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = sizeOrOffsetMax;
            // stretch width
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(0f, sizeOrOffsetMax.y);
        }
        else if (aMin.y <= 0.01f && aMax.y <= 0.01f)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, sizeOrOffsetMax.y);
        }

        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        go.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
    }

    static void BuildBrand(Transform canvasRoot, float cardH)
    {
        // Brand sits above card center
        float brandY = cardH * 0.5f + 78f;

        StyleTitle(canvasRoot, "Titletext", "METAEDU WORLD", 44f,
            new Vector2(0f, brandY), soft: false);
        StyleTitle(canvasRoot, "Subtitle", "Virtual Learning · Teknik Informatika", 15f,
            new Vector2(0f, brandY - 42f), soft: true);

        // Thin gold rule under brand
        EnsureBrandRule(canvasRoot, brandY - 58f);
    }

    static void EnsureBrandRule(Transform canvasRoot, float y)
    {
        const string name = "BrandRule";
        Transform t = canvasRoot.Find(name);
        GameObject go = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform));
        if (t == null) go.transform.SetParent(canvasRoot, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(120f, 2f);
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.55f);
        img.raycastTarget = false;
    }

    static RectTransform BuildMenuCard(Transform canvasRoot, float w, float h)
    {
        var panel = FindDeep(canvasRoot, "Menupanel") ?? FindDeep(canvasRoot, "MenuPanel");
        if (panel == null) return null;

        var panelRt = panel as RectTransform;
        panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = new Vector2(0f, -36f);
        panelRt.sizeDelta = new Vector2(w, h);

        var pimg = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
        pimg.color = UITheme.PanelDark;
        pimg.raycastTarget = true;

        if (panel.GetComponent<RectMask2D>() == null)
            panel.gameObject.AddComponent<RectMask2D>();

        EnsureOutline(panel.gameObject, UITheme.Gold, 1.8f);
        EnsureAccent(panel, "MenuAccent", 4f);

        // Inner surface for depth
        EnsureInner(panel, "MenuInner");

        // Section header inside card
        EnsureCardHeader(panel, "MenuCardHeader", "MENU UTAMA");

        return panelRt;
    }

    static void EnsureInner(Transform panel, string name)
    {
        Transform t = panel.Find(name);
        GameObject go = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform));
        if (t == null) go.transform.SetParent(panel, false);
        go.transform.SetSiblingIndex(0);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(12f, 12f);
        rt.offsetMax = new Vector2(-12f, -48f);
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = UITheme.CardInner;
        img.raycastTarget = false;
    }

    static void EnsureCardHeader(Transform panel, string name, string text)
    {
        Transform t = panel.Find(name);
        TMP_Text tmp;
        RectTransform rt;
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(panel, false);
            tmp = go.AddComponent<TextMeshProUGUI>();
            rt = go.GetComponent<RectTransform>();
        }
        else
        {
            tmp = t.GetComponent<TMP_Text>() ?? t.gameObject.AddComponent<TextMeshProUGUI>();
            rt = t as RectTransform;
        }

        tmp.text = text;
        tmp.color = UITheme.GoldSoft;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        UITheme.FitText(tmp, 13f, false);

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -18f);
        rt.sizeDelta = new Vector2(-32f, 22f);
    }

    static void LayoutButtons(RectTransform panel, Transform canvasRoot, float cardW)
    {
        var specs = new List<BtnSpec>
        {
            new BtnSpec { name = "StartButton", label = "MULAI PETUALANGAN", icon = "▶", primary = true },
            new BtnSpec { name = "LeaderBoardButton", label = "LEADERBOARD", icon = "★" },
            new BtnSpec { name = "SettingButton", label = "PROFIL", icon = "●" },
            new BtnSpec { name = "HelpButton", label = "BANTUAN", icon = "?" },
            new BtnSpec { name = "reset", label = "RESET PROGRES", icon = "↺", danger = true, secondaryGroup = true },
            new BtnSpec { name = "ExitButton", label = "KELUAR", icon = "✕", secondaryGroup = true },
        };

        float btnW = Mathf.Min(cardW - 64f, 340f);
        float hPrimary = 56f;
        float hNormal = 48f;
        float gap = 12f;
        float top = -56f;

        // Divider between main actions and utility
        EnsureDivider(panel, "MenuDivider", -56f - hPrimary - gap - (hNormal + gap) * 3f + 6f);

        for (int i = 0; i < specs.Count; i++)
        {
            var s = specs[i];
            Transform t = FindDeep(panel, s.name) ?? FindDeep(canvasRoot, s.name);
            if (t == null) continue;

            if (t.parent != panel)
                t.SetParent(panel, false);

            float h = s.primary ? hPrimary : hNormal;
            if (s.secondaryGroup && i > 0 && !specs[i - 1].secondaryGroup)
                top -= 10f; // extra space after divider

            var rt = t as RectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, top);
            rt.sizeDelta = new Vector2(btnW, h);
            rt.localScale = Vector3.one;

            ApplyButtonVisual(t.gameObject, s);
            top -= (h + gap);
        }
    }

    static void EnsureDivider(Transform panel, string name, float yFromTop)
    {
        Transform t = panel.Find(name);
        GameObject go = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform));
        if (t == null) go.transform.SetParent(panel, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, yFromTop);
        rt.sizeDelta = new Vector2(280f, 1f);
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.22f);
        img.raycastTarget = false;
    }

    static void ApplyButtonVisual(GameObject go, BtnSpec s)
    {
        var btn = go.GetComponent<Button>();
        if (btn == null) return;

        btn.transition = Selectable.Transition.ColorTint;
        btn.colors = s.primary ? PrimaryColors() : (s.danger ? DangerColors() : SecondaryColors());
        btn.interactable = true;

        var img = btn.targetGraphic as Image ?? go.GetComponent<Image>();
        if (img != null)
        {
            img.color = Color.white;
            btn.targetGraphic = img;
        }

        EnsureOutline(go, s.danger ? UITheme.Danger : UITheme.Gold, s.primary ? 1.7f : 1.2f);

        // Left accent bar for non-primary
        EnsureBtnAccent(go.transform, s.primary, s.danger);

        var tmp = go.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            string icon = string.IsNullOrEmpty(s.icon) ? "" : s.icon + "  ";
            tmp.text = icon + s.label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = s.primary ? UITheme.TextOnGold
                : (s.danger ? new Color(1f, 0.75f, 0.72f) : UITheme.Cream);
            UITheme.FitText(tmp, s.primary ? 18f : 16f, false);
            Stretch(tmp.rectTransform);
            var trt = tmp.rectTransform;
            trt.offsetMin = new Vector2(10f, 2f);
            trt.offsetMax = new Vector2(-10f, -2f);
        }
    }

    static void EnsureBtnAccent(Transform btn, bool primary, bool danger)
    {
        const string name = "BtnAccent";
        Transform t = btn.Find(name);
        if (primary)
        {
            if (t != null) t.gameObject.SetActive(false);
            return;
        }

        GameObject go = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform));
        if (t == null) go.transform.SetParent(btn, false);
        go.SetActive(true);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(3f, 0f);
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = danger ? UITheme.Danger : UITheme.Gold;
        img.raycastTarget = false;
    }

    static void StylePrimaryDanger(Transform root)
    {
        // Re-apply after ExclusiveUIStyler so primary gold survives
        ReStyle(root, "StartButton", "▶  MULAI PETUALANGAN", true, false);
        ReStyle(root, "LeaderBoardButton", "★  LEADERBOARD", false, false);
        ReStyle(root, "SettingButton", "●  PROFIL", false, false);
        ReStyle(root, "HelpButton", "?  BANTUAN", false, false);
        ReStyle(root, "reset", "↺  RESET PROGRES", false, true);
        ReStyle(root, "ExitButton", "✕  KELUAR", false, false);
    }

    static void ReStyle(Transform root, string name, string label, bool primary, bool danger)
    {
        var t = FindDeep(root, name);
        if (t == null) return;
        var btn = t.GetComponent<Button>();
        if (btn != null)
        {
            btn.transition = Selectable.Transition.ColorTint;
            btn.colors = primary ? PrimaryColors() : (danger ? DangerColors() : SecondaryColors());
            btn.interactable = true;
        }
        var tmp = t.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = label;
            tmp.color = primary ? UITheme.TextOnGold
                : (danger ? new Color(1f, 0.75f, 0.72f) : UITheme.Cream);
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            UITheme.FitText(tmp, primary ? 18f : 16f, false);
        }
        EnsureOutline(t.gameObject, danger ? UITheme.Danger : UITheme.Gold, primary ? 1.7f : 1.2f);
    }

    static void EnsureButtonFx(Transform root)
    {
        foreach (var btn in root.GetComponentsInChildren<Button>(true))
        {
            if (btn.GetComponent<UIButtonPressFx>() == null)
                btn.gameObject.AddComponent<UIButtonPressFx>();
        }
    }

    static void BuildWelcomeChip(Transform canvasRoot, float cardH)
    {
        const string name = "MenuWelcomeChip";
        Transform t = canvasRoot.Find(name);
        GameObject go = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform));
        if (t == null) go.transform.SetParent(canvasRoot, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, cardH * 0.5f + 12f);
        // sits between brand rule and card — actually place top-left as profile chip
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(28f, -28f);
        rt.sizeDelta = new Vector2(260f, 52f);

        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = UITheme.HudPanel;
        img.raycastTarget = false;
        if (go.GetComponent<RectMask2D>() == null) go.AddComponent<RectMask2D>();
        EnsureOutline(go, UITheme.Gold, 1.1f);

        // accent
        Transform accT = go.transform.Find("ChipAccent");
        GameObject acc = accT != null ? accT.gameObject : new GameObject("ChipAccent", typeof(RectTransform));
        if (accT == null) acc.transform.SetParent(go.transform, false);
        var art = acc.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(0f, 0f);
        art.anchorMax = new Vector2(0f, 1f);
        art.pivot = new Vector2(0f, 0.5f);
        art.sizeDelta = new Vector2(3f, 0f);
        var aimg = acc.GetComponent<Image>() ?? acc.AddComponent<Image>();
        aimg.color = UITheme.Gold;
        aimg.raycastTarget = false;

        string player = PlayerPrefs.GetString("playerName", "Mahasiswa");
        if (string.IsNullOrWhiteSpace(player)) player = "Mahasiswa";
        if (player.Length > 18) player = player.Substring(0, 17) + "…";

        Transform txtT = go.transform.Find("ChipText");
        TMP_Text tmp;
        if (txtT == null)
        {
            var tg = new GameObject("ChipText", typeof(RectTransform));
            tg.transform.SetParent(go.transform, false);
            tmp = tg.AddComponent<TextMeshProUGUI>();
            txtT = tg.transform;
        }
        else tmp = txtT.GetComponent<TMP_Text>();

        var trt = txtT as RectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(14f, 6f);
        trt.offsetMax = new Vector2(-10f, -6f);
        tmp.text = "Halo, " + player;
        tmp.color = UITheme.GoldSoft;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
        UITheme.FitText(tmp, 15f, false);
    }

    static void BuildFooter(Transform canvasRoot)
    {
        const string name = "MenuFooterHint";
        Transform existing = canvasRoot.Find(name);
        TMP_Text tmp;
        RectTransform rt;
        if (existing == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(canvasRoot, false);
            tmp = go.AddComponent<TextMeshProUGUI>();
            rt = go.GetComponent<RectTransform>();
        }
        else
        {
            tmp = existing.GetComponent<TMP_Text>();
            rt = existing as RectTransform;
        }

        tmp.text = "Misi  ·  Kuis  ·  Leaderboard  ·  Eksplorasi Kampus";
        tmp.color = UITheme.Muted;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        UITheme.FitText(tmp, 13f, false);

        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 32f);
        rt.sizeDelta = new Vector2(720f, 24f);
    }

    static void StyleTitle(Transform root, string name, string text, float size, Vector2 pos, bool soft)
    {
        var t = FindDeep(root, name);
        TMP_Text tmp;
        RectTransform rt;

        if (t != null)
        {
            tmp = t.GetComponent<TMP_Text>();
            rt = t as RectTransform;
            if (t.parent != root) t.SetParent(root, false);
        }
        else
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(root, false);
            tmp = go.AddComponent<TextMeshProUGUI>();
            rt = go.GetComponent<RectTransform>();
        }

        if (tmp == null)
        {
            if (t != null) tmp = t.gameObject.AddComponent<TextMeshProUGUI>();
            else return;
        }

        tmp.text = text;
        tmp.color = soft ? UITheme.Muted : UITheme.Gold;
        tmp.fontStyle = soft ? FontStyles.Normal : FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        UITheme.FitText(tmp, size, false);

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(soft ? 780f : 900f, soft ? 28f : 58f);
        rt.localScale = Vector3.one;
    }

    static ColorBlock PrimaryColors()
    {
        return new ColorBlock
        {
            normalColor = UITheme.Gold,
            highlightedColor = UITheme.GoldSoft,
            pressedColor = new Color(0.55f, 0.44f, 0.14f, 1f),
            selectedColor = UITheme.GoldSoft,
            disabledColor = UITheme.ButtonDisabled,
            colorMultiplier = 1f,
            fadeDuration = 0.12f
        };
    }

    static ColorBlock SecondaryColors()
    {
        return new ColorBlock
        {
            normalColor = UITheme.ButtonNormal,
            highlightedColor = new Color(0.28f, 0.24f, 0.14f, 1f),
            pressedColor = UITheme.ButtonPressed,
            selectedColor = new Color(0.32f, 0.28f, 0.16f, 1f),
            disabledColor = UITheme.ButtonDisabled,
            colorMultiplier = 1f,
            fadeDuration = 0.12f
        };
    }

    static ColorBlock DangerColors()
    {
        return new ColorBlock
        {
            normalColor = new Color(0.20f, 0.10f, 0.12f, 1f),
            highlightedColor = new Color(0.48f, 0.18f, 0.18f, 1f),
            pressedColor = new Color(0.32f, 0.10f, 0.10f, 1f),
            selectedColor = new Color(0.48f, 0.18f, 0.18f, 1f),
            disabledColor = UITheme.ButtonDisabled,
            colorMultiplier = 1f,
            fadeDuration = 0.12f
        };
    }

    static void EnsureAccent(Transform panel, string name, float height)
    {
        Transform existing = panel.Find(name);
        GameObject bar = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        if (existing == null) bar.transform.SetParent(panel, false);

        var rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, height);
        var img = bar.GetComponent<Image>() ?? bar.AddComponent<Image>();
        img.color = UITheme.Gold;
        img.raycastTarget = false;
    }

    static void EnsureOutline(GameObject go, Color color, float distance)
    {
        if (go == null || distance <= 0f) return;
        var outline = go.GetComponent<Outline>() ?? go.AddComponent<Outline>();
        outline.effectColor = new Color(color.r, color.g, color.b, 0.8f);
        outline.effectDistance = new Vector2(distance, -distance);
        outline.useGraphicAlpha = true;
    }

    static void Stretch(RectTransform rt)
    {
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var f = FindDeep(root.GetChild(i), name);
            if (f != null) return f;
        }
        return null;
    }
}
