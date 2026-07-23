using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Exclusive MetaEdu main-menu / pause layout.
/// Rebuilds a clean shell; keeps OnClick on named buttons.
/// </summary>
public static class MainMenuVisuals
{
    public enum MenuMode
    {
        Title,
        Pause
    }

    struct BtnSpec
    {
        public string name;
        public string label;
        public string icon;
        public bool primary;
        public bool danger;
        public bool secondaryGroup;
    }

    public static void Apply(Canvas canvas)
    {
        if (canvas == null) return;
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        UITheme.ApplyStandardScaler(scaler);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Apply(canvas.transform, MenuMode.Title);
    }

    public static void Apply(Transform root, MenuMode mode)
    {
        if (root == null) return;

        float cardW = 400f;
        float cardH = mode == MenuMode.Pause ? 560f : 500f;

        HideStrays(root);
        BuildBackground(root);
        BuildBrand(root, cardH, mode);
        EnsureMenuPanel(root);
        var panelRt = BuildMenuCard(root, cardW, cardH, mode);
        if (panelRt != null)
            LayoutButtons(panelRt, root, cardW, mode);
        BuildWelcomeChip(root);
        BuildFooter(root, mode);

        // Do NOT ExclusiveUIStyler whole tree — it fights button primary gold.
        StylePrimaryDanger(root, mode);
        EnsureButtonFx(root);
        KillDecorativeRaycasts(root);
    }

    static void KillDecorativeRaycasts(Transform root)
    {
        string[] names =
        {
            "Background", "MenuTopWash", "MenuBottomWash", "MenuVignette",
            "MenuInner", "MenuAccent", "MenuDivider", "BrandRule",
            "Titletext", "Subtitle", "MenuCardHeader", "MenuFooterHint",
            "MenuWelcomeChip", "ChipText", "ChipAccent", "BtnAccent"
        };
        for (int i = 0; i < names.Length; i++)
        {
            var t = FindDeep(root, names[i]);
            if (t == null) continue;
            var img = t.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;
            var tmp = t.GetComponent<TMP_Text>();
            if (tmp != null) tmp.raycastTarget = false;
        }

        // Full-screen leftover overlays that are not buttons
        var vignette = FindDeep(root, "MenuVignette");
        if (vignette != null) vignette.gameObject.SetActive(false);
    }

    static void HideStrays(Transform root)
    {
        // Deactivate unknown top-level / deep junk that clutters exclusive menu
        // (scene leftovers, old labels, pause title, etc.)
        var pauseTitle = FindDeep(root, "PauseTitle");
        if (pauseTitle != null) pauseTitle.gameObject.SetActive(false);

        // Hide bare "Text (TMP)" not under a button
        HideOrphanLabels(root);
    }

    static void HideOrphanLabels(Transform root)
    {
        var tmps = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < tmps.Length; i++)
        {
            var t = tmps[i];
            if (t == null) continue;
            if (t.GetComponentInParent<Button>() != null) continue;
            string n = t.gameObject.name;
            if (n == "Titletext" || n == "Subtitle" || n == "MenuCardHeader"
                || n == "MenuFooterHint" || n == "ChipText"
                || n.IndexOf("Chip", System.StringComparison.OrdinalIgnoreCase) >= 0)
                continue;
            if (n.StartsWith("Text") || n == "Text (TMP)" || n == "Text")
                t.gameObject.SetActive(false);
        }
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
        img.color = new Color(0.03f, 0.025f, 0.045f, 1f);
        // Must not steal clicks from menu buttons
        img.raycastTarget = false;

        EnsureStrip(canvasRoot, "MenuTopWash",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, 0f), new Vector2(0f, 200f),
            new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.07f), 1, top: true);
        EnsureStrip(canvasRoot, "MenuBottomWash",
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 0f), new Vector2(0f, 160f),
            new Color(0f, 0f, 0f, 0.4f), 2, top: false);
    }

    static void EnsureStrip(Transform parent, string name, Vector2 aMin, Vector2 aMax,
        Vector2 _, Vector2 size, Color color, int sibling, bool top)
    {
        Transform t = parent.Find(name);
        GameObject go = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform));
        if (t == null) go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        if (top)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, size.y);
        }
        else
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, size.y);
        }

        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        go.transform.SetSiblingIndex(Mathf.Min(sibling, parent.childCount - 1));
    }

    static void BuildBrand(Transform canvasRoot, float cardH, MenuMode mode)
    {
        // Brand above card: card center at -20, height cardH → top of card ≈ cardH/2 - 20
        float brandY = cardH * 0.5f + 36f;

        StyleTitle(canvasRoot, "Titletext", "METAEDU", 42f,
            new Vector2(0f, brandY + 18f), soft: false);
        StyleTitle(canvasRoot, "Subtitle",
            mode == MenuMode.Pause ? "Permainan dijeda" : "WORLD  ·  Virtual Campus",
            14f, new Vector2(0f, brandY - 22f), soft: true);
        EnsureBrandRule(canvasRoot, brandY - 40f);
    }

    static void EnsureBrandRule(Transform canvasRoot, float y)
    {
        const string name = "BrandRule";
        Transform t = canvasRoot.Find(name) ?? FindDeep(canvasRoot, name);
        GameObject go = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform));
        if (t == null) go.transform.SetParent(canvasRoot, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(72f, 2f);
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.7f);
        img.raycastTarget = false;
    }

    static void EnsureMenuPanel(Transform root)
    {
        if (FindDeep(root, "Menupanel") != null || FindDeep(root, "MenuPanel") != null)
            return;
        var go = new GameObject("Menupanel", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(root, false);
    }

    static RectTransform BuildMenuCard(Transform canvasRoot, float w, float h, MenuMode mode)
    {
        var panel = FindDeep(canvasRoot, "Menupanel") ?? FindDeep(canvasRoot, "MenuPanel");
        if (panel == null) return null;

        var panelRt = panel as RectTransform;
        panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = new Vector2(0f, -28f);
        panelRt.sizeDelta = new Vector2(w, h);
        panelRt.localScale = Vector3.one;
        panel.gameObject.SetActive(true);

        var pimg = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
        pimg.color = UITheme.PanelDark;
        pimg.raycastTarget = true;

        // Clean mask/outline
        if (panel.GetComponent<RectMask2D>() == null)
            panel.gameObject.AddComponent<RectMask2D>();
        EnsureOutline(panel.gameObject, UITheme.Gold, 1.6f);
        EnsureAccent(panel, "MenuAccent", 3f);
        EnsureInner(panel, "MenuInner");
        EnsureCardHeader(panel, "MenuCardHeader",
            mode == MenuMode.Pause ? "MENU" : "MENU UTAMA");

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
        rt.offsetMin = new Vector2(10f, 10f);
        rt.offsetMax = new Vector2(-10f, -44f);
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
        tmp.enableAutoSizing = false;
        tmp.fontSize = 13f;
        tmp.characterSpacing = 6f;

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -14f);
        rt.sizeDelta = new Vector2(-28f, 22f);
    }

    static void LayoutButtons(RectTransform panel, Transform canvasRoot, float cardW, MenuMode mode)
    {
        // ASCII-only labels — LiberationSans SDF has no ▶★↺✕ glyphs
        List<BtnSpec> specs = mode == MenuMode.Pause
            ? new List<BtnSpec>
            {
                new BtnSpec { name = "StartButton", label = "LANJUTKAN", icon = "", primary = true },
                new BtnSpec { name = "LeaderBoardButton", label = "LEADERBOARD", icon = "" },
                new BtnSpec { name = "SettingButton", label = "PROFIL", icon = "" },
                new BtnSpec { name = "HelpButton", label = "BANTUAN", icon = "" },
                new BtnSpec { name = "MainMenuButton", label = "MENU UTAMA", icon = "" },
                new BtnSpec { name = "reset", label = "RESET PROGRES", icon = "", danger = true, secondaryGroup = true },
                new BtnSpec { name = "ExitButton", label = "KELUAR", icon = "", secondaryGroup = true },
            }
            : new List<BtnSpec>
            {
                new BtnSpec { name = "StartButton", label = "MULAI PETUALANGAN", icon = "", primary = true },
                new BtnSpec { name = "LeaderBoardButton", label = "LEADERBOARD", icon = "" },
                new BtnSpec { name = "SettingButton", label = "PROFIL", icon = "" },
                new BtnSpec { name = "HelpButton", label = "BANTUAN", icon = "" },
                new BtnSpec { name = "reset", label = "RESET PROGRES", icon = "", danger = true, secondaryGroup = true },
                new BtnSpec { name = "ExitButton", label = "KELUAR", icon = "", secondaryGroup = true },
            };

        float btnW = Mathf.Min(cardW - 56f, 320f);
        float hPrimary = 52f;
        float hNormal = 44f;
        float gap = 10f;
        float top = -48f;

        int mainCount = 0;
        for (int i = 0; i < specs.Count; i++)
            if (!specs[i].secondaryGroup) mainCount++;

        float divY = -48f - hPrimary - gap - (hNormal + gap) * (mainCount - 1) + 4f;
        EnsureDivider(panel, "MenuDivider", divY, btnW);

        // Hide non-spec buttons under panel
        HideUnknownButtons(panel, specs);

        for (int i = 0; i < specs.Count; i++)
        {
            var s = specs[i];
            Transform t = FindDeep(panel, s.name) ?? FindDeep(canvasRoot, s.name);
            if (t == null)
                t = EnsureButton(panel, s.name);

            if (t.parent != panel)
                t.SetParent(panel, false);

            t.gameObject.SetActive(true);

            float h = s.primary ? hPrimary : hNormal;
            if (s.secondaryGroup && i > 0 && !specs[i - 1].secondaryGroup)
                top -= 12f;

            var rt = t as RectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, top);
            rt.sizeDelta = new Vector2(btnW, h);
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            ApplyButtonVisual(t.gameObject, s);
            EnsureClickable(t.gameObject);

            // Button always above decorative siblings
            t.SetAsLastSibling();
            top -= (h + gap);
        }

        // Divider / inner behind buttons
        var div = panel.Find("MenuDivider");
        if (div != null) div.SetAsFirstSibling();
        var inn = panel.Find("MenuInner");
        if (inn != null) inn.SetAsFirstSibling();
        var acc = panel.Find("MenuAccent");
        if (acc != null) acc.SetAsFirstSibling();

        // Auto-fit card height to content
        float contentH = 48f + (-top) + 20f;
        panel.sizeDelta = new Vector2(cardW, Mathf.Max(panel.sizeDelta.y, contentH));

        if (mode == MenuMode.Pause)
        {
            var resumeLegacy = FindDeep(canvasRoot, "ResumeButton");
            if (resumeLegacy != null)
                resumeLegacy.gameObject.SetActive(false);
        }
    }

    static void HideUnknownButtons(Transform panel, List<BtnSpec> specs)
    {
        var btns = panel.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < btns.Length; i++)
        {
            string n = btns[i].gameObject.name;
            bool known = false;
            for (int s = 0; s < specs.Count; s++)
            {
                if (specs[s].name == n) { known = true; break; }
            }
            if (!known && n != "StartButton")
            {
                // keep nested only if parent is known button
                if (btns[i].transform.parent == panel)
                    btns[i].gameObject.SetActive(false);
            }
        }
    }

    static Transform EnsureButton(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = Color.white;
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
        Stretch(textGo.GetComponent<RectTransform>());
        return go.transform;
    }

    static void EnsureDivider(Transform panel, string name, float yFromTop, float width)
    {
        Transform t = panel.Find(name);
        GameObject go = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform));
        if (t == null) go.transform.SetParent(panel, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, yFromTop);
        rt.sizeDelta = new Vector2(width - 24f, 1f);
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.25f);
        img.raycastTarget = false;
    }

    static void EnsureClickable(GameObject go)
    {
        if (go == null) return;
        var btn = go.GetComponent<Button>();
        if (btn != null) btn.interactable = true;

        var img = go.GetComponent<Image>();
        if (img != null) img.raycastTarget = true;

        var cg = go.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }
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
            img.raycastTarget = true;
            btn.targetGraphic = img;
        }

        EnsureOutline(go, s.danger ? UITheme.Danger : UITheme.Gold, s.primary ? 1.5f : 1.1f);
        EnsureBtnAccent(go.transform, s.primary, s.danger);

        // Prefer Label child; else first TMP
        TMP_Text tmp = null;
        var label = go.transform.Find("Label");
        if (label != null) tmp = label.GetComponent<TMP_Text>();
        if (tmp == null) tmp = go.GetComponentInChildren<TMP_Text>(true);

        // Hide extra text children (scene often has multiple Text TMP)
        var allTmp = go.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < allTmp.Length; i++)
        {
            if (allTmp[i] == tmp) continue;
            allTmp[i].gameObject.SetActive(false);
        }

        if (tmp != null)
        {
            tmp.gameObject.SetActive(true);
            string icon = string.IsNullOrEmpty(s.icon) ? "" : s.icon + "  ";
            tmp.text = icon + s.label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = s.primary ? UITheme.TextOnGold
                : (s.danger ? new Color(1f, 0.78f, 0.74f) : UITheme.Cream);
            tmp.enableAutoSizing = false;
            tmp.fontSize = s.primary ? 17f : 15f;
            tmp.raycastTarget = false;
            Stretch(tmp.rectTransform);
            var trt = tmp.rectTransform;
            trt.offsetMin = new Vector2(12f, 2f);
            trt.offsetMax = new Vector2(-12f, -2f);
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
        rt.anchorMin = new Vector2(0f, 0.15f);
        rt.anchorMax = new Vector2(0f, 0.85f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(3f, 0f);
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = danger ? UITheme.Danger : UITheme.Gold;
        img.raycastTarget = false;
    }

    static void StylePrimaryDanger(Transform root, MenuMode mode)
    {
        string startLabel = mode == MenuMode.Pause ? "LANJUTKAN" : "MULAI PETUALANGAN";
        ReStyle(root, "StartButton", startLabel, true, false);
        ReStyle(root, "LeaderBoardButton", "LEADERBOARD", false, false);
        ReStyle(root, "SettingButton", "PROFIL", false, false);
        ReStyle(root, "HelpButton", "BANTUAN", false, false);
        if (mode == MenuMode.Pause)
            ReStyle(root, "MainMenuButton", "MENU UTAMA", false, false);
        ReStyle(root, "reset", "RESET PROGRES", false, true);
        ReStyle(root, "ExitButton", "KELUAR", false, false);
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
            var img = btn.targetGraphic as Image ?? t.GetComponent<Image>();
            if (img != null) img.color = Color.white;
        }
        var tmp = t.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.gameObject.SetActive(true);
            tmp.text = label;
            tmp.color = primary ? UITheme.TextOnGold
                : (danger ? new Color(1f, 0.78f, 0.74f) : UITheme.Cream);
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;
            tmp.fontSize = primary ? 17f : 15f;
        }
        EnsureOutline(t.gameObject, danger ? UITheme.Danger : UITheme.Gold, primary ? 1.5f : 1.1f);
    }

    static void EnsureButtonFx(Transform root)
    {
        foreach (var btn in root.GetComponentsInChildren<Button>(true))
        {
            if (!btn.gameObject.activeInHierarchy) continue;
            if (btn.GetComponent<UIButtonPressFx>() == null)
                btn.gameObject.AddComponent<UIButtonPressFx>();
        }
    }

    static void BuildWelcomeChip(Transform canvasRoot)
    {
        const string name = "MenuWelcomeChip";
        Transform t = canvasRoot.Find(name) ?? FindDeep(canvasRoot, name);
        GameObject go = t != null ? t.gameObject : new GameObject(name, typeof(RectTransform));
        if (t == null) go.transform.SetParent(canvasRoot, false);
        go.SetActive(true);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(28f, -28f);
        rt.sizeDelta = new Vector2(240f, 44f);

        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = UITheme.HudPanel;
        img.raycastTarget = false;
        EnsureOutline(go, UITheme.Gold, 1f);

        Transform accT = go.transform.Find("ChipAccent");
        GameObject acc = accT != null ? accT.gameObject : new GameObject("ChipAccent", typeof(RectTransform));
        if (accT == null) acc.transform.SetParent(go.transform, false);
        var art = acc.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(0f, 0.2f);
        art.anchorMax = new Vector2(0f, 0.8f);
        art.pivot = new Vector2(0f, 0.5f);
        art.sizeDelta = new Vector2(3f, 0f);
        art.anchoredPosition = Vector2.zero;
        var aimg = acc.GetComponent<Image>() ?? acc.AddComponent<Image>();
        aimg.color = UITheme.Gold;
        aimg.raycastTarget = false;

        string player = PlayerPrefs.GetString("playerName", "Mahasiswa");
        if (string.IsNullOrWhiteSpace(player)) player = "Mahasiswa";
        if (player.Length > 16) player = player.Substring(0, 15) + "…";

        Transform txtT = go.transform.Find("ChipText");
        TMP_Text tmp;
        if (txtT == null)
        {
            var tg = new GameObject("ChipText", typeof(RectTransform));
            tg.transform.SetParent(go.transform, false);
            tmp = tg.AddComponent<TextMeshProUGUI>();
            txtT = tg.transform;
        }
        else tmp = txtT.GetComponent<TMP_Text>() ?? txtT.gameObject.AddComponent<TextMeshProUGUI>();

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
        tmp.enableAutoSizing = false;
        tmp.fontSize = 14f;
    }

    static void BuildFooter(Transform canvasRoot, MenuMode mode)
    {
        const string name = "MenuFooterHint";
        Transform existing = canvasRoot.Find(name) ?? FindDeep(canvasRoot, name);
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
            tmp = existing.GetComponent<TMP_Text>() ?? existing.gameObject.AddComponent<TextMeshProUGUI>();
            rt = existing as RectTransform;
            existing.gameObject.SetActive(true);
        }

        tmp.text = mode == MenuMode.Pause
            ? "ESC  ·  Lanjutkan permainan"
            : "Misi  ·  Kuis  ·  Eksplorasi Kampus";
        tmp.color = UITheme.Muted;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.enableAutoSizing = false;
        tmp.fontSize = 12f;

        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 28f);
        rt.sizeDelta = new Vector2(640f, 22f);
    }

    static void StyleTitle(Transform root, string name, string text, float size, Vector2 pos, bool soft)
    {
        var t = FindDeep(root, name);
        TMP_Text tmp;
        RectTransform rt;

        if (t != null)
        {
            tmp = t.GetComponent<TMP_Text>();
            if (tmp == null) tmp = t.gameObject.AddComponent<TextMeshProUGUI>();
            rt = t as RectTransform;
            if (t.parent != root) t.SetParent(root, false);
            t.gameObject.SetActive(true);
        }
        else
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(root, false);
            tmp = go.AddComponent<TextMeshProUGUI>();
            rt = go.GetComponent<RectTransform>();
        }

        tmp.text = text;
        tmp.color = soft ? UITheme.Muted : UITheme.Gold;
        tmp.fontStyle = soft ? FontStyles.Normal : FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.enableAutoSizing = false;
        tmp.fontSize = size;
        if (!soft) tmp.characterSpacing = 4f;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(soft ? 640f : 720f, soft ? 26f : 52f);
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
        outline.effectColor = new Color(color.r, color.g, color.b, 0.75f);
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
