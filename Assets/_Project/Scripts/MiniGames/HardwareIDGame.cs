using System.Collections.Generic;
using UnityEngine;

namespace MetaEdu.MiniGames
{
    public class HardwareIDGame : MonoBehaviour
    {
        [System.Serializable]
        public class HardwareComponent
        {
            public string componentName;
            public string description;
            public string visualReferenceTag;
        }

        public List<HardwareComponent> components = new List<HardwareComponent>();
        private int currentComponentIndex = 0;
        private int correctAnswers = 0;

        public System.Action<HardwareComponent> OnComponentPresented;

        public void StartGame()
        {
            currentComponentIndex = 0;
            correctAnswers = 0;
            PresentComponent();
        }

        private void PresentComponent()
        {
            if (currentComponentIndex < components.Count)
            {
                OnComponentPresented?.Invoke(components[currentComponentIndex]);
            }
            else
            {
                FinishGame();
            }
        }

        public void AnswerComponent(string selectedName)
        {
            if (components[currentComponentIndex].componentName == selectedName)
            {
                correctAnswers++;
            }

            currentComponentIndex++;
            PresentComponent();
        }

        private void FinishGame()
        {
            int score = Mathf.RoundToInt(((float)correctAnswers / components.Count) * 100f);
            bool isSuccess = score >= 70;
            MiniGameManager.Instance.FinishMiniGame(MiniGameType.HardwareID, score, isSuccess);
        }
    }
}
