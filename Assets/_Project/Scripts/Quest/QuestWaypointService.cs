using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetaEdu.Quest
{
    /// <summary>
    /// Resolves world position for current focus objective (book / classroom door).
    /// </summary>
    public class QuestWaypointService : MonoBehaviour
    {
        public static QuestWaypointService Instance { get; private set; }

        Transform player;
        QuestWorldMarker marker;
        float refreshTimer;
        const float RefreshInterval = 0.4f;

        public Vector3? TargetPosition { get; private set; }
        public string TargetLabel { get; private set; }
        public string TargetTag { get; private set; }
        public bool HasTarget => TargetPosition.HasValue;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestUpdated += OnQuestEvt;
                QuestManager.Instance.OnQuestActivated += OnQuestEvt;
                QuestManager.Instance.OnQuestCompleted += OnQuestEvt;
            }
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestUpdated -= OnQuestEvt;
                QuestManager.Instance.OnQuestActivated -= OnQuestEvt;
                QuestManager.Instance.OnQuestCompleted -= OnQuestEvt;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (marker != null)
                Destroy(marker.gameObject);
        }

        void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            player = null;
            Refresh();
        }

        void OnQuestEvt(QuestData _)
        {
            Refresh();
        }

        void Update()
        {
            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = RefreshInterval;
                Refresh();
            }

            if (marker != null)
            {
                if (HasTarget)
                    marker.SetTarget(TargetPosition.Value);
                else
                    marker.Hide();
            }
        }

        public void Refresh()
        {
            string scene = SceneManager.GetActiveScene().name;
            if (scene == "MainMenu" || scene == "Leaderboard")
            {
                ClearTarget();
                return;
            }

            if (QuestManager.Instance == null)
            {
                ClearTarget();
                return;
            }

            EnsurePlayer();

            // Walk active objectives until one resolves to a world position
            if (!TryResolveFocus(out var label, out var tag, out var pos))
            {
                // Keep label/hint even if no pin
                if (QuestManager.Instance.GetFocusTarget(out _, out var obj) && obj != null)
                {
                    TargetTag = obj.targetTag ?? "";
                    TargetLabel = string.IsNullOrEmpty(obj.hintText) ? obj.description : obj.hintText;
                    TargetPosition = null;
                    if (marker != null) marker.Hide();
                }
                else
                {
                    ClearTarget();
                }
                return;
            }

            TargetTag = tag;
            TargetLabel = label;
            TargetPosition = pos;
            EnsureMarker();
        }

        bool TryResolveFocus(out string label, out string tag, out Vector3? pos)
        {
            label = "";
            tag = "";
            pos = null;

            var qm = QuestManager.Instance;
            if (qm == null) return false;

            var active = qm.GetActiveQuestData();
            for (int q = 0; q < active.Count; q++)
            {
                var quest = active[q];
                if (quest?.objectives == null) continue;
                for (int i = 0; i < quest.objectives.Count; i++)
                {
                    var obj = quest.objectives[i];
                    if (obj == null || obj.isCompleted) continue;

                    string t = obj.targetTag ?? "";
                    Vector3? p = ResolveTarget(t);
                    if (!p.HasValue) continue;

                    tag = t;
                    label = string.IsNullOrEmpty(obj.hintText) ? obj.description : obj.hintText;
                    pos = p;
                    return true;
                }
            }
            return false;
        }

        void ClearTarget()
        {
            TargetPosition = null;
            TargetLabel = "";
            TargetTag = "";
            if (marker != null) marker.Hide();
        }

        void EnsurePlayer()
        {
            if (player != null) return;
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) { player = go.transform; return; }
            var fps = FindAnyObjectByType<FPSController>();
            if (fps != null) player = fps.transform;
        }

        Vector3? ResolveTarget(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return null;

            if (string.Equals(tag, "Book", System.StringComparison.OrdinalIgnoreCase))
                return FindNearestBook();

            if (string.Equals(tag, "ClassroomDoor", System.StringComparison.OrdinalIgnoreCase))
                return FindClassroomDoor();

            if (string.Equals(tag, "MainSceneDoor", System.StringComparison.OrdinalIgnoreCase))
                return FindDoorToScene("MainScene", "mainscene", "main_scene", "Door_Main");

            return null;
        }

        Vector3? FindNearestBook()
        {
            var books = FindObjectsByType<KnowledgeItem>();
            if (books == null || books.Length == 0) return null;

            Vector3 origin = player != null ? player.position : Vector3.zero;
            float best = float.MaxValue;
            Vector3? bestPos = null;
            for (int i = 0; i < books.Length; i++)
            {
                if (books[i] == null || !books[i].gameObject.activeInHierarchy) continue;
                float d = (books[i].transform.position - origin).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    bestPos = books[i].transform.position + Vector3.up * 1.2f;
                }
            }
            return bestPos;
        }

        Vector3? FindClassroomDoor()
        {
            string scene = SceneManager.GetActiveScene().name;

            // Inside classroom: guide to exit door back to campus
            if (string.Equals(scene, "classroom", System.StringComparison.OrdinalIgnoreCase))
            {
                var exit = FindDoorToScene("campusyard", "Door_Return", "return", "keluar");
                if (exit.HasValue) return exit;
            }

            // Campus / elsewhere: door into classroom
            return FindDoorToScene("classroom", "classroom", "Door_Classroom");
        }

        /// <summary>First SceneDoor matching target scene, else name keywords.</summary>
        Vector3? FindDoorToScene(string targetScene, params string[] nameHints)
        {
            var doors = FindObjectsByType<SceneDoor>();
            if (doors == null || doors.Length == 0) return null;

            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] == null) continue;
                if (string.Equals(doors[i].TargetScene, targetScene, System.StringComparison.OrdinalIgnoreCase))
                    return doors[i].transform.position + Vector3.up * 2f;
            }

            if (nameHints == null || nameHints.Length == 0) return null;

            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] == null) continue;
                string n = doors[i].gameObject.name;
                for (int h = 0; h < nameHints.Length; h++)
                {
                    if (string.IsNullOrEmpty(nameHints[h])) continue;
                    if (n.IndexOf(nameHints[h], System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return doors[i].transform.position + Vector3.up * 2f;
                }
            }

            return null;
        }

        void EnsureMarker()
        {
            if (marker != null) return;
            var go = new GameObject("QuestWorldMarker");
            marker = go.AddComponent<QuestWorldMarker>();
        }

        public float GetDistanceToTarget()
        {
            if (!HasTarget) return -1f;
            EnsurePlayer();
            if (player == null) return -1f;
            Vector3 a = player.position;
            Vector3 b = TargetPosition.Value;
            a.y = b.y = 0f;
            return Vector3.Distance(a, b);
        }

        public float GetYawToTarget()
        {
            if (!HasTarget) return 0f;
            EnsurePlayer();
            Transform cam = Camera.main != null ? Camera.main.transform : player;
            if (cam == null) return 0f;

            Vector3 to = TargetPosition.Value - cam.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.001f) return 0f;

            Vector3 flatFwd = cam.forward;
            flatFwd.y = 0f;
            if (flatFwd.sqrMagnitude < 0.001f) flatFwd = Vector3.forward;
            flatFwd.Normalize();
            to.Normalize();

            return Vector3.SignedAngle(flatFwd, to, Vector3.up);
        }
    }
}
