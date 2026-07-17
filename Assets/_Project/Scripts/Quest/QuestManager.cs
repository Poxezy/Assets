using System.Collections.Generic;
using UnityEngine;

namespace MetaEdu.Quest
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public List<QuestData> allQuests = new List<QuestData>();
        
        private Dictionary<string, QuestData> questDatabase = new Dictionary<string, QuestData>();

        public System.Action<QuestData> OnQuestActivated;
        public System.Action<QuestData> OnQuestCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDatabase()
        {
            foreach (var quest in allQuests)
            {
                if (quest != null && !questDatabase.ContainsKey(quest.questID))
                {
                    questDatabase.Add(quest.questID, quest);
                }
            }
        }

        public void ActivateQuest(string questID)
        {
            if (questDatabase.TryGetValue(questID, out QuestData quest))
            {
                if (quest.status == QuestStatus.Available || quest.status == QuestStatus.Locked)
                {
                    quest.status = QuestStatus.Active;
                    OnQuestActivated?.Invoke(quest);
                }
            }
        }

        public void CompleteQuest(string questID)
        {
            if (questDatabase.TryGetValue(questID, out QuestData quest))
            {
                if (quest.status == QuestStatus.Active)
                {
                    quest.status = QuestStatus.Completed;
                    
                    // Berikan reward XP
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddXP(quest.xpReward);
                        if (!string.IsNullOrEmpty(quest.badgeReward))
                        {
                            ScoreManager.Instance.UnlockBadge(quest.badgeReward);
                        }
                    }

                    OnQuestCompleted?.Invoke(quest);
                    UnlockNextQuests(questID);
                }
            }
        }

        private void UnlockNextQuests(string completedQuestID)
        {
            foreach (var quest in questDatabase.Values)
            {
                if (quest.prerequisiteQuestID == completedQuestID && quest.status == QuestStatus.Locked)
                {
                    quest.status = QuestStatus.Available;
                }
            }
        }

        public List<string> GetCompletedQuests()
        {
            List<string> completed = new List<string>();
            foreach (var quest in questDatabase.Values)
            {
                if (quest.status == QuestStatus.Completed)
                    completed.Add(quest.questID);
            }
            return completed;
        }

        public List<string> GetActiveQuests()
        {
            List<string> active = new List<string>();
            foreach (var quest in questDatabase.Values)
            {
                if (quest.status == QuestStatus.Active)
                    active.Add(quest.questID);
            }
            return active;
        }

        public void LoadQuestProgress(List<string> activeIDs, List<string> completedIDs)
        {
            ResetAllQuests();
            foreach (var id in completedIDs)
            {
                if (questDatabase.TryGetValue(id, out QuestData q))
                {
                    q.status = QuestStatus.Completed;
                }
            }
            foreach (var id in activeIDs)
            {
                if (questDatabase.TryGetValue(id, out QuestData q))
                {
                    q.status = QuestStatus.Active;
                }
            }
        }

        public void ResetAllQuests()
        {
            foreach (var quest in questDatabase.Values)
            {
                quest.status = string.IsNullOrEmpty(quest.prerequisiteQuestID) ? QuestStatus.Available : QuestStatus.Locked;
                foreach (var obj in quest.objectives)
                {
                    obj.currentCount = 0;
                }
            }
        }
    }
}
