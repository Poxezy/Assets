using UnityEngine;

/// <summary>
/// Main Menu / UI entry for full progress reset.
/// Must clear ScoreManager memory (DontDestroyOnLoad), not only PlayerPrefs.
/// </summary>
public class ResetData : MonoBehaviour
{
    public void ResetProgress()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetProgress();
        }
        else
        {
            // No live ScoreManager — wipe prefs keys only
            PlayerPrefs.DeleteKey("PlayerScore");
            PlayerPrefs.DeleteKey("PlayerLevel");
            PlayerPrefs.DeleteKey("UnlockedBadges");
            PlayerPrefs.DeleteKey("UnlockedAreas");
            PlayerPrefs.SetInt("PlayerScore", 0);
            PlayerPrefs.SetInt("PlayerLevel", 1);
            KnowledgeItem.ClearAllCollected();
            PlayerPrefs.Save();
        }

        Debug.Log("Progress di-reset. POINT = 0, Level = 1.");
    }
}
