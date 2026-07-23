using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Single exclusive MetaEdu menu (Title + Pause). Runtime-built, always clickable.
/// Scene/prefab chrome stripped; Profile/Help panels preserved on clearHost.
/// </summary>
public static class ExclusiveMenuUI
{
    public enum Mode { Title, Pause }

    public sealed class Actions
    {
        public UnityAction Primary;      // Start / Resume
        public UnityAction Leaderboard;
        public UnityAction Profile;
        public UnityAction Help;
        public UnityAction MainMenu;     // pause only
        public UnityAction Reset;
        public UnityAction Exit;
    }

    public sealed class Result
    {
        public GameObject Root;
        public Canvas Canvas;
        public CanvasGroup Group;
    }

    /// <summary>
    /// Build full-screen exclusive menu under host (or new canvas if host null).
    /// Clears host children first when clearHost=true.
    /// </summary>
    public static Result Build(Transform host, Mode mode, Actions actions, bool clearHost = true)
    {
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        if (clearHost)
            ClearChildren(host);

        var canvas = host.GetComponent<Canvas>();
        if (canvas == null)
            canvas = host.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        if (mode == Mode.Pause)
        {
            canvas.sortingOrder = 900;
            canvas.overrideSorting = true;
        }
        else
        {
            canvas.sortingOrder = 100;
            canvas.overrideSorting = false;
        }

        var scaler = host.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = host.gameObject.AddComponent<CanvasScaler>();
        UITheme.ApplyStandardScaler(scaler);

        if (host.GetComponent<GraphicRaycaster>() == null)
            host.gameObject.AddComponent<GraphicRaycaster>();

        var group = host.GetComponent<CanvasGroup>();
        if (group == null) group = host.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = true;
        group.ignoreParentGroups = true;

        // Dim
        var dim = MakeImage(host, "Dim", UITheme.DimOverlay, stretch: true, raycast: true);
        dim.transform.SetAsFirstSibling();

        // Brand
        float brandY = 210f;
        MakeText(host, "BrandTitle", "METAEDU", 42f, UITheme.Gold, FontStyles.Bold,
            new Vector2(0.5f, 0.5f), new Vector2(0f, brandY), new Vector2(720f, 52f), center: true, spacing: 4f);
        MakeText(host, "BrandSub",
            mode == Mode.Pause ? "Permainan dijeda" : "WORLD  ·  Virtual Campus",
            14f, UITheme.Muted, FontStyles.Normal,
            new Vector2(0.5f, 0.5f), new Vector2(0f, brandY - 40f), new Vector2(640f, 28f), center: true);
        var rule = MakeImage(host, "BrandRule",
            new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.7f),
            stretch: false, raycast: false);
        PlaceCenter(rule.rectTransform, new Vector2(0f, brandY - 58f), new Vector2(72f, 2f));

        // Welcome chip
        BuildChip(host);

        // Card
        float cardW = 400f;
        bool pause = mode == Mode.Pause;
        int btnCount = pause ? 7 : 6;
        float hPrimary = 52f;
        float hNormal = 44f;
        float gap = 10f;
        float padTop = 52f;
        float padBot = 22f;
        float cardH = padTop + hPrimary + gap
            + (pause ? 4 : 3) * (hNormal + gap)
            + 12f + 2 * (hNormal + gap) + padBot;

        var card = MakeImage(host, "MenuCard", UITheme.PanelDark, stretch: false, raycast: false);
        PlaceCenter(card.rectTransform, new Vector2(0f, -36f), new Vector2(cardW, cardH));
        Outline(card.gameObject, UITheme.Gold, 1.6f);

        var accent = MakeImage(card.transform, "CardAccent", UITheme.Gold, stretch: false, raycast: false);
        var art = accent.rectTransform;
        art.anchorMin = new Vector2(0f, 1f);
        art.anchorMax = new Vector2(1f, 1f);
        art.pivot = new Vector2(0.5f, 1f);
        art.anchoredPosition = Vector2.zero;
        art.sizeDelta = new Vector2(0f, 3f);

        MakeText(card.transform, "CardHeader",
            pause ? "MENU" : "MENU UTAMA",
            13f, UITheme.GoldSoft, FontStyles.Bold,
            new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(cardW - 40f, 22f),
            center: true, spacing: 6f);

        // Buttons
        float top = -padTop;
        float btnW = 320f;

        if (pause)
        {
            MakeButton(card.transform, "BtnPrimary", "LANJUTKAN", top, btnW, hPrimary, true, false, actions?.Primary);
            top -= hPrimary + gap;
            MakeButton(card.transform, "BtnLeaderboard", "LEADERBOARD", top, btnW, hNormal, false, false, actions?.Leaderboard);
            top -= hNormal + gap;
            MakeButton(card.transform, "BtnProfile", "PROFIL", top, btnW, hNormal, false, false, actions?.Profile);
            top -= hNormal + gap;
            MakeButton(card.transform, "BtnHelp", "BANTUAN", top, btnW, hNormal, false, false, actions?.Help);
            top -= hNormal + gap;
            MakeButton(card.transform, "BtnMainMenu", "MENU UTAMA", top, btnW, hNormal, false, false, actions?.MainMenu);
            top -= hNormal + gap + 8f;
            // divider
            var div = MakeImage(card.transform, "Divider",
                new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.25f), false, false);
            PlaceTopCenter(div.rectTransform, top + 4f, new Vector2(btnW - 24f, 1f));
            MakeButton(card.transform, "BtnReset", "RESET PROGRES", top, btnW, hNormal, false, true, actions?.Reset);
            top -= hNormal + gap;
            MakeButton(card.transform, "BtnExit", "KELUAR", top, btnW, hNormal, false, false, actions?.Exit);
        }
        else
        {
            MakeButton(card.transform, "BtnPrimary", "MULAI PETUALANGAN", top, btnW, hPrimary, true, false, actions?.Primary);
            top -= hPrimary + gap;
            MakeButton(card.transform, "BtnLeaderboard", "LEADERBOARD", top, btnW, hNormal, false, false, actions?.Leaderboard);
            top -= hNormal + gap;
            MakeButton(card.transform, "BtnProfile", "PROFIL", top, btnW, hNormal, false, false, actions?.Profile);
            top -= hNormal + gap;
            MakeButton(card.transform, "BtnHelp", "BANTUAN", top, btnW, hNormal, false, false, actions?.Help);
            top -= hNormal + gap + 8f;
            var div = MakeImage(card.transform, "Divider",
                new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.25f), false, false);
            PlaceTopCenter(div.rectTransform, top + 4f, new Vector2(btnW - 24f, 1f));
            MakeButton(card.transform, "BtnReset", "RESET PROGRES", top, btnW, hNormal, false, true, actions?.Reset);
            top -= hNormal + gap;
            MakeButton(card.transform, "BtnExit", "KELUAR", top, btnW, hNormal, false, false, actions?.Exit);
        }

        // Footer
        MakeText(host, "Footer",
            pause ? "ESC  ·  Lanjutkan permainan" : "Misi  ·  Kuis  ·  Eksplorasi Kampus",
            12f, UITheme.Muted, FontStyles.Normal,
            new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(640f, 22f), center: true);

        // Ensure all buttons last-sibling and clickable
        ForceAllButtons(host);

        return new Result
        {
            Root = host.gameObject,
            Canvas = canvas,
            Group = group
        };
    }

    public static void ForceAllButtons(Transform root)
    {
        if (root == null) return;
        var btns = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < btns.Length; i++)
        {
            var b = btns[i];
            if (b == null) continue;
            b.interactable = true;
            b.enabled = true;
            var img = b.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
                b.targetGraphic = img;
            }
            var tmp = b.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null) tmp.raycastTarget = false;
            var cg = b.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
                cg.interactable = true;
                cg.ignoreParentGroups = true;
            }
        }
    }

    static void BuildChip(Transform host)
    {
        var chip = MakeImage(host, "WelcomeChip", UITheme.HudPanel, false, false);
        var rt = chip.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(28f, -28f);
        rt.sizeDelta = new Vector2(240f, 44f);
        Outline(chip.gameObject, UITheme.Gold, 1f);

        var acc = MakeImage(chip.transform, "ChipAccent", UITheme.Gold, false, false);
        var art = acc.rectTransform;
        art.anchorMin = new Vector2(0f, 0.2f);
        art.anchorMax = new Vector2(0f, 0.8f);
        art.pivot = new Vector2(0f, 0.5f);
        art.anchoredPosition = Vector2.zero;
        art.sizeDelta = new Vector2(3f, 0f);

        string player = PlayerPrefs.GetString("playerName", "Mahasiswa");
        if (string.IsNullOrWhiteSpace(player)) player = "Mahasiswa";
        if (player.Length > 16) player = player.Substring(0, 15) + "…";

        var tmp = MakeText(chip.transform, "ChipText", "Halo, " + player,
            14f, UITheme.GoldSoft, FontStyles.Bold,
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, center: false);
        Stretch(tmp.rectTransform);
        tmp.rectTransform.offsetMin = new Vector2(14f, 6f);
        tmp.rectTransform.offsetMax = new Vector2(-10f, -6f);
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
    }

    static void MakeButton(
        Transform parent, string name, string label,
        float top, float w, float h,
        bool primary, bool danger, UnityAction action)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, top);
        rt.sizeDelta = new Vector2(w, h);

        var img = go.GetComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = true;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        btn.colors = primary ? PrimaryColors() : (danger ? DangerColors() : SecondaryColors());
        btn.interactable = true;
        if (action != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }

        Outline(go, danger ? UITheme.Danger : UITheme.Gold, primary ? 1.5f : 1.1f);

        if (!primary)
        {
            var acc = MakeImage(go.transform, "BtnAccent",
                danger ? UITheme.Danger : UITheme.Gold, false, false);
            var art = acc.rectTransform;
            art.anchorMin = new Vector2(0f, 0.15f);
            art.anchorMax = new Vector2(0f, 0.85f);
            art.pivot = new Vector2(0f, 0.5f);
            art.anchoredPosition = Vector2.zero;
            art.sizeDelta = new Vector2(3f, 0f);
        }

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.color = primary ? UITheme.TextOnGold
            : (danger ? new Color(1f, 0.78f, 0.74f) : UITheme.Cream);
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.enableAutoSizing = false;
        tmp.fontSize = primary ? 17f : 15f;
        Stretch(tmp.rectTransform);
        tmp.rectTransform.offsetMin = new Vector2(12f, 2f);
        tmp.rectTransform.offsetMax = new Vector2(-12f, -2f);

        if (go.GetComponent<UIButtonPressFx>() == null)
            go.AddComponent<UIButtonPressFx>();
    }

    static Image MakeImage(Transform parent, string name, Color color, bool stretch, bool raycast)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = raycast;
        if (stretch) Stretch(go.GetComponent<RectTransform>());
        return img;
    }

    static TMP_Text MakeText(
        Transform parent, string name, string text, float size, Color color, FontStyles style,
        Vector2 anchor, Vector2 pos, Vector2 sizeDelta, bool center, float spacing = 0f)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.fontSize = size;
        tmp.enableAutoSizing = false;
        tmp.raycastTarget = false;
        tmp.characterSpacing = spacing;
        tmp.alignment = center ? TextAlignmentOptions.Center : TextAlignmentOptions.Left;

        var rt = go.GetComponent<RectTransform>();
        if (sizeDelta == Vector2.zero && anchor == new Vector2(0.5f, 0.5f))
        {
            // chip text uses stretch set by caller
        }
        else if (anchor == new Vector2(0.5f, 1f))
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
        }
        else if (anchor == new Vector2(0.5f, 0f))
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
        }
        else
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
        }
        return tmp;
    }

    static void PlaceCenter(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void PlaceTopCenter(RectTransform rt, float top, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, top);
        rt.sizeDelta = size;
    }

    static void Stretch(RectTransform rt)
    {
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void Outline(GameObject go, Color color, float distance)
    {
        if (go == null || distance <= 0f) return;
        var o = go.GetComponent<Outline>();
        if (o == null) o = go.AddComponent<Outline>();
        o.effectColor = new Color(color.r, color.g, color.b, 0.75f);
        o.effectDistance = new Vector2(distance, -distance);
        o.useGraphicAlpha = true;
    }

    static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            var c = root.GetChild(i);
            if (c == null) continue;
            // Keep side panels — clearHost must not kill Profile/Help
            string n = c.name;
            if (n == "ProfilePanel" || n == "HelpPanel") continue;
            c.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(c.gameObject);
        }
    }

    static ColorBlock PrimaryColors() => new ColorBlock
    {
        normalColor = UITheme.Gold,
        highlightedColor = UITheme.GoldSoft,
        pressedColor = new Color(0.55f, 0.44f, 0.14f, 1f),
        selectedColor = UITheme.GoldSoft,
        disabledColor = UITheme.ButtonDisabled,
        colorMultiplier = 1f,
        fadeDuration = 0.1f
    };

    static ColorBlock SecondaryColors() => new ColorBlock
    {
        normalColor = UITheme.ButtonNormal,
        highlightedColor = new Color(0.28f, 0.24f, 0.14f, 1f),
        pressedColor = UITheme.ButtonPressed,
        selectedColor = new Color(0.32f, 0.28f, 0.16f, 1f),
        disabledColor = UITheme.ButtonDisabled,
        colorMultiplier = 1f,
        fadeDuration = 0.1f
    };

    static ColorBlock DangerColors() => new ColorBlock
    {
        normalColor = new Color(0.20f, 0.10f, 0.12f, 1f),
        highlightedColor = new Color(0.48f, 0.18f, 0.18f, 1f),
        pressedColor = new Color(0.32f, 0.10f, 0.10f, 1f),
        selectedColor = new Color(0.48f, 0.18f, 0.18f, 1f),
        disabledColor = UITheme.ButtonDisabled,
        colorMultiplier = 1f,
        fadeDuration = 0.1f
    };
}
