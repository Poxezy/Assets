using UnityEngine;
using MetaEdu.Quiz;

namespace MetaEdu.Interaction
{
    public class QuizInteractable : MonoBehaviour, IInteractable
    {
        public QuizData quizData;
        public string promptText = "Mulai Kuis Akademik";

        public string GetInteractionPrompt()
        {
            return promptText;
        }

        public void Interact()
        {
            if (QuizManager.Instance != null && quizData != null)
            {
                QuizManager.Instance.StartQuiz(quizData);
            }
        }
    }
}
