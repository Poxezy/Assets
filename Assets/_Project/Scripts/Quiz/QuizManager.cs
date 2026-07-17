using System.Collections.Generic;
using UnityEngine;

namespace MetaEdu.Quiz
{
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        private QuizData currentQuiz;
        private int currentQuestionIndex = 0;
        private int correctAnswersCount = 0;

        public System.Action<QuizQuestion, int, int> OnQuestionLoaded; // Question, index, total
        public System.Action<bool, string> OnAnswerFeedback; // isCorrect, explanation
        public System.Action<int, int, int> OnQuizFinished; // score, correct, total

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

        public void StartQuiz(QuizData data)
        {
            currentQuiz = data;
            currentQuestionIndex = 0;
            correctAnswersCount = 0;

            if (currentQuiz != null && currentQuiz.questions.Count > 0)
            {
                LoadQuestion();
            }
        }

        private void LoadQuestion()
        {
            if (currentQuiz == null || currentQuestionIndex >= currentQuiz.questions.Count) return;
            var q = currentQuiz.questions[currentQuestionIndex];
            OnQuestionLoaded?.Invoke(q, currentQuestionIndex, currentQuiz.questions.Count);
        }

        public void SubmitAnswer(int optionIndex)
        {
            if (currentQuiz == null || currentQuestionIndex >= currentQuiz.questions.Count) return;

            var q = currentQuiz.questions[currentQuestionIndex];
            bool isCorrect = (optionIndex == q.correctAnswerIndex);

            if (isCorrect)
            {
                correctAnswersCount++;
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(q.scoreValue);
                }
            }

            OnAnswerFeedback?.Invoke(isCorrect, q.explanation);
        }

        public void NextQuestion()
        {
            currentQuestionIndex++;
            if (currentQuestionIndex < currentQuiz.questions.Count)
            {
                LoadQuestion();
            }
            else
            {
                FinishQuiz();
            }
        }

        private void FinishQuiz()
        {
            int totalQuestions = currentQuiz.questions.Count;
            int score = Mathf.RoundToInt(((float)correctAnswersCount / totalQuestions) * 100f);

            // Jika kuis berhasil lolos dengan nilai >= 70, buka badge kuis terkait
            if (score >= 70 && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddXP(150);
            }

            OnQuizFinished?.Invoke(score, correctAnswersCount, totalQuestions);
            currentQuiz = null;
        }
    }
}
