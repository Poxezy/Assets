using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Title screen — single exclusive menu layout only.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    ProfilePanel profilePanel;
    HelpPanel helpPanel;
    Canvas rootCanvas;
    CanvasGroup canvasGroup;
    bool transitioning;

    void Start()
    {
        UIMotion.EnsureInit();
        EventSystemGuard.Ensure();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        rootCanvas = GetComponentInChildren<Canvas>();
        if (rootCanvas == null)
            rootCanvas = FindAnyObjectByType<Canvas>();
        if (rootCanvas == null)
        {
            var go = new GameObject("MainMenuCanvas", typeof(RectTransform));
            rootCanvas = go.AddComponent<Canvas>();
        }

        // Components first (no UI yet) — exclusive rebuild owns screen chrome
        profilePanel = GetComponent<ProfilePanel>();
        if (profilePanel == null) profilePanel = gameObject.AddComponent<ProfilePanel>();
        helpPanel = GetComponent<HelpPanel>();
        if (helpPanel == null) helpPanel = gameObject.AddComponent<HelpPanel>();

        BuildMenu();
        // Side panels kept by ClearChildren skip; hide until opened
        if (profilePanel != null) profilePanel.Hide();
        if (helpPanel != null) helpPanel.Hide();
    }

    void BuildMenu()
    {
        var actions = new ExclusiveMenuUI.Actions
        {
            Primary = StartGame,
            Leaderboard = OpenLeaderboard,
            Profile = OpenProfile,
            Help = OpenHelp,
            Reset = ResetProgress,
            Exit = ExitGame
        };

        var result = ExclusiveMenuUI.Build(
            rootCanvas.transform,
            ExclusiveMenuUI.Mode.Title,
            actions,
            clearHost: true);

        canvasGroup = result.Group;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        ExclusiveMenuUI.ForceAllButtons(rootCanvas.transform);
    }

    public void StartGame()
    {
        // Kill DDOL quiz dim that can steal clicks after reset
        if (MetaEdu.Quiz.QuizManager.Instance != null)
            MetaEdu.Quiz.QuizManager.Instance.ForceAbort();
        if (MetaEdu.Quiz.QuizUI.Instance != null)
            MetaEdu.Quiz.QuizUI.Instance.ForceClose();

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        TransitionTo("campusyard");
    }

    public void OpenLeaderboard()
    {
        TransitionTo("Leaderboard");
    }

    public void OpenProfile()
    {
        if (helpPanel != null && helpPanel.IsOpen) helpPanel.Hide();
        if (profilePanel != null) profilePanel.Show();
    }

    public void OpenHelp()
    {
        if (profilePanel != null && profilePanel.IsOpen) profilePanel.Hide();
        if (helpPanel != null) helpPanel.Show();
    }

    public void OpenSettings() => OpenProfile();

    void ResetProgress()
    {
        var reset = GetComponent<ResetData>();
        if (reset == null) reset = FindAnyObjectByType<ResetData>();
        if (reset == null) reset = gameObject.AddComponent<ResetData>();
        reset.ResetProgress();

        // Re-enable menu after reset (fade/overlay may have left dead state)
        transitioning = false;
        Time.timeScale = 1f;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        if (rootCanvas != null)
            ExclusiveMenuUI.ForceAllButtons(rootCanvas.transform);
        EventSystemGuard.Ensure();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    void TransitionTo(string scene)
    {
        if (transitioning) return;
        transitioning = true;
        if (canvasGroup == null && rootCanvas != null)
            canvasGroup = rootCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            UIMotion.FadeAndLoad(canvasGroup, scene, 0.2f);
        }
        else
            SceneManager.LoadScene(scene);
    }
}
