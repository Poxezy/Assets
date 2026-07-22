using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger door: stand in zone, press E to load target scene.
/// Optional area unlock check via ScoreManager.
/// </summary>
public class SceneDoor : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] string targetScene = "classroom";
    [SerializeField] string displayName = "Classroom";

    public string TargetScene => targetScene;
    public string DisplayName => displayName;

    [Header("Unlock (optional)")]
    [SerializeField] bool requireUnlock;
    [SerializeField] string areaName = "Classroom";
    [SerializeField] int requiredLevel = 1;

    [Header("Prompt")]
    [SerializeField] KeyCode interactKey = KeyCode.E;

    bool playerInside;
    static SceneDoorPromptUI promptUI;

    public void Configure(
        string scene,
        string label,
        bool unlockRequired,
        string unlockArea,
        int levelNeeded)
    {
        targetScene = scene;
        displayName = label;
        requireUnlock = unlockRequired;
        areaName = unlockArea;
        requiredLevel = levelNeeded;
    }

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2.5f, 3f, 2.5f);
            box.center = new Vector3(0f, 1.5f, 0f);
        }
        else
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        ShowPrompt(BuildPrompt());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        HidePrompt();
    }

    void Update()
    {
        if (!playerInside) return;
        if (!Input.GetKeyDown(interactKey)) return;
        TryEnter();
    }

    void TryEnter()
    {
        if (requireUnlock)
        {
            if (ScoreManager.Instance == null)
            {
                Debug.LogError("ScoreManager missing — cannot check unlock.");
                return;
            }

            if (!ScoreManager.Instance.IsAreaUnlocked(areaName))
            {
                ShowPrompt(displayName + " terkunci · Level " + requiredLevel);
                return;
            }
        }

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("SceneDoor: targetScene empty");
            return;
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (string.Equals(targetScene, "classroom", System.StringComparison.OrdinalIgnoreCase)
            && MetaEdu.Quest.QuestManager.Instance != null)
        {
            MetaEdu.Quest.QuestManager.Instance.NotifyEnteredClassroom();
        }

        SceneManager.LoadScene(targetScene);
    }

    string BuildPrompt()
    {
        string label = displayName ?? "";
        if (label.IndexOf("KELUAR", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return "[E] " + label;
        return "[E] Masuk " + label;
    }

    void ShowPrompt(string msg)
    {
        EnsurePromptUI();
        promptUI.Show(msg);
    }

    void HidePrompt()
    {
        if (promptUI != null)
            promptUI.Hide();
    }

    static void EnsurePromptUI()
    {
        if (promptUI != null) return;
        var go = new GameObject("SceneDoorPromptUI");
        promptUI = go.AddComponent<SceneDoorPromptUI>();
        DontDestroyOnLoad(go);
    }

    void OnDisable()
    {
        if (playerInside)
        {
            playerInside = false;
            HidePrompt();
        }
    }
}
