using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Local leaderboard display + exclusive dark-gold style.
/// </summary>
public class LeaderboardDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text leaderboardText;
    [SerializeField] private int topCount = 10;

    private void Start()
    {
        UIMotion.EnsureInit();
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            ExclusiveUIStyler.Apply(canvas.transform);
            var cg = canvas.GetComponent<CanvasGroup>();
            if (cg == null) cg = canvas.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            UIMotion.FadeCanvas(cg, 1f, 0.25f);
        }

        EnsureBackButton(canvas);
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
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            var cg = canvas.GetComponent<CanvasGroup>();
            if (cg == null) cg = canvas.gameObject.AddComponent<CanvasGroup>();
            UIMotion.FadeAndLoad(cg, "MainMenu", 0.2f);
            return;
        }
        SceneManager.LoadScene("MainMenu");
    }

    void EnsureBackButton(Canvas canvas)
    {
        if (canvas == null) return;
        if (GameObject.Find("BackButton") != null) return;

        var go = new GameObject("BackButton", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(40, -40);
        rt.sizeDelta = new Vector2(180, 52);

        var img = go.AddComponent<Image>();
        img.color = Color.white;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(BackToMainMenu);

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "← BACK";
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = UITheme.Cream;
        tmp.fontStyle = FontStyles.Bold;

        ExclusiveUIStyler.Apply(go.transform);
    }
}
