using System.Collections.Generic;
using UnityEngine;

namespace MetaEdu.MiniGames
{
    public enum MiniGameType
    {
        AlgorithmSequence,
        NetworkBuilder,
        HardwareID
    }

    public class MiniGameManager : MonoBehaviour
    {
        public static MiniGameManager Instance { get; private set; }

        public System.Action<MiniGameType> OnMiniGameStarted;
        public System.Action<MiniGameType, int, bool> OnMiniGameFinished; // Type, score, isSuccess

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

        public void StartMiniGame(MiniGameType gameType)
        {
            OnMiniGameStarted?.Invoke(gameType);
        }

        public void FinishMiniGame(MiniGameType gameType, int score, bool isSuccess)
        {
            if (isSuccess && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(score);
                ScoreManager.Instance.AddXP(100);

                // Menyesuaikan reward badge sesuai tipe mini game
                switch (gameType)
                {
                    case MiniGameType.AlgorithmSequence:
                        ScoreManager.Instance.UnlockBadge("Programming Beginner");
                        break;
                    case MiniGameType.NetworkBuilder:
                        ScoreManager.Instance.UnlockBadge("Network Builder");
                        break;
                    case MiniGameType.HardwareID:
                        ScoreManager.Instance.UnlockBadge("Hardware Specialist");
                        break;
                }
            }

            OnMiniGameFinished?.Invoke(gameType, score, isSuccess);
        }
    }
}
