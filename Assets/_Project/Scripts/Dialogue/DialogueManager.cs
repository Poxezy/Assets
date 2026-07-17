using System.Collections.Generic;
using UnityEngine;

namespace MetaEdu.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        private DialogueData currentDialogue;
        private int currentNodeIndex = 0;

        public System.Action<DialogueNode> OnDialogueStarted;
        public System.Action<DialogueNode> OnDialogueUpdated;
        public System.Action OnDialogueEnded;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartDialogue(DialogueData data)
        {
            currentDialogue = data;
            currentNodeIndex = 0;
            if (currentDialogue != null && currentDialogue.nodes.Count > 0)
            {
                OnDialogueStarted?.Invoke(currentDialogue.nodes[currentNodeIndex]);
            }
        }

        public void SelectOption(int optionIndex)
        {
            if (currentDialogue == null || currentNodeIndex >= currentDialogue.nodes.Count) return;

            var node = currentDialogue.nodes[currentNodeIndex];
            if (optionIndex < 0 || optionIndex >= node.options.Count) return;

            var option = node.options[optionIndex];

            // Trigger Quest Event jika dikonfigurasi
            if (!string.IsNullOrEmpty(option.triggerQuestActivationID))
            {
                Quest.QuestManager.Instance?.ActivateQuest(option.triggerQuestActivationID);
            }
            if (!string.IsNullOrEmpty(option.triggerQuestCompletionID))
            {
                Quest.QuestManager.Instance?.CompleteQuest(option.triggerQuestCompletionID);
            }

            int next = option.nextNodeIndex;
            if (next == -1 || next >= currentDialogue.nodes.Count)
            {
                EndDialogue();
            }
            else
            {
                currentNodeIndex = next;
                OnDialogueUpdated?.Invoke(currentDialogue.nodes[currentNodeIndex]);
            }
        }

        public void EndDialogue()
        {
            currentDialogue = null;
            OnDialogueEnded?.Invoke();
        }
    }
}
