using System.Collections.Generic;
using UnityEngine;

namespace MetaEdu.Dialogue
{
    [System.Serializable]
    public class DialogueNode
    {
        public string speakerName;
        public Sprite speakerPortrait;
        [TextArea(3, 5)]
        public string dialogueText;
        public List<DialogueOption> options = new List<DialogueOption>();
    }

    [System.Serializable]
    public class DialogueOption
    {
        public string optionText;
        public int nextNodeIndex = -1; // -1 untuk mengakhiri dialog
        public string triggerQuestActivationID = "";
        public string triggerQuestCompletionID = "";
    }

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "MetaEdu/Dialogue", order = 3)]
    public class DialogueData : ScriptableObject
    {
        public string dialogueID;
        public List<DialogueNode> nodes = new List<DialogueNode>();
    }
}
