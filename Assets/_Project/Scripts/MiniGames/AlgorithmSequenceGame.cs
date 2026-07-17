using System.Collections.Generic;
using UnityEngine;

namespace MetaEdu.MiniGames
{
    public class AlgorithmSequenceGame : MonoBehaviour
    {
        [System.Serializable]
        public class AlgorithmStep
        {
            public string stepText;
            public int correctOrderIndex;
        }

        public List<AlgorithmStep> steps = new List<AlgorithmStep>();
        private List<AlgorithmStep> playerCurrentSequence = new List<AlgorithmStep>();

        public void StartGame()
        {
            playerCurrentSequence.Clear();
            // Melakukan kloning list dan melakukan shuffle sederhana
            var shuffledSteps = new List<AlgorithmStep>(steps);
            for (int i = 0; i < shuffledSteps.Count; i++)
            {
                var temp = shuffledSteps[i];
                int randomIndex = Random.Range(i, shuffledSteps.Count);
                shuffledSteps[i] = shuffledSteps[randomIndex];
                shuffledSteps[randomIndex] = temp;
            }
            // Kirim event list acak ke UI controller untuk di drag-and-drop
        }

        public void SubmitSequence(List<int> stepIndicesInCurrentOrder)
        {
            bool isCorrect = true;
            for (int i = 0; i < stepIndicesInCurrentOrder.Count; i++)
            {
                int stepIndex = stepIndicesInCurrentOrder[i];
                if (steps[stepIndex].correctOrderIndex != i)
                {
                    isCorrect = false;
                    break;
                }
            }

            int finalScore = isCorrect ? 100 : 0;
            MiniGameManager.Instance.FinishMiniGame(MiniGameType.AlgorithmSequence, finalScore, isCorrect);
        }
    }
}
