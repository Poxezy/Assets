using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetaEdu.Quest
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        const string PrefPrefix = "MetaEdu.Quest.";

        public List<QuestData> allQuests = new List<QuestData>();

        readonly Dictionary<string, QuestData> questDatabase = new Dictionary<string, QuestData>();
        readonly List<string> seedOrder = new List<string>();

        public System.Action<QuestData> OnQuestActivated;
        public System.Action<QuestData> OnQuestCompleted;
        public System.Action<QuestData> OnQuestUpdated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            string scene = SceneManager.GetActiveScene().name;
            if (scene == "MainMenu" || scene == "Leaderboard") return;
            EnsureSystems();
        }

        public static void EnsureSystems()
        {
            if (Instance != null)
            {
                EnsureComponents(Instance.gameObject);
                Instance.GetComponent<QuestUI>()?.Prepare();
                return;
            }

            var existing = Object.FindAnyObjectByType<QuestManager>();
            if (existing != null)
            {
                Instance = existing;
                EnsureComponents(existing.gameObject);
                existing.GetComponent<QuestUI>()?.Prepare();
                return;
            }

            var go = new GameObject("QuestSystems");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<QuestManager>();
            EnsureComponents(go);
        }

        static void EnsureComponents(GameObject go)
        {
            if (go.GetComponent<QuestUI>() == null)
                go.AddComponent<QuestUI>();
            if (go.GetComponent<QuestWaypointService>() == null)
                go.AddComponent<QuestWaypointService>();
            if (go.GetComponent<QuestCompassUI>() == null)
                go.AddComponent<QuestCompassUI>();
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SeedDefaultQuestsIfEmpty();
                InitializeDatabase();
                LoadProgressFromPrefs();
                EnsureComponents(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // Auto-start first available intro quest
            foreach (var q in questDatabase.Values)
            {
                if (q != null && q.status == QuestStatus.Available
                    && string.IsNullOrEmpty(q.prerequisiteQuestID))
                {
                    ActivateQuest(q.questID);
                    break;
                }
            }

            GetComponent<QuestUI>()?.Prepare();
            GetComponent<QuestWaypointService>()?.Refresh();
        }

        void SeedDefaultQuestsIfEmpty()
        {
            if (allQuests != null && allQuests.Count > 0) return;

            allQuests = new List<QuestData>
            {
                MakeQuest(
                    "intro_campus",
                    "Jelajahi Campus Yard",
                    "Pengenalan kampus. Cari buku pengetahuan yang bersinar, selesaikan kuisnya.",
                    "",
                    QuestStatus.Available,
                    80,
                    "",
                    Obj("finish_book_quiz", "Selesaikan kuis 1 buku", "Cari buku bersinar di Campus Yard", "Book", 1)),

                MakeQuest(
                    "collect_books",
                    "Kolektor Pengetahuan",
                    "Kuasai materi: selesaikan kuis dari 3 buku berbeda di area kampus.",
                    "intro_campus",
                    QuestStatus.Locked,
                    150,
                    "knowledge_explorer",
                    Obj("finish_book_quiz", "Selesaikan kuis buku", "Cari buku bersinar berikutnya", "Book", 3)),

                MakeQuest(
                    "visit_classroom",
                    "Masuk Classroom",
                    "Temukan pintu Classroom (beacon emas) di Campus Yard, tekan E, masuk kelas.",
                    "intro_campus",
                    QuestStatus.Locked,
                    100,
                    "",
                    Obj("enter_classroom", "Masuk ke Classroom", "Ikuti kompas ke pintu Classroom, tekan E", "ClassroomDoor", 1)),
            };
        }

        static QuestData MakeQuest(
            string id, string title, string desc, string prereq,
            QuestStatus status, int xp, string badge, params QuestObjective[] objs)
        {
            var q = ScriptableObject.CreateInstance<QuestData>();
            q.questID = id;
            q.questTitle = title;
            q.description = desc;
            q.prerequisiteQuestID = prereq ?? "";
            q.status = status;
            q.xpReward = xp;
            q.badgeReward = badge ?? "";
            q.objectives = new List<QuestObjective>(objs);
            return q;
        }

        static QuestObjective Obj(string id, string desc, string hint, string targetTag, int required)
        {
            return new QuestObjective
            {
                objectiveId = id,
                description = desc,
                hintText = hint ?? "",
                targetTag = targetTag ?? "",
                currentCount = 0,
                requiredCount = Mathf.Max(1, required)
            };
        }

        void InitializeDatabase()
        {
            questDatabase.Clear();
            seedOrder.Clear();
            if (allQuests == null) return;
            foreach (var quest in allQuests)
            {
                if (quest != null && !string.IsNullOrEmpty(quest.questID)
                    && !questDatabase.ContainsKey(quest.questID))
                {
                    questDatabase.Add(quest.questID, quest);
                    seedOrder.Add(quest.questID);
                }
            }
        }

        public void ActivateQuest(string questID)
        {
            if (!questDatabase.TryGetValue(questID, out QuestData quest))
                return;

            if (quest.status == QuestStatus.Available || quest.status == QuestStatus.Locked)
            {
                if (quest.status == QuestStatus.Locked
                    && !string.IsNullOrEmpty(quest.prerequisiteQuestID))
                {
                    if (questDatabase.TryGetValue(quest.prerequisiteQuestID, out var pre)
                        && pre.status != QuestStatus.Completed)
                        return;
                }

                quest.status = QuestStatus.Active;
                SaveProgressToPrefs();
                OnQuestActivated?.Invoke(quest);
            }
        }

        public void CompleteQuest(string questID)
        {
            if (!questDatabase.TryGetValue(questID, out QuestData quest))
                return;

            if (quest.status != QuestStatus.Active)
                return;

            quest.status = QuestStatus.Completed;

            if (ScoreManager.Instance != null)
            {
                if (quest.xpReward > 0)
                    ScoreManager.Instance.AddXP(quest.xpReward);
                if (!string.IsNullOrEmpty(quest.badgeReward))
                    ScoreManager.Instance.UnlockBadge(quest.badgeReward);
            }

            SaveProgressToPrefs();
            OnQuestCompleted?.Invoke(quest);
            UnlockNextQuests(questID);
        }

        /// <summary>Progress first matching incomplete objective on active quests by objectiveId.</summary>
        public void ReportObjective(string objectiveId, int amount = 1)
        {
            if (string.IsNullOrEmpty(objectiveId) || amount <= 0) return;

            // Prefer seed order so intro advances before multi-count quest when both match
            foreach (string id in EnumerateActiveOrdered())
            {
                if (!questDatabase.TryGetValue(id, out var quest) || quest == null) continue;
                if (quest.objectives == null) continue;

                bool changed = false;
                for (int i = 0; i < quest.objectives.Count; i++)
                {
                    var obj = quest.objectives[i];
                    if (obj == null || obj.isCompleted) continue;
                    if (!IdMatches(obj, objectiveId)) continue;

                    obj.currentCount = Mathf.Min(obj.requiredCount, obj.currentCount + amount);
                    changed = true;
                    break;
                }

                if (!changed) continue;

                SaveProgressToPrefs();
                OnQuestUpdated?.Invoke(quest);

                bool allDone = true;
                for (int i = 0; i < quest.objectives.Count; i++)
                {
                    if (quest.objectives[i] != null && !quest.objectives[i].isCompleted)
                    {
                        allDone = false;
                        break;
                    }
                }

                if (allDone)
                    CompleteQuest(quest.questID);

                // Only one quest receives a report tick
                return;
            }
        }

        static bool IdMatches(QuestObjective obj, string objectiveId)
        {
            if (!string.IsNullOrEmpty(obj.objectiveId)
                && string.Equals(obj.objectiveId, objectiveId, System.StringComparison.OrdinalIgnoreCase))
                return true;

            // Legacy fallback: description contains keyword
            if (!string.IsNullOrEmpty(obj.description)
                && obj.description.IndexOf(objectiveId, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        IEnumerable<string> EnumerateActiveOrdered()
        {
            for (int i = 0; i < seedOrder.Count; i++)
            {
                string id = seedOrder[i];
                if (questDatabase.TryGetValue(id, out var q) && q != null && q.status == QuestStatus.Active)
                    yield return id;
            }

            foreach (var kv in questDatabase)
            {
                if (seedOrder.Contains(kv.Key)) continue;
                if (kv.Value != null && kv.Value.status == QuestStatus.Active)
                    yield return kv.Key;
            }
        }

        public void NotifyBookCollected()
        {
            ReportObjective("collect_book", 1);
        }

        public void NotifyBookQuizFinished()
        {
            ReportObjective("finish_book_quiz", 1);
        }

        public void NotifyEnteredClassroom()
        {
            ReportObjective("enter_classroom", 1);
        }

        void UnlockNextQuests(string completedQuestID)
        {
            foreach (var quest in questDatabase.Values)
            {
                if (quest.prerequisiteQuestID == completedQuestID && quest.status == QuestStatus.Locked)
                {
                    quest.status = QuestStatus.Available;
                    ActivateQuest(quest.questID);
                }
            }
        }

        public List<string> GetCompletedQuests()
        {
            var completed = new List<string>();
            foreach (var quest in questDatabase.Values)
                if (quest.status == QuestStatus.Completed)
                    completed.Add(quest.questID);
            return completed;
        }

        public List<string> GetActiveQuests()
        {
            var active = new List<string>();
            foreach (var quest in questDatabase.Values)
                if (quest.status == QuestStatus.Active)
                    active.Add(quest.questID);
            return active;
        }

        public List<QuestData> GetActiveQuestData()
        {
            var list = new List<QuestData>();
            foreach (string id in EnumerateActiveOrdered())
            {
                if (questDatabase.TryGetValue(id, out var quest) && quest != null)
                    list.Add(quest);
            }
            return list;
        }

        /// <summary>Focus objective for compass / next-step UI.</summary>
        public bool GetFocusTarget(out QuestData quest, out QuestObjective objective)
        {
            quest = null;
            objective = null;
            foreach (string id in EnumerateActiveOrdered())
            {
                if (!questDatabase.TryGetValue(id, out var q) || q?.objectives == null) continue;
                for (int i = 0; i < q.objectives.Count; i++)
                {
                    var obj = q.objectives[i];
                    if (obj == null || obj.isCompleted) continue;
                    quest = q;
                    objective = obj;
                    return true;
                }
            }
            return false;
        }

        public string GetNextStepText()
        {
            if (!GetFocusTarget(out var q, out var obj))
                return "Tidak ada misi aktif.";
            if (!string.IsNullOrEmpty(obj.hintText))
                return obj.hintText;
            return obj.description ?? q.questTitle;
        }

        public void LoadQuestProgress(List<string> activeIDs, List<string> completedIDs)
        {
            ResetAllQuests(clearPrefs: false);
            if (completedIDs != null)
            {
                foreach (var id in completedIDs)
                {
                    if (questDatabase.TryGetValue(id, out QuestData q))
                        q.status = QuestStatus.Completed;
                }
            }
            if (activeIDs != null)
            {
                foreach (var id in activeIDs)
                {
                    if (questDatabase.TryGetValue(id, out QuestData q))
                        q.status = QuestStatus.Active;
                }
            }
            SaveProgressToPrefs();
            OnQuestUpdated?.Invoke(null);
        }

        public void ResetAllQuests()
        {
            ResetAllQuests(clearPrefs: true);
        }

        void ResetAllQuests(bool clearPrefs)
        {
            foreach (var quest in questDatabase.Values)
            {
                quest.status = string.IsNullOrEmpty(quest.prerequisiteQuestID)
                    ? QuestStatus.Available
                    : QuestStatus.Locked;
                if (quest.objectives == null) continue;
                foreach (var obj in quest.objectives)
                    if (obj != null) obj.currentCount = 0;
            }

            if (clearPrefs)
                ClearProgressPrefs();

            OnQuestUpdated?.Invoke(null);
        }

        void SaveProgressToPrefs()
        {
            var sb = new StringBuilder(128);
            foreach (var kv in questDatabase)
            {
                var q = kv.Value;
                if (q == null) continue;
                sb.Append(q.questID);
                sb.Append('=');
                sb.Append((int)q.status);
                if (q.objectives != null)
                {
                    for (int i = 0; i < q.objectives.Count; i++)
                    {
                        sb.Append('|');
                        sb.Append(q.objectives[i] != null ? q.objectives[i].currentCount : 0);
                    }
                }
                sb.Append(';');
            }
            PlayerPrefs.SetString(PrefPrefix + "State", sb.ToString());
            PlayerPrefs.Save();
        }

        void LoadProgressFromPrefs()
        {
            string raw = PlayerPrefs.GetString(PrefPrefix + "State", "");
            if (string.IsNullOrEmpty(raw)) return;

            string[] entries = raw.Split(';');
            for (int e = 0; e < entries.Length; e++)
            {
                string entry = entries[e];
                if (string.IsNullOrEmpty(entry)) continue;
                int eq = entry.IndexOf('=');
                if (eq <= 0) continue;
                string id = entry.Substring(0, eq);
                if (!questDatabase.TryGetValue(id, out var q) || q == null) continue;

                string rest = entry.Substring(eq + 1);
                string[] parts = rest.Split('|');
                if (parts.Length > 0 && int.TryParse(parts[0], out int statusInt))
                    q.status = (QuestStatus)Mathf.Clamp(statusInt, 0, 3);

                if (q.objectives != null)
                {
                    for (int i = 0; i < q.objectives.Count && i + 1 < parts.Length; i++)
                    {
                        if (q.objectives[i] == null) continue;
                        if (int.TryParse(parts[i + 1], out int count))
                            q.objectives[i].currentCount = Mathf.Max(0, count);
                    }
                }
            }
        }

        void ClearProgressPrefs()
        {
            PlayerPrefs.DeleteKey(PrefPrefix + "State");
            PlayerPrefs.Save();
        }
    }
}
