using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ESC pause — one exclusive overlay canvas. Prefab PausePanel forced off.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pausePanel; // legacy prefab ref — never shown

    const int Sort = 900;

    bool isPaused;
    GameObject pauseRoot;
    Canvas pauseCanvas;
    CanvasGroup rootGroup;
    ProfilePanel profilePanel;
    HelpPanel helpPanel;
    ResetData resetData;

    void Start()
    {
        Time.timeScale = 1f;
        EventSystemGuard.Ensure();
        if (pausePanel != null)
            pausePanel.SetActive(false);
        KillPrefabPauseChrome();
        EnsureSidePanels();
    }

    void OnDestroy()
    {
        MuteOtherCanvases(false);
        if (pauseRoot != null)
            Destroy(pauseRoot);
    }

    void OnDisable()
    {
        MuteOtherCanvases(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (isPaused)
        {
            // Close side panels first
            if (profilePanel != null && profilePanel.IsOpen) { profilePanel.Hide(); return; }
            if (helpPanel != null && helpPanel.IsOpen) { helpPanel.Hide(); return; }
            ResumeGame();
        }
        else OpenPauseMenu();
    }

    public void OpenPauseMenu()
    {
        isPaused = true;
        EventSystemGuard.Ensure();
        EnsureSidePanels();
        Build();

        pauseRoot.SetActive(true);
        if (rootGroup != null)
        {
            rootGroup.alpha = 1f;
            rootGroup.blocksRaycasts = true;
            rootGroup.interactable = true;
            rootGroup.ignoreParentGroups = true;
        }

        ExclusiveMenuUI.ForceAllButtons(pauseRoot.transform);
        MuteOtherCanvases(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (profilePanel != null && profilePanel.IsOpen) profilePanel.Hide();
        if (helpPanel != null && helpPanel.IsOpen) helpPanel.Hide();
        if (pauseRoot != null) pauseRoot.SetActive(false);
        MuteOtherCanvases(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        MuteOtherCanvases(false);
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenLeaderboard()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        MuteOtherCanvases(false);
        SceneManager.LoadScene("Leaderboard");
    }

    public void OpenProfile()
    {
        EnsureSidePanels();
        if (helpPanel != null && helpPanel.IsOpen) helpPanel.Hide();
        if (profilePanel != null)
        {
            profilePanel.Show();
            Lift("ProfilePanel");
            if (pauseRoot != null)
                ExclusiveMenuUI.ForceAllButtons(pauseRoot.transform);
        }
    }

    public void OpenHelp()
    {
        EnsureSidePanels();
        if (profilePanel != null && profilePanel.IsOpen) profilePanel.Hide();
        if (helpPanel != null)
        {
            helpPanel.Show();
            Lift("HelpPanel");
            if (pauseRoot != null)
                ExclusiveMenuUI.ForceAllButtons(pauseRoot.transform);
        }
    }

    public void ResetProgress()
    {
        if (resetData == null)
            resetData = GetComponent<ResetData>() ?? gameObject.AddComponent<ResetData>();
        resetData.ResetProgress();
        // Stay playable after reset — unpause so petualangan can continue
        ResumeGame();
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    void Build()
    {
        if (pauseRoot == null)
        {
            pauseRoot = new GameObject("ExclusivePauseRoot");
            pauseCanvas = pauseRoot.AddComponent<Canvas>();
            pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            pauseCanvas.sortingOrder = Sort;
            pauseCanvas.overrideSorting = true;
            var scaler = pauseRoot.AddComponent<CanvasScaler>();
            UITheme.ApplyStandardScaler(scaler);
            pauseRoot.AddComponent<GraphicRaycaster>();
            rootGroup = pauseRoot.AddComponent<CanvasGroup>();
        }
        else
        {
            pauseCanvas = pauseRoot.GetComponent<Canvas>();
            rootGroup = pauseRoot.GetComponent<CanvasGroup>();
            // wipe previous exclusive chrome; keep side panels
            for (int i = pauseRoot.transform.childCount - 1; i >= 0; i--)
            {
                var c = pauseRoot.transform.GetChild(i);
                if (c == null) continue;
                string n = c.name;
                if (n == "ProfilePanel" || n == "HelpPanel")
                {
                    c.gameObject.SetActive(false);
                    continue;
                }
                c.gameObject.SetActive(false);
                Destroy(c.gameObject);
            }
        }

        pauseCanvas.sortingOrder = Sort;
        pauseCanvas.overrideSorting = true;
        rootGroup.alpha = 1f;
        rootGroup.blocksRaycasts = true;
        rootGroup.interactable = true;
        rootGroup.ignoreParentGroups = true;

        var actions = new ExclusiveMenuUI.Actions
        {
            Primary = ResumeGame,
            Leaderboard = OpenLeaderboard,
            Profile = OpenProfile,
            Help = OpenHelp,
            MainMenu = BackToMainMenu,
            Reset = ResetProgress,
            Exit = ExitGame
        };

        ExclusiveMenuUI.Build(
            pauseRoot.transform,
            ExclusiveMenuUI.Mode.Pause,
            actions,
            clearHost: true);

        // Rebuild wipes CanvasGroup on host? re-get
        rootGroup = pauseRoot.GetComponent<CanvasGroup>();
        if (rootGroup == null) rootGroup = pauseRoot.AddComponent<CanvasGroup>();
        rootGroup.alpha = 1f;
        rootGroup.blocksRaycasts = true;
        rootGroup.interactable = true;
        rootGroup.ignoreParentGroups = true;

        if (pauseRoot.GetComponent<GraphicRaycaster>() == null)
            pauseRoot.AddComponent<GraphicRaycaster>();
        if (pauseCanvas == null)
            pauseCanvas = pauseRoot.GetComponent<Canvas>();
        if (pauseCanvas != null)
        {
            pauseCanvas.sortingOrder = Sort;
            pauseCanvas.overrideSorting = true;
            pauseCanvas.enabled = true;
        }
    }

    void Lift(string panelName)
    {
        if (pauseRoot == null) return;
        var all = FindObjectsByType<RectTransform>(FindObjectsInactive.Include);
        Transform panel = null;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].name == panelName && all[i].gameObject.activeInHierarchy)
            {
                panel = all[i];
                break;
            }
        }
        if (panel == null) return;
        panel.SetParent(pauseRoot.transform, false);
        var rt = panel as RectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        panel.SetAsLastSibling();
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = true;
            cg.interactable = true;
            cg.ignoreParentGroups = true;
            cg.alpha = 1f;
        }
    }

    void EnsureSidePanels()
    {
        if (profilePanel == null)
            profilePanel = GetComponent<ProfilePanel>() ?? gameObject.AddComponent<ProfilePanel>();
        if (helpPanel == null)
            helpPanel = GetComponent<HelpPanel>() ?? gameObject.AddComponent<HelpPanel>();
        if (resetData == null)
            resetData = GetComponent<ResetData>() ?? gameObject.AddComponent<ResetData>();
    }

    void MuteOtherCanvases(bool mute)
    {
        var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        for (int i = 0; i < canvases.Length; i++)
        {
            var c = canvases[i];
            if (c == null || c == pauseCanvas) continue;
            if (c.sortingOrder >= Sort) continue;
            var ray = c.GetComponent<GraphicRaycaster>();
            if (ray != null) ray.enabled = !mute;
        }
    }

    static void KillPrefabPauseChrome()
    {
        var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] == null) continue;
            // Force prefab PausePanel off always
            Transform t = FindDeep(canvases[i].transform, "PausePanel");
            if (t != null) t.gameObject.SetActive(false);

            string[] junk = { "PauseTitle", "ResumeButton", "ResumeText", "MainMenuText", "ExitText" };
            for (int j = 0; j < junk.Length; j++)
            {
                var jn = FindDeep(canvases[i].transform, junk[j]);
                if (jn != null) jn.gameObject.SetActive(false);
            }
        }
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var f = FindDeep(root.GetChild(i), name);
            if (f != null) return f;
        }
        return null;
    }
}
