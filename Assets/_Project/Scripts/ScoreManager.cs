using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; set; }
    public int Level { get; set; } = 1;

    // Alias untuk kompatibilitas dengan SaveLoadManager
    public int currentXP { get => Score; set => Score = value; }
    public int currentLevel { get => Level; set => Level = value; }
    public int totalScore { get => Score; set => Score = value; }

    public event Action OnProgressChanged;
    public event Action<string> OnRewardReceived;

    private readonly HashSet<string> unlockedBadges = new();
    private readonly HashSet<string> unlockedAreas = new();

    private const int PointsPerLevel = 100;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int previousLevel = Level;

        Score += amount;
        Level = (Score / PointsPerLevel) + 1;

        OnRewardReceived?.Invoke($"+{amount} POINT");

        if (Level > previousLevel)
        {
            OnRewardReceived?.Invoke($"LEVEL UP! LEVEL {Level}");
        }

        CheckBadges();
        CheckUnlockedAreas();
        SaveProgress();
        SaveToDatabase();

        OnProgressChanged?.Invoke();
    }

    private void CheckBadges()
    {
        UnlockBadgeIfEligible(
            badgeId: "first_item",
            badgeName: "Knowledge Beginner",
            requiredScore: 50
        );

        UnlockBadgeIfEligible(
            badgeId: "knowledge_explorer",
            badgeName: "Knowledge Explorer",
            requiredScore: 100
        );

        UnlockBadgeIfEligible(
            badgeId: "campus_scholar",
            badgeName: "Campus Scholar",
            requiredScore: 300
        );

        UnlockBadgeIfEligible(
            badgeId: "metaedu_master",
            badgeName: "MetaEdu Master",
            requiredScore: 500
        );
    }

    private void UnlockBadgeIfEligible(
        string badgeId,
        string badgeName,
        int requiredScore)
    {
        if (Score < requiredScore ||
            unlockedBadges.Contains(badgeId))
        {
            return;
        }

        unlockedBadges.Add(badgeId);

        OnRewardReceived?.Invoke(
            $"BADGE DIPEROLEH\n{badgeName}"
        );
    }

    private void CheckUnlockedAreas()
    {
        UnlockArea("Campus", 1);
        UnlockArea("Classroom", 1);
        UnlockArea("Library", 2);
        UnlockArea("ComputerLab", 3);
        UnlockArea("TechnologyCenter", 4);
    }

    private void UnlockArea(
        string areaName,
        int requiredLevel)
    {
        if (Level < requiredLevel ||
            unlockedAreas.Contains(areaName))
        {
            return;
        }

        unlockedAreas.Add(areaName);

        if (requiredLevel > 1)
        {
            OnRewardReceived?.Invoke(
                $"AREA TERBUKA\n{areaName}"
            );
        }
    }

    public bool IsAreaUnlocked(string areaName)
    {
        return unlockedAreas.Contains(areaName);
    }

    public bool HasBadge(string badgeId)
    {
        return unlockedBadges.Contains(badgeId);
    }

    public int GetBadgeCount()
    {
        return unlockedBadges.Count;
    }

    public System.Collections.Generic.List<string> GetUnlockedBadges()
    {
        return new System.Collections.Generic.List<string>(unlockedBadges);
    }

    public void SetUnlockedBadges(System.Collections.Generic.List<string> badges)
    {
        unlockedBadges.Clear();
        if (badges != null)
        {
            foreach (string b in badges)
            {
                if (!string.IsNullOrWhiteSpace(b))
                    unlockedBadges.Add(b);
            }
        }
    }

    public void UnlockBadge(string badgeName)
    {
        if (string.IsNullOrEmpty(badgeName)) return;
        if (unlockedBadges.Contains(badgeName)) return;

        unlockedBadges.Add(badgeName);
        OnRewardReceived?.Invoke($"BADGE DIPEROLEH\n{badgeName}");
        SaveProgress();
    }

    public void AddXP(int amount)
    {
        AddScore(amount);
    }

    public string GetBadgesAsText()
    {
        if (unlockedBadges.Count == 0)
        {
            return "Badge: 0";
        }

        return $"Badge: {unlockedBadges.Count}";
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("PlayerScore", Score);
        PlayerPrefs.SetInt("PlayerLevel", Level);

        PlayerPrefs.SetString(
            "UnlockedBadges",
            string.Join("|", unlockedBadges)
        );

        PlayerPrefs.SetString(
            "UnlockedAreas",
            string.Join("|", unlockedAreas)
        );

        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        Score = PlayerPrefs.GetInt("PlayerScore", 0);
        Level = PlayerPrefs.GetInt("PlayerLevel", 1);

        LoadHashSet(
            PlayerPrefs.GetString("UnlockedBadges", ""),
            unlockedBadges
        );

        LoadHashSet(
            PlayerPrefs.GetString("UnlockedAreas", ""),
            unlockedAreas
        );

        CheckBadges();
        CheckUnlockedAreas();
    }

    private static void LoadHashSet(
        string savedData,
        HashSet<string> target)
    {
        target.Clear();

        if (string.IsNullOrWhiteSpace(savedData))
        {
            return;
        }

        string[] values = savedData.Split('|');

        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                target.Add(value);
            }
        }
    }

    private void SaveToDatabase()
    {
        if (DatabaseManager.Instance == null)
        {
            Debug.LogWarning(
                "DatabaseManager tidak ditemukan. " +
                "Progress hanya disimpan secara lokal."
            );

            return;
        }

        string playerName = PlayerPrefs.GetString(
            "PlayerName",
            "Fendi"
        );

        DatabaseManager.Instance.SaveScore(
            playerName,
            Score,
            Level
        );
    }

    public void ResetProgress()
    {
        Score = 0;
        Level = 1;

        unlockedBadges.Clear();
        unlockedAreas.Clear();

        PlayerPrefs.DeleteKey("PlayerScore");
        PlayerPrefs.DeleteKey("PlayerLevel");
        PlayerPrefs.DeleteKey("UnlockedBadges");
        PlayerPrefs.DeleteKey("UnlockedAreas");
        // Write zeros explicitly so reload cannot revive old score
        PlayerPrefs.SetInt("PlayerScore", 0);
        PlayerPrefs.SetInt("PlayerLevel", 1);
        KnowledgeItem.ClearAllCollected();
        if (MetaEdu.Quest.QuestManager.Instance != null)
            MetaEdu.Quest.QuestManager.Instance.ResetAllQuests();
        PlayerPrefs.Save();

        CheckUnlockedAreas();
        SaveProgress();
        SaveToDatabase();

        OnProgressChanged?.Invoke();
        OnRewardReceived?.Invoke("PROGRESS DI-RESET");
    }
}
