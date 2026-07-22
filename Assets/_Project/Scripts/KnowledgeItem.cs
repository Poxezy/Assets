using MetaEdu.Quiz;
using UnityEngine;

/// <summary>
/// Book pickup: enter trigger → open quiz → on finish mark collected + destroy.
/// </summary>
public class KnowledgeItem : MonoBehaviour
{
    const string CollectedListKey = "KnowledgeCollected";
    const string DefaultItemId = "book_classroom_01";
    const float MinTriggerAxis = 0.8f;
    const int FindBonus = 15;

    [Header("Informasi Item")]
    [SerializeField] private string itemId = DefaultItemId;
    [SerializeField] private string itemName = "Programming Book";
    [SerializeField] private int scoreValue = 50;

    [Header("Penyimpanan")]
    [SerializeField] private bool collectOnlyOnce = true;

    private bool hasBeenCollected;
    private bool quizPending;
    private bool playerInside;
    private float retryTimer;

    public void Configure(string id, string displayName, int points, bool once = true)
    {
        itemId = id;
        itemName = displayName;
        scoreValue = points;
        collectOnlyOnce = once;
    }

    public static void ClearAllCollected()
    {
        PlayerPrefs.DeleteKey(CollectedListKey);
        PlayerPrefs.DeleteKey("KnowledgeItem_" + DefaultItemId + "_Collected");
        PlayerPrefs.Save();
    }

    private void Awake()
    {
        EnsureUniqueId();
        EnsurePhysicsPickup();
    }

    private void Start()
    {
        QuizManager.EnsureSystems();

        if (collectOnlyOnce && IsCollected(itemId))
        {
            Debug.Log("KnowledgeItem: already collected → hide " + itemId);
            gameObject.SetActive(false);
            return;
        }

        Debug.Log("KnowledgeItem: ready '" + itemId + "' on " + gameObject.name);
    }

    private void OnDestroy()
    {
        UnbindQuiz();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerInside = true;
        Debug.Log("KnowledgeItem: player enter → " + itemId);
        TryStartQuiz();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerInside = true;
        if (!hasBeenCollected && !quizPending)
            TryStartQuiz();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        playerInside = false;
    }

    private void Update()
    {
        if (hasBeenCollected || quizPending)
            return;

        retryTimer += Time.unscaledDeltaTime;
        if (retryTimer < 0.2f) return;
        retryTimer = 0f;

        // Fallback: proximity (CC trigger can miss)
        if (!playerInside)
            playerInside = IsPlayerNearby(2.2f);

        if (playerInside)
            TryStartQuiz();
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
        a.y = b.y = 0f; // horizontal distance
        return (a - b).sqrMagnitude <= radius * radius;
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

    void TryStartQuiz()
    {
        if (hasBeenCollected || quizPending)
            return;

        QuizManager.EnsureSystems();
        if (QuizManager.Instance == null)
        {
            Debug.LogError("KnowledgeItem: QuizManager null.");
            return;
        }

        if (QuizManager.Instance.IsRunning)
        {
            // Another quiz open — wait
            return;
        }

        var quiz = BookQuizBank.CreateForItem(itemId);
        if (quiz == null || quiz.questions == null || quiz.questions.Count == 0)
        {
            Debug.LogError("KnowledgeItem: quiz bank empty for " + itemId);
            return;
        }

        quizPending = true;
        QuizManager.Instance.OnQuizFinished += OnQuizDone;
        Debug.Log("KnowledgeItem: starting quiz for " + itemId);
        QuizManager.Instance.StartQuiz(quiz);
    }

    void OnQuizDone(int percent, int correct, int total)
    {
        Debug.Log("KnowledgeItem: quiz done " + percent + "% → collect " + itemId);
        UnbindQuiz();
        CompleteCollect();
    }

    void UnbindQuiz()
    {
        if (QuizManager.Instance != null)
            QuizManager.Instance.OnQuizFinished -= OnQuizDone;
        quizPending = false;
    }

    void CompleteCollect()
    {
        if (hasBeenCollected) return;
        hasBeenCollected = true;
        playerInside = false;

        if (ScoreManager.Instance != null && FindBonus > 0)
            ScoreManager.Instance.AddScore(FindBonus);

        if (collectOnlyOnce)
            MarkCollected(itemId);

        // Quest progress: quiz finished (single objective id — no double-count)
        if (MetaEdu.Quest.QuestManager.Instance != null)
            MetaEdu.Quest.QuestManager.Instance.NotifyBookQuizFinished();

        Destroy(gameObject);
    }

    void EnsureUniqueId()
    {
        if (!string.IsNullOrEmpty(itemId) && itemId != DefaultItemId)
            return;

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        itemId = scene + "_" + Sanitize(gameObject.name);
    }

    /// <summary>
    /// Trigger + kinematic Rigidbody so CharacterController reliably fires OnTriggerEnter.
    /// </summary>
    void EnsurePhysicsPickup()
    {
        // Rigidbody required for reliable CC ↔ trigger messages on many setups
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        var box = GetComponent<BoxCollider>();
        if (box != null && box.isTrigger)
        {
            if (NeedsBiggerTrigger(box))
                FitTrigger(box);
            return;
        }

        // Prefer existing child trigger boxes — enlarge if tiny
        var cols = GetComponentsInChildren<Collider>(true);
        BoxCollider best = null;
        for (int i = 0; i < cols.Length; i++)
        {
            if (!cols[i].isTrigger) continue;
            var b = cols[i] as BoxCollider;
            if (b != null)
            {
                best = b;
                break;
            }
        }

        if (best != null)
        {
            if (NeedsBiggerTrigger(best))
                FitTrigger(best);
            return;
        }

        // Convert first solid box on self to trigger if small prop
        box = GetComponent<BoxCollider>();
        if (box != null)
        {
            box.isTrigger = true;
            FitTrigger(box);
            return;
        }

        box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        FitTrigger(box);
    }

    static bool NeedsBiggerTrigger(BoxCollider box)
    {
        Vector3 s = box.size;
        Vector3 lossy = box.transform.lossyScale;
        float wx = Mathf.Abs(s.x * lossy.x);
        float wy = Mathf.Abs(s.y * lossy.y);
        float wz = Mathf.Abs(s.z * lossy.z);
        return Mathf.Max(wx, wy, wz) < MinTriggerAxis;
    }

    static void FitTrigger(BoxCollider box)
    {
        var go = box.gameObject;
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);

            Vector3 localCenter = go.transform.InverseTransformPoint(b.center);
            Vector3 localSize = go.transform.InverseTransformVector(b.size);
            localSize = new Vector3(
                Mathf.Abs(localSize.x) * 1.5f + 0.6f,
                Mathf.Abs(localSize.y) * 1.5f + 0.6f,
                Mathf.Abs(localSize.z) * 1.5f + 0.6f);
            localSize = Vector3.Max(localSize, new Vector3(1.5f, 1.5f, 1.5f));
            box.center = localCenter;
            box.size = localSize;
        }
        else
        {
            box.center = new Vector3(0f, 0.5f, 0f);
            box.size = new Vector3(1.5f, 1.5f, 1.5f);
        }
        box.isTrigger = true;
    }

    static bool IsCollected(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        if (PlayerPrefs.GetInt("KnowledgeItem_" + id + "_Collected", 0) == 1)
            return true;

        string list = PlayerPrefs.GetString(CollectedListKey, "");
        return IsInList(list, id);
    }

    static void MarkCollected(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;

        string list = PlayerPrefs.GetString(CollectedListKey, "");
        if (IsInList(list, id))
        {
            PlayerPrefs.Save();
            return;
        }

        list = string.IsNullOrEmpty(list) ? id : list + "," + id;
        PlayerPrefs.SetString(CollectedListKey, list);
        PlayerPrefs.Save();
    }

    static bool IsInList(string list, string id)
    {
        if (string.IsNullOrEmpty(list)) return false;
        int start = 0;
        while (start <= list.Length)
        {
            int comma = list.IndexOf(',', start);
            if (comma < 0) comma = list.Length;
            if (comma > start && string.CompareOrdinal(list, start, id, 0, id.Length) == 0
                && comma - start == id.Length)
                return true;
            start = comma + 1;
            if (comma >= list.Length) break;
        }
        return false;
    }

    static string Sanitize(string name)
    {
        char[] chars = name.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (!(char.IsLetterOrDigit(c) || c == '_'))
                chars[i] = '_';
        }
        return new string(chars);
    }
}
