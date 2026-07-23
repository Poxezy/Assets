using MetaEdu.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Exclusive profile panel (name + avatar). Builds own UI under Canvas if none assigned.
/// </summary>
public class ProfilePanel : MonoBehaviour
{
    [SerializeField] GameObject panelRoot;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] TMP_Text avatarLabel;
    [SerializeField] Button prevAvatarButton;
    [SerializeField] Button nextAvatarButton;
    [SerializeField] Button saveButton;
    [SerializeField] Button closeButton;

    static readonly string[] Avatars = { "DefaultMale", "DefaultFemale" };

    int avatarIndex;
    bool built;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    void Awake()
    {
        if (panelRoot == null)
            BuildRuntimeUI();

        WireButtons();
        Hide();
    }

    public void Show()
    {
        EnsurePanel();
        if (panelRoot == null) return;
        LoadFromGame();
        panelRoot.SetActive(true);
        panelRoot.transform.SetAsLastSibling();
        ExclusiveUIStyler.Apply(panelRoot.transform);
        StyleProfileActions();
        ExclusiveMenuUI.ForceAllButtons(panelRoot.transform);
        var cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg == null) cg = panelRoot.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
        cg.ignoreParentGroups = true;
        UIMotion.FadeCanvas(cg, 1f, 0.18f);
        Transform cardT = panelRoot.transform.Find("ProfileCard");
        if (cardT == null && panelRoot.transform.childCount > 0)
            cardT = panelRoot.transform.GetChild(0);
        if (cardT is RectTransform crt) UIMotion.PopIn(crt, 0.2f);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void EnsurePanel()
    {
        // Destroyed by clearHost → Unity fake-null; must rebuild
        if (panelRoot == null)
        {
            built = false;
            nameInput = null;
            avatarLabel = null;
            prevAvatarButton = null;
            nextAvatarButton = null;
            saveButton = null;
            closeButton = null;
            BuildRuntimeUI();
            return;
        }
        // Reparent to active top canvas (pause overlay when open)
        var host = ResolveHostCanvas();
        if (host != null && panelRoot.transform.parent != host.transform)
        {
            panelRoot.transform.SetParent(host.transform, false);
            StretchFull(panelRoot.GetComponent<RectTransform>());
        }
    }

    void StyleProfileActions()
    {
        if (saveButton != null)
        {
            saveButton.transition = Selectable.Transition.ColorTint;
            saveButton.colors = new ColorBlock
            {
                normalColor = UITheme.Gold,
                highlightedColor = UITheme.GoldSoft,
                pressedColor = UITheme.ButtonPressed,
                selectedColor = UITheme.GoldSoft,
                disabledColor = UITheme.ButtonDisabled,
                colorMultiplier = 1f,
                fadeDuration = 0.12f
            };
            var st = saveButton.GetComponentInChildren<TMP_Text>(true);
            if (st != null)
            {
                st.text = "SIMPAN";
                st.color = UITheme.TextOnGold;
                st.fontStyle = FontStyles.Bold;
            }
            if (saveButton.GetComponent<UIButtonPressFx>() == null)
                saveButton.gameObject.AddComponent<UIButtonPressFx>();
        }
        if (closeButton != null)
        {
            closeButton.transition = Selectable.Transition.ColorTint;
            closeButton.colors = UITheme.ButtonColors();
            var ct = closeButton.GetComponentInChildren<TMP_Text>(true);
            if (ct != null)
            {
                ct.text = "TUTUP";
                ct.color = UITheme.Cream;
                ct.fontStyle = FontStyles.Bold;
            }
            if (closeButton.GetComponent<UIButtonPressFx>() == null)
                closeButton.gameObject.AddComponent<UIButtonPressFx>();
        }
        if (prevAvatarButton != null && prevAvatarButton.GetComponent<UIButtonPressFx>() == null)
            prevAvatarButton.gameObject.AddComponent<UIButtonPressFx>();
        if (nextAvatarButton != null && nextAvatarButton.GetComponent<UIButtonPressFx>() == null)
            nextAvatarButton.gameObject.AddComponent<UIButtonPressFx>();
    }

    public void Toggle()
    {
        if (IsOpen) Hide();
        else Show();
    }

    void WireButtons()
    {
        if (prevAvatarButton != null)
        {
            prevAvatarButton.onClick.RemoveListener(PrevAvatar);
            prevAvatarButton.onClick.AddListener(PrevAvatar);
        }
        if (nextAvatarButton != null)
        {
            nextAvatarButton.onClick.RemoveListener(NextAvatar);
            nextAvatarButton.onClick.AddListener(NextAvatar);
        }
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(Save);
            saveButton.onClick.AddListener(Save);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }
    }

    void LoadFromGame()
    {
        string name = "Mahasiswa Baru";
        string avatar = Avatars[0];

        if (GameManager.Instance != null)
        {
            name = GameManager.Instance.playerName;
            avatar = GameManager.Instance.selectedAvatar;
        }
        else
        {
            name = PlayerPrefs.GetString("playerName", name);
            avatar = PlayerPrefs.GetString("selectedAvatar", avatar);
        }

        if (nameInput != null) nameInput.text = name;
        avatarIndex = 0;
        for (int i = 0; i < Avatars.Length; i++)
            if (Avatars[i] == avatar) { avatarIndex = i; break; }
        RefreshAvatarLabel();
    }

    void PrevAvatar()
    {
        avatarIndex = (avatarIndex - 1 + Avatars.Length) % Avatars.Length;
        RefreshAvatarLabel();
    }

    void NextAvatar()
    {
        avatarIndex = (avatarIndex + 1) % Avatars.Length;
        RefreshAvatarLabel();
    }

    void RefreshAvatarLabel()
    {
        if (avatarLabel != null)
            avatarLabel.text = Avatars[avatarIndex];
    }

    void Save()
    {
        string name = nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text)
            ? nameInput.text.Trim()
            : "Mahasiswa Baru";
        if (name.Length > 24) name = name.Substring(0, 24);

        string avatar = Avatars[avatarIndex];

        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerName = name;
            GameManager.Instance.selectedAvatar = avatar;
        }

        PlayerPrefs.SetString("playerName", name);
        PlayerPrefs.SetString("selectedAvatar", avatar);
        PlayerPrefs.Save();
        Hide();
    }

    void BuildRuntimeUI()
    {
        if (built && panelRoot != null) return;
        built = true;

        Canvas canvas = ResolveHostCanvas();
        if (canvas == null) { built = false; return; }

        // Prefer stable scaler on host canvas
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null && scaler.referenceResolution.x < 1000f)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        panelRoot = CreateUIObject("ProfilePanel", canvas.transform);
        StretchFull(panelRoot.GetComponent<RectTransform>());
        var dim = panelRoot.AddComponent<Image>();
        dim.color = UITheme.DimOverlay;
        dim.raycastTarget = true;

        var card = CreateUIObject("ProfileCard", panelRoot.transform);
        var cardRt = card.GetComponent<RectTransform>();
        cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(480, 440);
        cardRt.anchoredPosition = Vector2.zero;
        card.AddComponent<Image>().color = UITheme.PanelDark;
        card.AddComponent<RectMask2D>();

        var accent = CreateUIObject("ProfileAccent", card.transform);
        var accentRt = accent.GetComponent<RectTransform>();
        accentRt.anchorMin = new Vector2(0f, 1f);
        accentRt.anchorMax = new Vector2(1f, 1f);
        accentRt.pivot = new Vector2(0.5f, 1f);
        accentRt.anchoredPosition = Vector2.zero;
        accentRt.sizeDelta = new Vector2(0f, 4f);
        accent.AddComponent<Image>().color = UITheme.Gold;

        var frame = CreateUIObject("ProfileFrame", card.transform);
        var frameRt = frame.GetComponent<RectTransform>();
        frameRt.anchorMin = Vector2.zero;
        frameRt.anchorMax = Vector2.one;
        frameRt.offsetMin = new Vector2(12, 12);
        frameRt.offsetMax = new Vector2(-12, -16);
        var frameImg = frame.AddComponent<Image>();
        frameImg.color = UITheme.CardInner;
        frameImg.raycastTarget = false;

        float y = -30f;
        var title = CreateTmp("ProfileTitle", card.transform, "PROFIL PEMAIN", 24, UITheme.Gold, FontStyles.Bold);
        PlaceTop(title.rectTransform, y, 420, 34);
        title.alignment = TextAlignmentOptions.Center;
        y -= 36f;

        var sub = CreateTmp("ProfileSubtitle", card.transform, "Atur nama & avatar untuk leaderboard", 13, UITheme.Muted, FontStyles.Normal);
        PlaceTop(sub.rectTransform, y, 400, 22);
        sub.alignment = TextAlignmentOptions.Center;
        y -= 36f;

        var nameLbl = CreateTmp("NameLabel", card.transform, "NAMA PEMAIN", 12, UITheme.GoldSoft, FontStyles.Bold);
        PlaceTop(nameLbl.rectTransform, y, 400, 20);
        y -= 24f;

        nameInput = CreateInput(card.transform);
        PlaceTop(nameInput.GetComponent<RectTransform>(), y, 400, 46);
        y -= 64f;

        var avLbl = CreateTmp("AvatarHeader", card.transform, "AVATAR", 12, UITheme.GoldSoft, FontStyles.Bold);
        PlaceTop(avLbl.rectTransform, y, 400, 20);
        y -= 28f;

        avatarLabel = CreateTmp("AvatarLabel", card.transform, Avatars[0], 18, UITheme.Cream, FontStyles.Bold);
        PlaceTop(avatarLabel.rectTransform, y, 220, 36);
        avatarLabel.alignment = TextAlignmentOptions.Center;

        prevAvatarButton = CreateButton(card.transform, "PrevAvatar", "‹", new Vector2(-150, y - 18), new Vector2(48, 40), 22);
        nextAvatarButton = CreateButton(card.transform, "NextAvatar", "›", new Vector2(150, y - 18), new Vector2(48, 40), 22);
        y -= 88f;

        saveButton = CreateButton(card.transform, "SaveProfile", "SIMPAN", new Vector2(-90, y - 18), new Vector2(160, 46), 16);
        closeButton = CreateButton(card.transform, "CloseProfile", "TUTUP", new Vector2(90, y - 18), new Vector2(160, 46), 16);

        WireButtons();
        ExclusiveUIStyler.Apply(panelRoot.transform);
        StyleProfileActions();
    }

    static Canvas ResolveHostCanvas()
    {
        // Prefer pause overlay when open so Profile sits above dim
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

    static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void PlaceTop(RectTransform rt, float yFromTop, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, yFromTop);
        rt.sizeDelta = new Vector2(width, height);
    }

    static TMP_Text CreateTmp(string name, Transform parent, string text, float size, Color color, FontStyles style)
    {
        var go = CreateUIObject(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.raycastTarget = false;
        UITheme.FitText(tmp, size, wrap: true);
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 posFromTopCenter, Vector2 size, float fontSize)
    {
        var go = CreateUIObject(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posFromTopCenter;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = Color.white;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var txt = CreateTmp(name + "Text", go.transform, label, fontSize, UITheme.Cream, FontStyles.Bold);
        StretchFull(txt.rectTransform);
        txt.alignment = TextAlignmentOptions.Center;
        UITheme.FitText(txt, fontSize, wrap: false);
        return btn;
    }

    static TMP_InputField CreateInput(Transform parent)
    {
        var go = CreateUIObject("NameInput", parent);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.05f, 0.045f, 0.07f, 1f);

        var textArea = CreateUIObject("TextArea", go.transform);
        StretchFull(textArea.GetComponent<RectTransform>());
        var areaRt = textArea.GetComponent<RectTransform>();
        areaRt.offsetMin = new Vector2(4, 4);
        areaRt.offsetMax = new Vector2(-4, -4);
        textArea.AddComponent<RectMask2D>();

        var textGo = CreateUIObject("Text", textArea.transform);
        StretchFull(textGo.GetComponent<RectTransform>());
        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.fontSize = 18;
        text.color = UITheme.Cream;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        var textRt = text.rectTransform;
        textRt.offsetMin = new Vector2(10, 2);
        textRt.offsetMax = new Vector2(-10, -2);

        var placeholderGo = CreateUIObject("Placeholder", textArea.transform);
        StretchFull(placeholderGo.GetComponent<RectTransform>());
        var placeholder = placeholderGo.AddComponent<TextMeshProUGUI>();
        placeholder.text = "Masukkan nama...";
        placeholder.fontSize = 18;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.color = UITheme.Muted;
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        placeholder.textWrappingMode = TextWrappingModes.NoWrap;
        placeholder.overflowMode = TextOverflowModes.Ellipsis;
        var phRt = placeholder.rectTransform;
        phRt.offsetMin = new Vector2(10, 2);
        phRt.offsetMax = new Vector2(-10, -2);

        var input = go.AddComponent<TMP_InputField>();
        input.textViewport = textArea.GetComponent<RectTransform>();
        input.textComponent = text;
        input.placeholder = placeholder;
        input.caretColor = UITheme.Gold;
        input.selectionColor = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.35f);
        input.pointSize = 18;
        input.characterLimit = 24;
        return input;
    }
}
