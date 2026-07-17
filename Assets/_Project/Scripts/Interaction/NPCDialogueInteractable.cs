using UnityEngine;
using MetaEdu.Dialogue;

namespace MetaEdu.Interaction
{
    public class NPCDialogueInteractable : MonoBehaviour, IInteractable
    {
        [Header("NPC Configuration")]
        public string npcName;
        public string npcRole;
        public DialogueData dialogueData;

        [Header("Interaction Settings")]
        public string customPrompt = "Bicara dengan ";

        public string GetInteractionPrompt()
        {
            return customPrompt + npcName + " (" + npcRole + ")";
        }

        public void Interact()
        {
            if (DialogueManager.Instance != null && dialogueData != null)
            {
                DialogueManager.Instance.StartDialogue(dialogueData);
            }
        }
    }
}
