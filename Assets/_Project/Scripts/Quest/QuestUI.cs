using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetaEdu.Quest
{
    /// <summary>
    /// Premium quest tracker — dark panel, cream body, gold titles, next-step chip.
    /// </summary>
    public class QuestUI : MonoBehaviour
    {
        public static QuestUI Instance { get; private set; }

        GameObject root;
        GameObject card;
        CanvasGroup cardGroup;
        TMP_Text headerText;
        TMP_Text bodyText;
        TMP_Text toastText;
        GameObject toastRoot;
        CanvasGroup toastGroup;
        Canvas canvas;
        bool bound;
        float toastTimer;
        bool cardOpen = true;

        public static void EnsureExists()
        {
            QuestManager.EnsureSystems();
        }

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
            Bind();
            Refresh();
        }

        void OnDisable()
        {
            Unbind();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Unbind();
            if (cardGroup != null) UIMotion.Kill(cardGroup);
            if (toastGroup != null) UIMotion.Kill(toastGroup);
        }

        void Update()
        {
            if (toastRoot != null && toastRoot.activeSelf)
            {
                toastTimer -= Time.unscaledDeltaTime;
                if (toastTimer <= 0f)
                    HideToast();
            }

            if (Input.GetKeyDown(KeyCode.J) && card != null)
            {
                cardOpen = !cardOpen;
                SetCardVisible(cardOpen, animate: true);
                if (cardOpen) Refresh();
            }
        }

        void Bind()
        {
            if (bound) return;
            if (QuestManager.Instance == null) return;
            QuestManager.Instance.OnQuestActivated += OnActivated;
            QuestManager.Instance.OnQuestCompleted += OnCompleted;
            QuestManager.Instance.OnQuestUpdated += OnUpdated;
            bound = true;
        }

        void Unbind()
        {
            if (!bound) return;
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestActivated -= OnActivated;
                QuestManager.Instance.OnQuestCompleted -= OnCompleted;
                QuestManager.Instance.OnQuestUpdated -= OnUpdated;
            }
            bound = false;
        }

        public void Prepare()
        {
            EnsureUI();
            Bind();
            Refresh();
        }

        void OnActivated(QuestData q)
        {
            EnsureUI();
            ShowToast("MISI AKTIF\n" + (q != null ? q.questTitle : ""));
            Refresh();
            cardOpen = true;
            SetCardVisible(true, animate: true);
        }

        void OnCompleted(QuestData q)
        {
            EnsureUI();
            ShowToast("MISI SELESAI\n" + (q != null ? q.questTitle : ""));
            Refresh();
        }

        void OnUpdated(QuestData q)
        {
            Refresh();
        }

        void ShowToast(string msg)
        {
            if (toastRoot == null || toastText == null) return;
            toastText.text = msg;
            UITheme.FitText(toastText, 15f, true);
            toastRoot.SetActive(true);
            toastTimer = 3f;
            if (toastGroup != null)
            {
                toastGroup.alpha = 0f;
                UIMotion.FadeCanvas(toastGroup, 1f, 0.18f);
            }
            if (toastRoot.transform is RectTransform trt)
                UIMotion.PopIn(trt, 0.18f);
        }

        void HideToast()
        {
            if (toastRoot == null) return;
            if (toastGroup != null)
                UIMotion.FadeCanvas(toastGroup, 0f, 0.15f);
            toastRoot.SetActive(false);
        }

        void SetCardVisible(bool visible, bool animate)
        {
            if (card == null) return;
            card.SetActive(true);
            if (cardGroup == null)
                cardGroup = card.GetComponent<CanvasGroup>() ?? card.AddComponent<CanvasGroup>();

            if (!animate)
            {
                cardGroup.alpha = visible ? 1f : 0f;
                cardGroup.blocksRaycasts = false;
                cardGroup.interactable = false;
                if (!visible) card.SetActive(false);
                return;
            }

            if (visible)
            {
                card.SetActive(true);
                UIMotion.FadeCanvas(cardGroup, 1f, 0.2f, from: 0f);
                UIMotion.PopIn(card.GetComponent<RectTransform>(), 0.2f);
            }
            else
            {
                // Instant hide keeps J-toggle snappy; fade only on open
                UIMotion.Kill(cardGroup);
                cardGroup.alpha = 0f;
                card.SetActive(false);
            }
        }

        public void Refresh()
        {
            EnsureUI();
            if (bodyText == null || QuestManager.Instance == null) return;

            var active = QuestManager.Instance.GetActiveQuestData();
            var sb = new StringBuilder(384);

            if (active.Count == 0)
            {
                if (headerText != null)
                    headerText.text = "MISI";
                sb.Append("<color=#C8BDA8>Tidak ada misi aktif.</color>\n");
                sb.Append("<color=#A89E8E><size=12>Jelajahi kampus atau tekan J.</size></color>");
            }
            else
            {
                if (headerText != null)
                    headerText.text = "MISI  ·  " + active.Count;

                string next = QuestManager.Instance.GetNextStepText();
                sb.Append("<color=#F5E08A><b>SELANJUTNYA</b></color>\n");
                sb.Append("<color=#FAF5EB><size=13>");
                sb.Append(next);
                sb.Append("</size></color>\n\n");

                for (int i = 0; i < active.Count; i++)
                {
                    var q = active[i];
                    if (q == null) continue;

                    sb.Append("<color=#F5E08A><b>");
                    sb.Append(q.questTitle ?? "Misi");
                    sb.Append("</b></color>\n");

                    if (!string.IsNullOrEmpty(q.description))
                    {
                        sb.Append("<color=#FAF5EB><size=13>");
                        sb.Append(q.description);
                        sb.Append("</size></color>\n");
                    }

                    if (q.objectives != null)
                    {
                        for (int o = 0; o < q.objectives.Count; o++)
                        {
                            var obj = q.objectives[o];
                            if (obj == null) continue;
                            bool done = obj.isCompleted;
                            string mark = done ? "✓" : "○";
                            string col = done ? "#7AD98F" : "#FAF5EB";
                            sb.Append("<color=");
                            sb.Append(col);
                            sb.Append(">");
                            sb.Append(mark);
                            sb.Append(" ");
                            sb.Append(obj.description ?? "Objektif");
                            if (obj.requiredCount > 1)
                            {
                                sb.Append("  (");
                                sb.Append(Mathf.Min(obj.currentCount, obj.requiredCount));
                                sb.Append("/");
                                sb.Append(obj.requiredCount);
                                sb.Append(")");
                            }
                            sb.Append("</color>\n");

                            // Text progress bar for multi-count
                            if (obj.requiredCount > 1 && !done)
                            {
                                int filled = Mathf.Clamp(obj.currentCount, 0, obj.requiredCount);
                                int total = obj.requiredCount;
                                sb.Append("<color=#C8BDA8><size=11>[");
                                for (int b = 0; b < total; b++)
                                    sb.Append(b < filled ? "■" : "□");
                                sb.Append("]</size></color>\n");
                            }
                        }
                    }

                    if (q.xpReward > 0 || !string.IsNullOrEmpty(q.badgeReward))
                    {
                        sb.Append("<color=#C8BDA8><size=12>Hadiah · ");
                        if (q.xpReward > 0)
                        {
                            sb.Append(q.xpReward);
                            sb.Append(" XP");
                        }
                        if (!string.IsNullOrEmpty(q.badgeReward))
                        {
                            if (q.xpReward > 0) sb.Append("  ·  ");
                            sb.Append("badge ");
                            sb.Append(q.badgeReward);
                        }
                        sb.Append("</size></color>\n");
                    }

                    if (i < active.Count - 1)
                        sb.Append("\n");
                }

                sb.Append("\n<color=#A89E8E><size=11>J · panel misi  ·  ikuti kompas</size></color>");
            }

            bodyText.richText = true;
            bodyText.text = sb.ToString();
            bodyText.color = UITheme.Cream;
            UITheme.FitText(bodyText, 14f, true);

            // Dynamic height
            if (card != null)
            {
                var cardRt = card.GetComponent<RectTransform>();
                int lines = 1;
                for (int i = 0; i < sb.Length; i++)
                    if (sb[i] == '\n') lines++;
                float h = Mathf.Clamp(42f + lines * 18f, 180f, 420f);
                cardRt.sizeDelta = new Vector2(360f, h);

                if (toastRoot != null)
                {
                    var trt = toastRoot.GetComponent<RectTransform>();
                    trt.anchoredPosition = new Vector2(-24f, -24f - h - 12f);
                }
            }
        }

        void EnsureUI()
        {
            if (root != null) return;
            Build();
        }

        void Build()
        {
            var canvasGo = transform.Find("QuestCanvas");
            GameObject cgo;
            if (canvasGo != null)
                cgo = canvasGo.gameObject;
            else
            {
                cgo = new GameObject("QuestCanvas", typeof(RectTransform));
                cgo.transform.SetParent(transform, false);
            }

            canvas = cgo.GetComponent<Canvas>();
            if (canvas == null) canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 450;

            var scaler = cgo.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = cgo.AddComponent<CanvasScaler>();
            UITheme.ApplyStandardScaler(scaler);
            if (cgo.GetComponent<GraphicRaycaster>() == null)
                cgo.AddComponent<GraphicRaycaster>();

            root = Create("QuestRoot", cgo.transform);
            Stretch(root.GetComponent<RectTransform>());
            var rootImg = root.AddComponent<Image>();
            rootImg.color = new Color(0, 0, 0, 0);
            rootImg.raycastTarget = false;

            card = Create("QuestCard", root.transform);
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(1f, 1f);
            cardRt.anchorMax = new Vector2(1f, 1f);
            cardRt.pivot = new Vector2(1f, 1f);
            cardRt.anchoredPosition = new Vector2(-24f, -24f);
            cardRt.sizeDelta = new Vector2(360f, 280f);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = UITheme.PanelDark;
            cardImg.raycastTarget = false;
            card.AddComponent<RectMask2D>();
            cardGroup = card.AddComponent<CanvasGroup>();
            cardGroup.blocksRaycasts = false;
            cardGroup.interactable = false;

            var outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.55f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var accent = Create("QuestAccent", card.transform);
            var art = accent.GetComponent<RectTransform>();
            art.anchorMin = new Vector2(0f, 1f);
            art.anchorMax = new Vector2(1f, 1f);
            art.pivot = new Vector2(0.5f, 1f);
            art.anchoredPosition = Vector2.zero;
            art.sizeDelta = new Vector2(0f, 3f);
            accent.AddComponent<Image>().color = UITheme.Gold;

            var inner = Create("QuestInner", card.transform);
            var irt = inner.GetComponent<RectTransform>();
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(10, 10);
            irt.offsetMax = new Vector2(-10, -16);
            var iimg = inner.AddComponent<Image>();
            iimg.color = UITheme.CardInner;
            iimg.raycastTarget = false;

            headerText = CreateTmp("QuestHeader", card.transform, "MISI", 16, UITheme.Gold, FontStyles.Bold);
            var hrt = headerText.rectTransform;
            hrt.anchorMin = new Vector2(0f, 1f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.pivot = new Vector2(0.5f, 1f);
            hrt.anchoredPosition = new Vector2(0f, -14f);
            hrt.offsetMin = new Vector2(16f, hrt.offsetMin.y);
            hrt.offsetMax = new Vector2(-16f, hrt.offsetMax.y);
            hrt.sizeDelta = new Vector2(hrt.sizeDelta.x, 24f);
            headerText.alignment = TextAlignmentOptions.Left;
            UITheme.StyleTitle(headerText, 16f);

            bodyText = CreateTmp("QuestBody", card.transform, "", 14, UITheme.Cream, FontStyles.Normal);
            var brt = bodyText.rectTransform;
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(16f, 14f);
            brt.offsetMax = new Vector2(-16f, -42f);
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            bodyText.richText = true;
            UITheme.StyleBody(bodyText, 14f);

            toastRoot = Create("QuestToast", root.transform);
            var trt = toastRoot.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(1f, 1f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.pivot = new Vector2(1f, 1f);
            trt.anchoredPosition = new Vector2(-24f, -316f);
            trt.sizeDelta = new Vector2(320f, 64f);
            var timg = toastRoot.AddComponent<Image>();
            timg.color = UITheme.HudPanel;
            toastRoot.AddComponent<RectMask2D>();
            toastGroup = toastRoot.AddComponent<CanvasGroup>();
            var tOutline = toastRoot.AddComponent<Outline>();
            tOutline.effectColor = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.5f);
            tOutline.effectDistance = new Vector2(1.2f, -1.2f);

            var tAccent = Create("ToastAccent", toastRoot.transform);
            var tart = tAccent.GetComponent<RectTransform>();
            tart.anchorMin = new Vector2(0f, 0f);
            tart.anchorMax = new Vector2(0f, 1f);
            tart.pivot = new Vector2(0f, 0.5f);
            tart.sizeDelta = new Vector2(4f, 0f);
            tAccent.AddComponent<Image>().color = UITheme.Gold;

            toastText = CreateTmp("ToastText", toastRoot.transform, "", 14, UITheme.GoldSoft, FontStyles.Bold);
            var ttrt = toastText.rectTransform;
            ttrt.anchorMin = Vector2.zero;
            ttrt.anchorMax = Vector2.one;
            ttrt.offsetMin = new Vector2(14f, 8f);
            ttrt.offsetMax = new Vector2(-10f, -8f);
            toastText.alignment = TextAlignmentOptions.Center;
            toastRoot.SetActive(false);

            card.SetActive(true);
            cardOpen = true;
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
            UITheme.FitText(tmp, size, true);
            return tmp;
        }
    }
}
