using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] ProfilePanel profilePanel;
    [SerializeField] HelpPanel helpPanel;

    Canvas rootCanvas;
    CanvasGroup canvasGroup;
    bool transitioning;

    void Start()
    {
        UIMotion.EnsureInit();
        EventSystemGuard.Ensure();

        rootCanvas = FindAnyObjectByType<Canvas>();
        if (rootCanvas != null)
        {
            MainMenuVisuals.Apply(rootCanvas);
            EnsureCanvasGroup();
            PlayIntroMotion();
        }

        if (profilePanel == null)
            profilePanel = GetComponent<ProfilePanel>();
        if (profilePanel == null)
            profilePanel = gameObject.AddComponent<ProfilePanel>();

        if (helpPanel == null)
            helpPanel = GetComponent<HelpPanel>();
        if (helpPanel == null)
            helpPanel = gameObject.AddComponent<HelpPanel>();
    }

    void EnsureCanvasGroup()
    {
        if (rootCanvas == null) return;
        canvasGroup = rootCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = rootCanvas.gameObject.AddComponent<CanvasGroup>();
    }

    void PlayIntroMotion()
    {
        if (rootCanvas == null) return;

        AnimateNamed(rootCanvas.transform, "Titletext", 0f, new Vector2(0f, 22f));
        AnimateNamed(rootCanvas.transform, "Subtitle", 0.05f, new Vector2(0f, 16f));
        AnimateNamed(rootCanvas.transform, "BrandRule", 0.08f, new Vector2(0f, 10f));
        AnimateNamed(rootCanvas.transform, "MenuWelcomeChip", 0.08f, new Vector2(-24f, 0f));
        AnimateNamed(rootCanvas.transform, "Menupanel", 0.12f, new Vector2(0f, -30f));
        AnimateNamed(rootCanvas.transform, "MenuPanel", 0.12f, new Vector2(0f, -30f));
        AnimateNamed(rootCanvas.transform, "MenuFooterHint", 0.28f, new Vector2(0f, -14f));

        string[] btns =
        {
            "StartButton", "LeaderBoardButton", "SettingButton",
            "HelpButton", "reset", "ExitButton"
        };
        for (int i = 0; i < btns.Length; i++)
            AnimateNamed(rootCanvas.transform, btns[i], 0.16f + i * 0.045f, new Vector2(0f, -16f));
    }

    void AnimateNamed(Transform root, string name, float delay, Vector2 fromOffset)
    {
        Transform t = FindDeep(root, name);
        if (t == null) return;
        var rt = t as RectTransform;
        if (rt == null) return;
        var cg = t.GetComponent<CanvasGroup>();
        if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        StartCoroutine(DelayedSlide(cg, rt, delay, fromOffset));
    }

    IEnumerator DelayedSlide(CanvasGroup cg, RectTransform rt, float delay, Vector2 fromOffset)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (cg == null || rt == null) yield break;
        UIMotion.SlideFadeIn(cg, rt, fromOffset, 0.28f);
    }

    public void StartGame()
    {
        TransitionTo("campusyard");
    }

    public void OpenLeaderboard()
    {
        TransitionTo("Leaderboard");
    }

    void TransitionTo(string scene)
    {
        if (transitioning) return;
        transitioning = true;
        EnsureCanvasGroup();
        if (canvasGroup != null)
            UIMotion.FadeAndLoad(canvasGroup, scene, 0.22f);
        else
            SceneManager.LoadScene(scene);
    }

    public void OpenProfile()
    {
        if (helpPanel != null && helpPanel.IsOpen)
            helpPanel.Hide();
        if (profilePanel != null)
            profilePanel.Show();
    }

    public void OpenHelp()
    {
        if (profilePanel != null && profilePanel.IsOpen)
            profilePanel.Hide();
        if (helpPanel != null)
            helpPanel.Show();
    }

    public void OpenSettings()
    {
        OpenProfile();
    }

    public void ExitGame()
    {
        Debug.Log("Keluar dari game");
        Application.Quit();
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
