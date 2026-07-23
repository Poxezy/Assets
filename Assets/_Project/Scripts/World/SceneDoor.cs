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
    float proximityTimer;
    static SceneDoorPromptUI promptUI;
    const float ProximityRadius = 3.2f;

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
        if (!IsPlayer(other)) return;
        SetInside(true);
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsPlayer(other)) return;
        SetInside(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        SetInside(false);
    }

    void Update()
    {
        // Fallback proximity — CC trigger often miss
        proximityTimer += Time.unscaledDeltaTime;
        if (proximityTimer >= 0.15f)
        {
            proximityTimer = 0f;
            bool near = IsPlayerNearby(ProximityRadius);
            if (near != playerInside)
                SetInside(near);
        }

        if (!playerInside) return;
        if (!Input.GetKeyDown(interactKey)) return;
        TryEnter();
    }

    void SetInside(bool inside)
    {
        if (playerInside == inside) return;
        playerInside = inside;
        if (inside) ShowPrompt(BuildPrompt());
        else HidePrompt();
    }

    static bool IsPlayer(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("Player")) return true;
        if (other.GetComponent<FPSController>() != null) return true;
        if (other.GetComponentInParent<FPSController>() != null) return true;
        if (other.GetComponent<CharacterController>() != null) return true;
        if (other.GetComponentInParent<CharacterController>() != null) return true;
        return false;
    }

    bool IsPlayerNearby(float radius)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            var fps = FindAnyObjectByType<FPSController>();
            if (fps != null) player = fps.gameObject;
        }
        if (player == null) return false;
        Vector3 a = transform.position;
        Vector3 b = player.transform.position;
        a.y = b.y = 0f;
        return (a - b).sqrMagnitude <= radius * radius;
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

        if (MetaEdu.Quest.QuestManager.Instance != null)
        {
            if (string.Equals(targetScene, "classroom", System.StringComparison.OrdinalIgnoreCase))
                MetaEdu.Quest.QuestManager.Instance.NotifyEnteredClassroom();
            else if (string.Equals(targetScene, "Library", System.StringComparison.OrdinalIgnoreCase)
                     || string.Equals(targetScene, "MainScene", System.StringComparison.OrdinalIgnoreCase))
                MetaEdu.Quest.QuestManager.Instance.NotifyEnteredLibrary();
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
