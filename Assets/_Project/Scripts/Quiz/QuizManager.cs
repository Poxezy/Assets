using UnityEngine;

namespace MetaEdu.Quiz
{
    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        private QuizData currentQuiz;
        private int currentQuestionIndex;
        private int correctAnswersCount;
        private bool answerLocked;

        public System.Action<QuizQuestion, int, int> OnQuestionLoaded;
        public System.Action<bool, string> OnAnswerFeedback;
        public System.Action<int, int, int> OnQuizFinished;

        public bool IsRunning => currentQuiz != null;
        public string CurrentTitle => currentQuiz != null ? currentQuiz.quizTitle : null;

        // Boot via GameplaySceneSetup on gameplay scenes (not RuntimeInitialize)

        public static void EnsureSystems()
        {
            if (Instance != null)
            {
                if (Instance.GetComponent<QuizUI>() == null)
                    Instance.gameObject.AddComponent<QuizUI>();
                return;
            }

            var existing = Object.FindAnyObjectByType<QuizManager>();
            if (existing != null)
            {
                Instance = existing;
                if (existing.GetComponent<QuizUI>() == null)
                    existing.gameObject.AddComponent<QuizUI>();
                return;
            }

            var go = new GameObject("QuizSystems");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<QuizManager>();
            go.AddComponent<QuizUI>();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                if (GetComponent<QuizUI>() == null)
                    gameObject.AddComponent<QuizUI>();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void StartQuiz(QuizData data)
        {
            if (data == null || data.questions == null || data.questions.Count == 0)
            {
                Debug.LogWarning("QuizManager: empty quiz.");
                return;
            }

            // Stuck quiz blocks all books — force clear
            if (IsRunning)
            {
                Debug.LogWarning("QuizManager: previous quiz still open — force close.");
                ForceAbort();
            }

            EnsureSystems();
            var ui = GetComponent<QuizUI>();
            if (ui != null)
                ui.PrepareForQuiz();

            currentQuiz = data;
            currentQuestionIndex = 0;
            correctAnswersCount = 0;
            answerLocked = false;

            Debug.Log("QuizManager: start '" + data.quizTitle + "' (" + data.questions.Count + " soal)");
            LoadQuestion();
        }

        /// <summary>Abort without finish rewards (used when stuck).</summary>
        public void ForceAbort()
        {
            if (currentQuiz == null)
            {
                var quietUi = GetComponent<QuizUI>();
                if (quietUi != null) quietUi.ForceClose();
                return;
            }

            var old = currentQuiz;
            currentQuiz = null;
            answerLocked = false;
            // total=0 → KnowledgeItem treats as cancel, not collect
            OnQuizFinished?.Invoke(0, 0, 0);
            if (old != null)
                Destroy(old);

            var ui = GetComponent<QuizUI>();
            if (ui != null) ui.ForceClose();
        }

        private void LoadQuestion()
        {
            if (currentQuiz == null || currentQuestionIndex >= currentQuiz.questions.Count)
                return;

            answerLocked = false;
            var q = currentQuiz.questions[currentQuestionIndex];
            OnQuestionLoaded?.Invoke(q, currentQuestionIndex, currentQuiz.questions.Count);
        }

        public void SubmitAnswer(int optionIndex)
        {
            if (!IsRunning || answerLocked) return;
            if (currentQuestionIndex >= currentQuiz.questions.Count) return;

            answerLocked = true;
            var q = currentQuiz.questions[currentQuestionIndex];
            bool isCorrect = optionIndex == q.correctAnswerIndex;
            if (isCorrect)
            {
                correctAnswersCount++;
                if (ScoreManager.Instance != null && q.scoreValue > 0)
                    ScoreManager.Instance.AddScore(q.scoreValue);
            }

            OnAnswerFeedback?.Invoke(isCorrect, q.explanation);
        }

        public void NextQuestion()
        {
            if (!IsRunning) return;

            currentQuestionIndex++;
            if (currentQuestionIndex < currentQuiz.questions.Count)
                LoadQuestion();
            else
                FinishQuiz();
        }

        private void FinishQuiz()
        {
            int totalQuestions = currentQuiz.questions.Count;
            int score = totalQuestions > 0
                ? Mathf.RoundToInt((correctAnswersCount / (float)totalQuestions) * 100f)
                : 0;

            if (score >= 70 && ScoreManager.Instance != null)
                ScoreManager.Instance.AddXP(150);

            var finished = currentQuiz;
            currentQuiz = null;
            answerLocked = false;

            OnQuizFinished?.Invoke(score, correctAnswersCount, totalQuestions);

            if (finished != null)
                Destroy(finished);
        }
    }
}
