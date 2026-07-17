using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetaEdu.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public System.Action<float> OnLoadProgressChanged;
        public System.Action OnLoadCompleted;

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

        public void LoadSceneAsync(string sceneName)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        private System.Collections.IEnumerator LoadSceneCoroutine(string sceneName)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (!op.isDone)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f);
                OnLoadProgressChanged?.Invoke(progress);

                if (op.progress >= 0.9f)
                {
                    op.allowSceneActivation = true;
                }

                yield return null;
            }

            OnLoadCompleted?.Invoke();
        }
    }
}
