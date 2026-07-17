using System.IO;
using System.Collections.Generic;
using UnityEngine;
using MetaEdu.Core;

namespace MetaEdu.SaveSystem
{
    [System.Serializable]
    public class GameSaveData
    {
        public string playerName;
        public string selectedAvatar;
        public int currentXP;
        public int currentLevel;
        public int totalScore;
        public List<string> unlockedBadges = new List<string>();
        public List<string> completedQuestIDs = new List<string>();
        public List<string> activeQuestIDs = new List<string>();
        
        // Terakhir kali posisi player
        public float playerPosX;
        public float playerPosY;
        public float playerPosZ;
    }

    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }
        private string saveFilePath;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                saveFilePath = Path.Combine(Application.persistentDataPath, "metaedu_save.json");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SaveGame(Vector3 playerPosition)
        {
            GameSaveData data = new GameSaveData
            {
                playerName = GameManager.Instance.playerName,
                selectedAvatar = GameManager.Instance.selectedAvatar,
                currentXP = ScoreManager.Instance.currentXP,
                currentLevel = ScoreManager.Instance.currentLevel,
                totalScore = ScoreManager.Instance.totalScore,
                unlockedBadges = ScoreManager.Instance.GetUnlockedBadges(),
                playerPosX = playerPosition.x,
                playerPosY = playerPosition.y,
                playerPosZ = playerPosition.z
            };

            // Mengambil status misi dari QuestManager jika ada
            if (Quest.QuestManager.Instance != null)
            {
                data.completedQuestIDs = Quest.QuestManager.Instance.GetCompletedQuests();
                data.activeQuestIDs = Quest.QuestManager.Instance.GetActiveQuests();
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log("Game Saved to: " + saveFilePath);
        }

        public GameSaveData LoadGame()
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

                GameManager.Instance.playerName = data.playerName;
                GameManager.Instance.selectedAvatar = data.selectedAvatar;
                ScoreManager.Instance.currentXP = data.currentXP;
                ScoreManager.Instance.currentLevel = data.currentLevel;
                ScoreManager.Instance.AddScore(data.totalScore - ScoreManager.Instance.totalScore);
                ScoreManager.Instance.SetUnlockedBadges(data.unlockedBadges);

                if (Quest.QuestManager.Instance != null)
                {
                    Quest.QuestManager.Instance.LoadQuestProgress(data.activeQuestIDs, data.completedQuestIDs);
                }

                Debug.Log("Game Loaded successfully.");
                return data;
            }
            else
            {
                Debug.LogWarning("No save file found.");
                return null;
            }
        }

        public void ResetProgress()
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }
            
            GameManager.Instance.playerName = "Mahasiswa Baru";
            GameManager.Instance.selectedAvatar = "DefaultMale";
            ScoreManager.Instance.currentXP = 0;
            ScoreManager.Instance.currentLevel = 1;
            ScoreManager.Instance.SetUnlockedBadges(new List<string>());
            
            if (Quest.QuestManager.Instance != null)
            {
                Quest.QuestManager.Instance.ResetAllQuests();
            }

            Debug.Log("Progress Reset.");
        }
    }
}
