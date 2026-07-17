using System.Collections;
using UnityEngine;
using TMPro;

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

    private void Start()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        StartCoroutine(WaitForScoreManager());
    }

    private IEnumerator WaitForScoreManager()
    {
        while (ScoreManager.Instance == null)
        {
            yield return null;
        }

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
        scoreText.text =
            "POINT : " + ScoreManager.Instance.Score;

        levelText.text =
            "LEVEL : " + ScoreManager.Instance.Level;

        badgeText.text =
            "BADGE : " + ScoreManager.Instance.GetBadgeCount();
    }

    private void ShowReward(string message)
    {
        if (rewardCoroutine != null)
        {
            StopCoroutine(rewardCoroutine);
        }

        rewardCoroutine =
            StartCoroutine(
                ShowRewardCoroutine(message)
            );
    }

    private IEnumerator ShowRewardCoroutine(
        string message)
    {
        rewardPanel.SetActive(true);

        rewardText.text = message;

        yield return new WaitForSeconds(
            rewardDuration
        );

        rewardPanel.SetActive(false);
    }
}