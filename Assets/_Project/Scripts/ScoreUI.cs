using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelText;

    private void Update()
    {
        if (ScoreManager.Instance == null)
        {
            scoreText.text = "POINT: 0";
            levelText.text = "LEVEL: 1";
            return;
        }

        scoreText.text =
            "POINT: " + ScoreManager.Instance.Score;

        levelText.text =
            "LEVEL: " + ScoreManager.Instance.Level;
    }
}