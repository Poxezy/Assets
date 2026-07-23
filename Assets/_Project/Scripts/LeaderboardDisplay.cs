using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Local leaderboard display + exclusive dark-gold style + working Back.
/// </summary>
public class LeaderboardDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text leaderboardText;
    [SerializeField] private int topCount = 10;

    Canvas rootCanvas;
    CanvasGroup canvasGroup;
    bool leaving;

    private void Start()
    {
        UIMotion.EnsureInit();
        EventSystemGuard.Ensure();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) rootCanvas = FindAnyObjectByType<Canvas>();
        if (rootCanvas == null)
        {
            Debug.LogError("LeaderboardDisplay: no Canvas.");
            return;
        }

        // Kill full-screen raycast blockers under canvas (Background Image, etc.)
        FixCanvasRaycasts(rootCanvas);

        if (rootCanvas.GetComponent<GraphicRaycaster>() == null)
            rootCanvas.gameObject.AddComponent<GraphicRaycaster>();
        rootCanvas.sortingOrder = Mathf.Max(rootCanvas.sortingOrder, 100);

        canvasGroup = rootCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = rootCanvas.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.ignoreParentGroups = true;

        ExclusiveUIStyler.Apply(rootCanvas.transform);
        EnsureBackButton(rootCanvas);
        ExclusiveMenuUI.ForceAllButtons(rootCanvas.transform);
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        if (leaderboardText == null) return;

        if (DatabaseManager.Instance == null)
        {
            leaderboardText.text = "Leaderboard belum tersedia";
            leaderboardText.color = UITheme.Muted;
            return;
        }

        LeaderboardResponse response = DatabaseManager.Instance.GetLeaderboard(topCount);

        if (!response.success || response.players == null || response.players.Length == 0)
        {
            leaderboardText.text = "Belum ada data leaderboard";
            leaderboardText.color = UITheme.Muted;
            return;
        }

        leaderboardText.color = UITheme.Cream;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("<color=#D4B038>RANK    PLAYER              LEVEL    POINT</color>");
        sb.AppendLine("<color=#6E6558>────────────────────────────────────────</color>");

        for (int i = 0; i < response.players.Length; i++)
        {
            PlayerLeaderboardData player = response.players[i];
            string rankColor = i == 0 ? "#FFD76A" : i < 3 ? "#E2C56A" : "#F5EEDB";
            string name = player.player_name ?? "-";
            if (name.Length > 16) name = name.Substring(0, 16);

            sb.AppendLine(
                $"<color={rankColor}>{i + 1,2}.    {name,-16}   Lv.{player.level,-3}   {player.score}</color>");
        }

        leaderboardText.richText = true;
        leaderboardText.text = sb.ToString();
    }

    public void BackToMainMenu()
    {
        if (leaving) return;
        leaving = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Direct load — more reliable than fade if canvas alpha was 0
        SceneManager.LoadScene("MainMenu");
    }

    void EnsureBackButton(Canvas canvas)
    {
        if (canvas == null) return;

        // Reuse or rebuild
        Transform existing = canvas.transform.Find("BackButton");
        GameObject go;
        if (existing != null)
            go = existing.gameObject;
        else
        {
            go = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas.transform, false);
        }

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(40, -40);
        rt.sizeDelta = new Vector2(200, 56);
        go.transform.SetAsLastSibling();

        var img = go.GetComponent<Image>();
        if (img == null) img = go.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = true;

        var btn = go.GetComponent<Button>();
        if (btn == null) btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.interactable = true;
        btn.transition = Selectable.Transition.ColorTint;
        btn.colors = UITheme.ButtonColors();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(BackToMainMenu);

        TMP_Text tmp = go.GetComponentInChildren<TMP_Text>(true);
        if (tmp == null)
        {
            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            tmp = textGo.AddComponent<TextMeshProUGUI>();
        }
        tmp.text = "← KEMBALI";
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = UITheme.TextOnGold;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;

        if (go.GetComponent<UIButtonPressFx>() == null)
            go.AddComponent<UIButtonPressFx>();

        ExclusiveUIStyler.Apply(go.transform);
        ExclusiveMenuUI.ForceAllButtons(go.transform);
    }

    static void FixCanvasRaycasts(Canvas canvas)
    {
        if (canvas == null) return;
        // Decorative full-screen images must not steal clicks from buttons
        var images = canvas.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            var img = images[i];
            if (img == null) continue;
            // Keep raycast on actual buttons only
            if (img.GetComponent<Button>() != null) continue;
            string n = img.gameObject.name;
            if (n.IndexOf("Background", System.StringComparison.OrdinalIgnoreCase) >= 0
                || n.IndexOf("Panel", System.StringComparison.OrdinalIgnoreCase) >= 0
                || n.IndexOf("Dim", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                img.raycastTarget = false;
            }
        }

        var texts = canvas.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
            if (texts[i] != null) texts[i].raycastTarget = false;
    }
}
