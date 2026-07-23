using UnityEngine;

/// <summary>
/// Main Menu / UI entry for full progress reset.
/// Must clear ScoreManager memory (DontDestroyOnLoad), not only PlayerPrefs.
/// </summary>
public class ResetData : MonoBehaviour
{
    public void ResetProgress()
    {
        // Clear stuck quiz overlay (blocks Main Menu clicks if left open)
        if (MetaEdu.Quiz.QuizManager.Instance != null)
            MetaEdu.Quiz.QuizManager.Instance.ForceAbort();
        if (MetaEdu.Quiz.QuizUI.Instance != null)
            MetaEdu.Quiz.QuizUI.Instance.ForceClose();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetProgress();
        }
        else
        {
            PlayerPrefs.DeleteKey("PlayerScore");
            PlayerPrefs.DeleteKey("PlayerLevel");
            PlayerPrefs.DeleteKey("UnlockedBadges");
            PlayerPrefs.DeleteKey("UnlockedAreas");
            PlayerPrefs.DeleteKey("MetaEdu.Quest.State");
            PlayerPrefs.SetInt("PlayerScore", 0);
            PlayerPrefs.SetInt("PlayerLevel", 1);
            KnowledgeItem.ClearAllCollected();
            PlayerPrefs.Save();
        }

        // Quest DDOL may exist without ScoreManager path having instance
        if (MetaEdu.Quest.QuestManager.Instance != null)
        {
            MetaEdu.Quest.QuestManager.Instance.ResetAllQuests();
            MetaEdu.Quest.QuestManager.Instance.RestartIntroQuest();
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Progress di-reset. POINT = 0, Level = 1. Petualangan siap mulai.");
    }
}
