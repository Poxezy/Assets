using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamificationUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text badgeText;

    [Header("Reward Popup")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private TMP_Text rewardText;

    [SerializeField] private float rewardDuration = 2.5f;

    private Coroutine rewardCoroutine;
    private TMP_Text profileNameText;
    private bool layoutFixed;

    private void Start()
    {
        FixCanvasAndHudLayout();

        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            // Gameplay: HUD + reward only
            HideLegacyMenuOnGameplay(canvas.transform);
            Transform hud = FindChildByName(canvas.transform, "HDDPanel")
                ?? FindChildByName(canvas.transform, "HUDPanel");
            if (hud != null)
                ExclusiveUIStyler.Apply(hud);
            if (rewardPanel != null)
                ExclusiveUIStyler.Apply(rewardPanel.transform);
        }

        StartCoroutine(WaitForScoreManager());
    }

    static void HideLegacyMenuOnGameplay(Transform canvasRoot)
    {
        if (canvasRoot == null) return;
        // Thin guard if old chrome reappears in hierarchy
        string[] hide = { "Menupanel", "MenuPanel", "Background", "PausePanel", "PauseTitle" };
        for (int i = 0; i < hide.Length; i++)
        {
            var t = FindChildByName(canvasRoot, hide[i]);
            if (t == null) continue;
            if (IsUnderName(t, "HDDPanel") || IsUnderName(t, "HUDPanel") || IsUnderName(t, "RewardPanel"))
                continue;
            t.gameObject.SetActive(false);
        }
    }

    static bool IsUnderName(Transform t, string ancestorName)
    {
        while (t != null)
        {
            if (t.name == ancestorName) return true;
            t = t.parent;
        }
        return false;
    }

    private IEnumerator WaitForScoreManager()
    {
        while (ScoreManager.Instance == null)
            yield return null;

        ScoreManager.Instance.OnProgressChanged += UpdateHUD;
        ScoreManager.Instance.OnRewardReceived += ShowReward;

        UpdateHUD();
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance == null)
            return;

        ScoreManager.Instance.OnProgressChanged -= UpdateHUD;
        ScoreManager.Instance.OnRewardReceived -= ShowReward;
    }

    private void UpdateHUD()
    {
        if (scoreText != null)
            scoreText.text = "POINT  ·  " + ScoreManager.Instance.Score;

        if (levelText != null)
            levelText.text = "LEVEL  ·  " + ScoreManager.Instance.Level;

        if (badgeText != null)
            badgeText.text = "BADGE  ·  " + ScoreManager.Instance.GetBadgeCount();

        if (profileNameText != null)
        {
            string name = PlayerPrefs.GetString("playerName", "Mahasiswa");
            if (string.IsNullOrWhiteSpace(name)) name = "Mahasiswa";
            if (name.Length > 18) name = name.Substring(0, 17) + "…";
            profileNameText.text = name;
        }
    }

    private void ShowReward(string message)
    {
        if (rewardCoroutine != null)
            StopCoroutine(rewardCoroutine);

        rewardCoroutine = StartCoroutine(ShowRewardCoroutine(message));
    }

    private IEnumerator ShowRewardCoroutine(string message)
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
            var cg = rewardPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = rewardPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            UIMotion.FadeCanvas(cg, 1f, 0.18f);
            var rt = rewardPanel.GetComponent<RectTransform>();
            if (rt != null) UIMotion.PopIn(rt, 0.2f);
        }

        if (rewardText != null)
        {
            rewardText.color = UITheme.GoldSoft;
            rewardText.text = message;
            UITheme.FitText(rewardText, 18f, wrap: true);
        }

        yield return new WaitForSecondsRealtime(rewardDuration);

        if (rewardPanel != null)
        {
            var cg = rewardPanel.GetComponent<CanvasGroup>();
            if (cg != null) UIMotion.FadeCanvas(cg, 0f, 0.12f);
            rewardPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Repair broken prefab layout: tiny canvas ref, children outside parent, overflow text.
    /// </summary>
    void FixCanvasAndHudLayout()
    {
        if (layoutFixed) return;
        layoutFixed = true;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        Transform hud = FindChildByName(canvas.transform, "HDDPanel");
        if (hud == null) hud = FindChildByName(canvas.transform, "HUDPanel");
        if (hud != null)
            LayoutHudCard(hud as RectTransform);

        rewardPanel = ResolveRewardPanel(canvas.transform, rewardPanel);
        if (rewardPanel != null)
            LayoutRewardCard(rewardPanel.GetComponent<RectTransform>());

        // Auto-bind missing TMP if inspector empty
        if (scoreText == null) scoreText = FindTmp(canvas.transform, "ScoreText");
        if (levelText == null) levelText = FindTmp(canvas.transform, "LevelText");
        if (badgeText == null) badgeText = FindTmp(canvas.transform, "BadgeText");
        if (rewardText == null) rewardText = FindTmp(canvas.transform, "RewardText");

        ClampHudTexts();
    }

    static GameObject ResolveRewardPanel(Transform canvasRoot, GameObject assigned)
    {
        // Scene miswire: rewardPanel often points at RewardText (TMP)
        if (assigned != null && assigned.GetComponent<TMP_Text>() != null)
        {
            Transform p = assigned.transform.parent;
            if (p != null)
                assigned = p.gameObject;
            else
                assigned = null;
        }

        if (assigned != null
            && assigned.GetComponent<TMP_Text>() == null
            && assigned.GetComponent<RectTransform>() != null)
            return assigned;

        var rp = FindChildByName(canvasRoot, "RewardPanel");
        return rp != null ? rp.gameObject : null;
    }

    void LayoutHudCard(RectTransform hud)
    {
        // Top-left premium chip
        hud.anchorMin = new Vector2(0f, 1f);
        hud.anchorMax = new Vector2(0f, 1f);
        hud.pivot = new Vector2(0f, 1f);
        hud.anchoredPosition = new Vector2(24f, -24f);
        hud.sizeDelta = new Vector2(260f, 148f);

        var img = hud.GetComponent<Image>();
        if (img == null && hud.GetComponent<Graphic>() == null)
            img = hud.gameObject.AddComponent<Image>();
        if (img != null)
        {
            img.color = UITheme.HudPanel;
            img.raycastTarget = false;
        }

        if (hud.GetComponent<RectMask2D>() == null)
            hud.gameObject.AddComponent<RectMask2D>();

        // Gold accent bar
        EnsureAccentBar(hud, "HudAccent");

        // Stack texts inside with padding
        float y = -18f;
        profileNameText = EnsureHudLine(hud, "ProfileNameText", "Mahasiswa", y, 16f, UITheme.GoldSoft, true);
        y -= 28f;
        scoreText = EnsureHudLine(hud, "ScoreText", scoreText, "POINT  ·  0", y, 18f, UITheme.Cream, false);
        y -= 28f;
        levelText = EnsureHudLine(hud, "LevelText", levelText, "LEVEL  ·  1", y, 18f, UITheme.Cream, false);
        y -= 28f;
        badgeText = EnsureHudLine(hud, "BadgeText", badgeText, "BADGE  ·  0", y, 18f, UITheme.Cream, false);
    }

    void LayoutRewardCard(RectTransform reward)
    {
        if (reward == null) return;

        // Never style a TMP GO as the panel
        if (reward.GetComponent<TMP_Text>() != null)
        {
            Debug.LogWarning("GamificationUI: rewardPanel points to text GO — skip layout.");
            return;
        }

        reward.anchorMin = new Vector2(0f, 1f);
        reward.anchorMax = new Vector2(0f, 1f);
        reward.pivot = new Vector2(0f, 1f);
        reward.anchoredPosition = new Vector2(24f, -186f);
        reward.sizeDelta = new Vector2(300f, 72f);

        var img = reward.GetComponent<Image>();
        if (img == null)
        {
            // Only add Image if no other Graphic on this GO
            if (reward.GetComponent<Graphic>() == null)
                img = reward.gameObject.AddComponent<Image>();
        }
        if (img != null)
        {
            img.color = UITheme.PanelDark;
            img.raycastTarget = false;
        }

        if (reward.GetComponent<RectMask2D>() == null)
            reward.gameObject.AddComponent<RectMask2D>();

        EnsureAccentBar(reward, "RewardAccent");

        if (rewardText == null)
            rewardText = FindTmp(reward, "RewardText");
        if (rewardText == null)
            rewardText = FindTmp(reward.root, "RewardText");

        if (rewardText == null)
        {
            var go = new GameObject("RewardText", typeof(RectTransform));
            go.transform.SetParent(reward, false);
            rewardText = go.AddComponent<TextMeshProUGUI>();
        }
        else if (rewardText.transform.parent != reward)
        {
            rewardText.transform.SetParent(reward, false);
        }

        var rt = rewardText.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(14f, 10f);
        rt.offsetMax = new Vector2(-14f, -10f);
        rewardText.alignment = TextAlignmentOptions.Center;
        rewardText.color = UITheme.GoldSoft;
        UITheme.FitText(rewardText, 17f, wrap: true);
    }

    static void EnsureAccentBar(RectTransform parent, string name)
    {
        Transform existing = parent.Find(name);
        GameObject bar;
        if (existing != null)
            bar = existing.gameObject;
        else
        {
            bar = new GameObject(name, typeof(RectTransform));
            bar.transform.SetParent(parent, false);
        }

        var rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0f, 3f);
        var img = bar.GetComponent<Image>();
        if (img == null) img = bar.AddComponent<Image>();
        img.color = UITheme.Gold;
        img.raycastTarget = false;
    }

    TMP_Text EnsureHudLine(
        RectTransform parent,
        string childName,
        TMP_Text existing,
        string fallback,
        float yFromTop,
        float fontSize,
        Color color,
        bool goldStyle)
    {
        TMP_Text tmp = existing;
        if (tmp == null)
            tmp = FindTmp(parent, childName);

        if (tmp == null)
        {
            // Prefab may have texts as siblings of HUD, not children — search canvas
            tmp = FindTmp(parent.root, childName);
        }

        if (tmp == null)
        {
            var go = new GameObject(childName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = fallback;
        }

        if (tmp.transform.parent != parent)
            tmp.transform.SetParent(parent, false);

        var rt = tmp.rectTransform;
        // Stretch X with 14px inset, fixed height from top
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, yFromTop);
        rt.sizeDelta = new Vector2(-28f, 26f);

        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.fontStyle = goldStyle ? FontStyles.Bold : FontStyles.Normal;
        UITheme.FitText(tmp, fontSize, wrap: false);
        return tmp;
    }

    TMP_Text EnsureHudLine(
        RectTransform parent,
        string childName,
        string fallback,
        float yFromTop,
        float fontSize,
        Color color,
        bool goldStyle)
    {
        return EnsureHudLine(parent, childName, null, fallback, yFromTop, fontSize, color, goldStyle);
    }

    void ClampHudTexts()
    {
        UITheme.FitText(scoreText, 18f, false);
        UITheme.FitText(levelText, 18f, false);
        UITheme.FitText(badgeText, 18f, false);
        UITheme.FitText(rewardText, 17f, true);
        UITheme.FitText(profileNameText, 16f, false);
    }

    static TMP_Text FindTmp(Transform root, string name)
    {
        var t = FindChildByName(root, name);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }

    static Transform FindChildByName(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var found = FindChildByName(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
