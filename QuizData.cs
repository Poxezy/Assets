using UnityEngine;
namespace MetaEdu.Quiz
{
    [System.Serializable]
    public class QuizQuestion
    {
        public string questionID;
        public string category;
        public string difficulty;
        [TextArea(3, 5)]
        public string questionText;
        public string[] answerOptions = new string[4];
        public int correctAnswerIndex;
        [TextArea(2, 4)]
        public string explanation;
        public int scoreValue = 20;
    }
    [CreateAssetMenu(fileName = "NewQuiz", menuName = "MetaEdu/Quiz", order = 2)]
    public class QuizData : ScriptableObject
    {
        public string quizID;
        public string quizTitle;
        public System.Collections.Generic.List<QuizQuestion> questions = new System.Collections.Generic.List<QuizQuestion>();
    }
}
