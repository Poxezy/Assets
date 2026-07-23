using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MetaEdu.Quest
{
    /// <summary>Top-center compass needle + distance + next-step label.</summary>
    public class QuestCompassUI : MonoBehaviour
    {
        public static QuestCompassUI Instance { get; private set; }

        GameObject root;
        GameObject panel;
        RectTransform needleRt;
        TMP_Text distText;
        TMP_Text labelText;
        TMP_Text titleText;
        CanvasGroup group;
        bool built;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            EnsureUI();
        }

        void Update()
        {
            string scene = SceneManager.GetActiveScene().name;
            if (scene == "MainMenu" || scene == "Leaderboard")
            {
                SetVisible(false);
                return;
            }

            if (MetaEdu.Quiz.QuizManager.Instance != null && MetaEdu.Quiz.QuizManager.Instance.IsRunning)
            {
                SetVisible(false);
                return;
            }

            EnsureUI();
            var wp = QuestWaypointService.Instance;
            bool hasPin = wp != null && wp.HasTarget;

            string label = null;
            if (hasPin)
                label = wp.TargetLabel;
            else if (QuestManager.Instance != null)
                label = QuestManager.Instance.GetNextStepText();

            if (string.IsNullOrEmpty(label)
                || label.IndexOf("Tidak ada misi", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (titleText != null) titleText.text = "ARAH";
                if (labelText != null) labelText.text = "Tidak ada target misi";
                if (distText != null) distText.text = "—";
                if (needleRt != null) needleRt.localRotation = Quaternion.identity;
                SetVisible(true);
                if (group != null) group.alpha = 0.5f;
                return;
            }

            SetVisible(true);
            if (group != null) group.alpha = hasPin ? 1f : 0.7f;

            if (titleText != null)
                titleText.text = hasPin ? "ARAH MISI" : "LANGKAH";

            if (hasPin)
            {
                float yaw = wp.GetYawToTarget();
                if (needleRt != null)
                    needleRt.localRotation = Quaternion.Euler(0f, 0f, -yaw);

                float dist = wp.GetDistanceToTarget();
                if (distText != null)
                    distText.text = dist >= 0f ? Mathf.RoundToInt(dist) + " m" : "—";
            }
            else
            {
                if (needleRt != null)
                    needleRt.localRotation = Quaternion.identity;
                if (distText != null)
                    distText.text = "…";
            }

            if (labelText != null)
            {
                string lab = label ?? "";
                if (lab.Length > 48) lab = lab.Substring(0, 47) + "…";
                labelText.text = lab;
            }
        }

        void SetVisible(bool on)
        {
            if (panel != null && panel.activeSelf != on)
                panel.SetActive(on);
        }

        void EnsureUI()
        {
            if (built && root != null) return;
            Build();
        }

        void Build()
        {
            built = true;

            var canvasGo = transform.Find("CompassCanvas");
            GameObject cgo;
            if (canvasGo != null)
                cgo = canvasGo.gameObject;
            else
            {
                cgo = new GameObject("CompassCanvas", typeof(RectTransform));
                cgo.transform.SetParent(transform, false);
            }

            var canvas = cgo.GetComponent<Canvas>();
            if (canvas == null) canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 440;

            var scaler = cgo.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = cgo.AddComponent<CanvasScaler>();
            UITheme.ApplyStandardScaler(scaler);
            if (cgo.GetComponent<GraphicRaycaster>() == null)
                cgo.AddComponent<GraphicRaycaster>();

            root = Create("CompassRoot", cgo.transform);
            Stretch(root.GetComponent<RectTransform>());
            var rootImg = root.AddComponent<Image>();
            rootImg.color = new Color(0, 0, 0, 0);
            rootImg.raycastTarget = false;

            panel = Create("CompassPanel", root.transform);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 1f);
            prt.anchorMax = new Vector2(0.5f, 1f);
            prt.pivot = new Vector2(0.5f, 1f);
            prt.anchoredPosition = new Vector2(0f, -16f);
            prt.sizeDelta = new Vector2(340f, 72f);

            var pimg = panel.AddComponent<Image>();
            pimg.color = UITheme.HudPanel;
            pimg.raycastTarget = false;
            panel.AddComponent<RectMask2D>();
            group = panel.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.55f);
            outline.effectDistance = new Vector2(1.25f, -1.25f);

            // Gold top accent
            var accent = Create("CompassAccent", panel.transform);
            var art = accent.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(0f, 1f);
            art.anchorMax = new Vector2(1f, 1f);
            art.pivot = new Vector2(0.5f, 1f);
            art.anchoredPosition = Vector2.zero;
            art.sizeDelta = new Vector2(0f, 2f);
            accent.AddComponent<Image>().color = UITheme.Gold;

            // Ring
            var ring = Create("CompassRing", panel.transform);
            var rrt = ring.GetComponent<RectTransform>();
            rrt.anchorMin = rrt.anchorMax = new Vector2(0f, 0.5f);
            rrt.pivot = new Vector2(0.5f, 0.5f);
            rrt.anchoredPosition = new Vector2(40f, -2f);
            rrt.sizeDelta = new Vector2(44f, 44f);
            var ringImg = ring.AddComponent<Image>();
            ringImg.color = UITheme.CardInner;
            ringImg.raycastTarget = false;
            var ringOutline = ring.AddComponent<Outline>();
            ringOutline.effectColor = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.45f);
            ringOutline.effectDistance = new Vector2(1f, -1f);

            var needle = Create("CompassNeedle", ring.transform);
            needleRt = needle.GetComponent<RectTransform>();
            needleRt.anchorMin = needleRt.anchorMax = new Vector2(0.5f, 0.5f);
            needleRt.pivot = new Vector2(0.5f, 0.12f);
            needleRt.anchoredPosition = Vector2.zero;
            needleRt.sizeDelta = new Vector2(7f, 24f);
            var nimg = needle.AddComponent<Image>();
            nimg.color = UITheme.Gold;
            nimg.raycastTarget = false;

            titleText = CreateTmp("CompassTitle", panel.transform, "ARAH MISI", 11, UITheme.GoldSoft, FontStyles.Bold);
            var trt = titleText.rectTransform;
            trt.anchorMin = new Vector2(0f, 1f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.pivot = new Vector2(0f, 1f);
            trt.anchoredPosition = new Vector2(78f, -8f);
            trt.sizeDelta = new Vector2(-90f, 16f);
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.characterSpacing = 2f;

            distText = CreateTmp("CompassDist", panel.transform, "—", 20, UITheme.Gold, FontStyles.Bold);
            var drt = distText.rectTransform;
            drt.anchorMin = new Vector2(0f, 0.5f);
            drt.anchorMax = new Vector2(1f, 1f);
            drt.offsetMin = new Vector2(78f, 2f);
            drt.offsetMax = new Vector2(-14f, -22f);
            distText.alignment = TextAlignmentOptions.Left;

            labelText = CreateTmp("CompassLabel", panel.transform, "Target misi", 12, UITheme.Cream, FontStyles.Normal);
            var lrt = labelText.rectTransform;
            lrt.anchorMin = new Vector2(0f, 0f);
            lrt.anchorMax = new Vector2(1f, 0.48f);
            lrt.offsetMin = new Vector2(78f, 8f);
            lrt.offsetMax = new Vector2(-14f, -2f);
            labelText.alignment = TextAlignmentOptions.Left;
            UITheme.FitText(labelText, 12f, true);
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

        static TMP_Text CreateTmp(string name, Transform parent, string text, float size, Color color, FontStyles style)
        {
            var go = Create(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = false;
            tmp.fontSize = size;
            return tmp;
        }
    }
}
