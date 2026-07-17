using UnityEngine;
using MetaEdu.MiniGames;

namespace MetaEdu.Interaction
{
    public class MiniGameInteractable : MonoBehaviour, IInteractable
    {
        public MiniGameType miniGameType;
        public string promptText = "Mainkan Mini-Game";

        public string GetInteractionPrompt()
        {
            return promptText;
        }

        public void Interact()
        {
            if (MiniGameManager.Instance != null)
            {
                MiniGameManager.Instance.StartMiniGame(miniGameType);
            }
        }
    }
}
