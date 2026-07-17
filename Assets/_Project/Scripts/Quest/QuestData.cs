using System.Collections.Generic;
using UnityEngine;

namespace MetaEdu.Quest
{
    public enum QuestStatus
    {
        Locked,
        Available,
        Active,
        Completed
    }

    [System.Serializable]
    public class QuestObjective
    {
        public string description;
        public int currentCount;
        public int requiredCount;
        public bool isCompleted => currentCount >= requiredCount;
    }

    [CreateAssetMenu(fileName = "NewQuest", menuName = "MetaEdu/Quest", order = 1)]
    public class QuestData : ScriptableObject
    {
        public string questID;
        public string questTitle;
        [TextArea(3, 5)]
        public string description;
        
        public List<QuestObjective> objectives = new List<QuestObjective>();
        
        public int xpReward = 100;
        public string badgeReward = "";
        
        public QuestStatus status = QuestStatus.Locked;
        public string prerequisiteQuestID = "";
    }
}
