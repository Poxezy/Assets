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
        /// <summary>Stable id for progress reports (not display text).</summary>
        public string objectiveId;
        public string description;
        public string hintText;
        /// <summary>Book | ClassroomDoor | empty</summary>
        public string targetTag;
        public int currentCount;
        public int requiredCount = 1;
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
