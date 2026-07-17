using UnityEngine;

namespace MetaEdu.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Player Settings")]
        public string playerName = "Mahasiswa Baru";
        public string selectedAvatar = "DefaultMale";
        
        [Header("State Variables")]
        public bool isGamePaused = false;

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

        public void PauseGame(bool pause)
        {
            isGamePaused = pause;
            Time.timeScale = pause ? 0f : 1f;
            Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = pause;
        }
    }
}
