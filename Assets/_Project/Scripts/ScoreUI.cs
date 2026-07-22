using TMPro;
using UnityEngine;

/// <summary>
/// Lightweight score/level mirror. Prefer GamificationUI for full HUD.
/// Does not fight layout — only updates text if refs exist.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelText;

    private void Start()
    {
        // Skip if GamificationUI already owns HUD (avoids double restyle thrash)
        if (FindAnyObjectByType<GamificationUI>() != null)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
            ExclusiveUIStyler.Apply(canvas.transform);
    }

    private void Update()
    {
        if (scoreText == null && levelText == null)
            return;

        if (ScoreManager.Instance == null)
        {
            if (scoreText != null) scoreText.text = "POINT  ·  0";
            if (levelText != null) levelText.text = "LEVEL  ·  1";
            return;
        }

        if (scoreText != null)
            scoreText.text = "POINT  ·  " + ScoreManager.Instance.Score;

        if (levelText != null)
            levelText.text = "LEVEL  ·  " + ScoreManager.Instance.Level;
    }
}
