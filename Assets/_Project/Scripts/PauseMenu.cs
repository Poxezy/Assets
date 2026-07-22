using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool isPaused;
    private bool styled;
    private CanvasGroup panelGroup;

    private void Start()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        EnsureEventSystem();
        WirePauseButtons();
        StyleHudOnce();
    }

    static void EnsureEventSystem()
    {
        EventSystemGuard.Ensure();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        isPaused = true;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            panelGroup = pausePanel.GetComponent<CanvasGroup>();
            if (panelGroup == null)
                panelGroup = pausePanel.AddComponent<CanvasGroup>();
            panelGroup.alpha = 0f;
            UIMotion.FadeCanvas(panelGroup, 1f, 0.18f);
            var rt = pausePanel.GetComponent<RectTransform>();
            if (rt != null) UIMotion.PopIn(rt, 0.18f);
        }

        WirePauseButtons();
        StyleHudOnce();
        if (pausePanel != null)
            ExclusiveUIStyler.Apply(pausePanel.transform);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null)
        {
            if (panelGroup != null)
                UIMotion.FadeCanvas(panelGroup, 0f, 0.12f);
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    void WirePauseButtons()
    {
        Transform root = pausePanel != null ? pausePanel.transform : transform;
        Bind(root, "ResumeButton", ResumeGame);
        Bind(root, "MainMenuButton", BackToMainMenu);
        Bind(root, "ExitButton", ExitGame);
        Bind(root, "ContinueButton", ResumeGame);
        Bind(root, "Resume", ResumeGame);
        Bind(root, "MainMenu", BackToMainMenu);
        Bind(root, "Exit", ExitGame);
    }

    void Bind(Transform root, string childName, UnityEngine.Events.UnityAction action)
    {
        Transform t = FindDeep(root, childName);
        if (t == null && pausePanel != null)
            t = FindDeep(pausePanel.transform.root, childName);
        if (t == null) return;
        var btn = t.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }

    void StyleHudOnce()
    {
        if (styled) return;
        styled = true;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
            ExclusiveUIStyler.Apply(canvas.transform);
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
