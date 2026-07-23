using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MetaEdu.Quiz
{
    /// <summary>
    /// Persistent quiz panel on its own DontDestroy canvas.
    /// </summary>
    public class QuizUI : MonoBehaviour
    {
        public static QuizUI Instance { get; private set; }

        GameObject root;
        Canvas canvas;
        TMP_Text titleText;
        TMP_Text progressText;
        TMP_Text questionText;
        TMP_Text feedbackText;
        Button[] optionButtons = new Button[4];
        TMP_Text[] optionLabels = new TMP_Text[4];
        Button nextButton;
        TMP_Text nextLabel;
        bool bound;
        bool waitingNext;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnEnable()
        {
            // Only wire quiz events — do not force UI open on menu scenes
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (scene == "MainMenu" || scene == "Leaderboard")
            {
                ForceClose();
                return;
            }
            PrepareForQuiz();
        }

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
        {
            if (s.name == "MainMenu" || s.name == "Leaderboard")
                ForceClose();
        }

        void OnDisable()
        {
            Unbind();
        }

        void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
            Unbind();
        }

        /// <summary>Call before StartQuiz so events are subscribed.</summary>
        public void PrepareForQuiz()
        {
            EnsureEventSystem();
            EnsureUI();
            Bind();
        }

        void Bind()
        {
            var qm = QuizManager.Instance;
            if (qm == null) return;

            if (bound) return;
            qm.OnQuestionLoaded += HandleQuestion;
            qm.OnAnswerFeedback += HandleFeedback;
            qm.OnQuizFinished += HandleFinished;
            bound = true;
        }

        void Unbind()
        {
            if (!bound) return;
            if (QuizManager.Instance != null)
            {
                QuizManager.Instance.OnQuestionLoaded -= HandleQuestion;
                QuizManager.Instance.OnAnswerFeedback -= HandleFeedback;
                QuizManager.Instance.OnQuizFinished -= HandleFinished;
            }
            bound = false;
        }

        void HandleQuestion(QuizQuestion q, int index, int total)
        {
            EnsureUI();
            waitingNext = false;
            if (root != null)
            {
                root.SetActive(true);
                root.transform.SetAsLastSibling();
                if (canvas != null)
                {
                    canvas.enabled = true;
                    canvas.sortingOrder = 910;
                    canvas.overrideSorting = true;
                }
                var ray = canvas != null ? canvas.GetComponent<GraphicRaycaster>() : null;
                if (ray != null) ray.enabled = true;

                var cg = root.GetComponent<CanvasGroup>();
                if (cg == null) cg = root.AddComponent<CanvasGroup>();
                // Visible + clickable immediately (fade was leaving alpha 0 / dead clicks)
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                cg.ignoreParentGroups = true;
                ExclusiveMenuUI.ForceAllButtons(root.transform);
                var card = root.transform.Find("QuizCard") as RectTransform;
                if (card != null) UIMotion.PopIn(card, 0.15f);
            }

            Time.timeScale = 1f;
            UnlockCursor();

            if (titleText != null)
            {
                string t = QuizManager.Instance != null ? QuizManager.Instance.CurrentTitle : null;
                titleText.text = string.IsNullOrEmpty(t) ? "KUIS BUKU" : t;
            }

            if (progressText != null)
                progressText.text = "SOAL  " + (index + 1) + " / " + total;

            if (questionText != null)
                questionText.text = q != null ? (q.questionText ?? "") : "";

            if (feedbackText != null)
            {
                feedbackText.text = "";
                feedbackText.gameObject.SetActive(false);
            }

            if (nextButton != null)
                nextButton.gameObject.SetActive(false);

            for (int i = 0; i < 4; i++)
            {
                bool has = q != null && q.answerOptions != null && i < q.answerOptions.Length
                           && !string.IsNullOrEmpty(q.answerOptions[i]);
                if (optionButtons[i] == null) continue;
                optionButtons[i].gameObject.SetActive(has);
                optionButtons[i].interactable = has;
                if (has && optionLabels[i] != null)
                    optionLabels[i].text = q.answerOptions[i];
            }
        }

        void HandleFeedback(bool correct, string explanation)
        {
            waitingNext = true;
            for (int i = 0; i < optionButtons.Length; i++)
                if (optionButtons[i] != null)
                    optionButtons[i].interactable = false;

            if (feedbackText != null)
            {
                feedbackText.gameObject.SetActive(true);
                feedbackText.color = correct ? UITheme.Success : UITheme.Danger;
                feedbackText.text = (correct ? "BENAR!" : "SALAH") + "\n" + (explanation ?? "");
                UITheme.FitText(feedbackText, 15f, true);
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
                if (nextLabel != null) nextLabel.text = "LANJUT";
            }
        }

        void HandleFinished(int percent, int correct, int total)
        {
            EnsureUI();
            waitingNext = true;
            for (int i = 0; i < optionButtons.Length; i++)
                if (optionButtons[i] != null)
                    optionButtons[i].gameObject.SetActive(false);

            if (questionText != null)
                questionText.text = "Kuis selesai!";

            if (feedbackText != null)
            {
                feedbackText.gameObject.SetActive(true);
                feedbackText.color = UITheme.GoldSoft;
                feedbackText.text = "Skor: " + percent + "%\nBenar " + correct + " / " + total;
                UITheme.FitText(feedbackText, 18f, true);
            }

            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
                if (nextLabel != null) nextLabel.text = "TUTUP";
            }
        }

        void OnOption(int index)
        {
            if (waitingNext) return;
            if (QuizManager.Instance == null) return;
            QuizManager.Instance.SubmitAnswer(index);
        }

        void OnNext()
        {
            if (QuizManager.Instance == null || !QuizManager.Instance.IsRunning)
            {
                ClosePanel();
                return;
            }

            QuizManager.Instance.NextQuestion();
        }

        void ClosePanel()
        {
            if (root != null) root.SetActive(false);
            LockCursor();
        }

        /// <summary>Hide quiz UI after abort / hard reset.</summary>
        public void ForceClose()
        {
            waitingNext = false;
            if (root != null) root.SetActive(false);
            LockCursor();
        }

        static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        static void EnsureEventSystem()
        {
            EventSystemGuard.Ensure();
        }

        void EnsureUI()
        {
            // Rebuild if destroyed with old scene canvas
            if (root == null)
                BuildUI();
        }

        void BuildUI()
        {
            // Own persistent canvas under this QuizSystems object
            var canvasGo = transform.Find("QuizCanvas");
            GameObject cgo;
            if (canvasGo != null)
                cgo = canvasGo.gameObject;
            else
            {
                cgo = new GameObject("QuizCanvas", typeof(RectTransform));
                cgo.transform.SetParent(transform, false);
            }

            canvas = cgo.GetComponent<Canvas>();
            if (canvas == null) canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 910; // above pause (900) — one modal tier

            var scaler = cgo.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = cgo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (cgo.GetComponent<GraphicRaycaster>() == null)
                cgo.AddComponent<GraphicRaycaster>();

            // Clear old children if rebuilding
            for (int i = cgo.transform.childCount - 1; i >= 0; i--)
                Destroy(cgo.transform.GetChild(i).gameObject);

            root = Create("QuizPanel", cgo.transform);
            Stretch(root.GetComponent<RectTransform>());
            var dim = root.AddComponent<Image>();
            dim.color = new Color(0.02f, 0.02f, 0.03f, 0.86f);
            dim.raycastTarget = true;

            var card = Create("QuizCard", root.transform);
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(620, 520);
            card.AddComponent<Image>().color = UITheme.PanelDark;
            card.AddComponent<RectMask2D>();

            var accent = Create("QuizAccent", card.transform);
            var art = accent.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(0f, 1f);
            art.anchorMax = new Vector2(1f, 1f);
            art.pivot = new Vector2(0.5f, 1f);
            art.sizeDelta = new Vector2(0f, 3f);
            accent.AddComponent<Image>().color = UITheme.Gold;

            titleText = CreateTmp("QuizTitle", card.transform, "KUIS BUKU", 24, UITheme.Gold, FontStyles.Bold);
            Place(titleText.rectTransform, 0.5f, 1f, 0, -22, 560, 32);
            titleText.alignment = TextAlignmentOptions.Center;

            progressText = CreateTmp("QuizProgress", card.transform, "SOAL 1 / 1", 13, UITheme.Muted, FontStyles.Normal);
            Place(progressText.rectTransform, 0.5f, 1f, 0, -52, 560, 22);
            progressText.alignment = TextAlignmentOptions.Center;

            questionText = CreateTmp("QuizQuestion", card.transform, "…", 18, UITheme.Cream, FontStyles.Normal);
            Place(questionText.rectTransform, 0.5f, 1f, 0, -88, 560, 90);
            questionText.alignment = TextAlignmentOptions.TopLeft;

            float y = -190f;
            for (int i = 0; i < 4; i++)
            {
                int captured = i;
                optionButtons[i] = CreateButton(card.transform, "Opt" + i, "…", new Vector2(0, y), new Vector2(560, 44), 15);
                optionLabels[i] = optionButtons[i].GetComponentInChildren<TMP_Text>();
                optionButtons[i].onClick.AddListener(() => OnOption(captured));
                y -= 52f;
            }

            feedbackText = CreateTmp("QuizFeedback", card.transform, "", 15, UITheme.GoldSoft, FontStyles.Normal);
            Place(feedbackText.rectTransform, 0.5f, 0f, 0, 78, 560, 56);
            feedbackText.alignment = TextAlignmentOptions.Center;
            feedbackText.gameObject.SetActive(false);

            nextButton = CreateButton(card.transform, "QuizNext", "LANJUT", new Vector2(0, 28), new Vector2(180, 42), 16);
            var nrt = nextButton.GetComponent<RectTransform>();
            nrt.anchorMin = nrt.anchorMax = new Vector2(0.5f, 0f);
            nrt.pivot = new Vector2(0.5f, 0f);
            nrt.anchoredPosition = new Vector2(0, 18);
            nextLabel = nextButton.GetComponentInChildren<TMP_Text>();
            nextButton.onClick.AddListener(OnNext);
            nextButton.gameObject.SetActive(false);

            var rootCg = root.GetComponent<CanvasGroup>();
            if (rootCg == null) rootCg = root.AddComponent<CanvasGroup>();
            rootCg.alpha = 1f;
            rootCg.interactable = true;
            rootCg.blocksRaycasts = true;
            rootCg.ignoreParentGroups = true;

            root.SetActive(false);
        }

        static GameObject Create(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void Place(RectTransform rt, float ax, float ay, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
            rt.pivot = new Vector2(0.5f, ay > 0.5f ? 1f : (ay < 0.5f ? 0f : 0.5f));
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        static TMP_Text CreateTmp(string name, Transform parent, string text, float size, Color color, FontStyles style)
        {
            var go = Create(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            UITheme.FitText(tmp, size, true);
            return tmp;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 posFromTop, Vector2 size, float fontSize)
        {
            var go = Create(name, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = posFromTop;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = Color.white;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            btn.colors = UITheme.ButtonColors();
            if (go.GetComponent<UIButtonPressFx>() == null)
                go.AddComponent<UIButtonPressFx>();

            var txt = CreateTmp(name + "Text", go.transform, label, fontSize, UITheme.Cream, FontStyles.Bold);
            Stretch(txt.rectTransform);
            txt.alignment = TextAlignmentOptions.Center;
            UITheme.FitText(txt, fontSize, false);
            return btn;
        }
    }
}
