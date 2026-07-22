using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Walk-in gate. Loads target scene if unlocked.
/// Missing scenes fall back to campusyard (no crash).
/// </summary>
public class AreaGate : MonoBehaviour
{
    [Header("Area Tujuan")]
    [SerializeField] private string areaName = "Campus";
    [SerializeField] private string targetScene = "campusyard";

    [Header("Pesan")]
    [SerializeField] private int requiredLevel = 1;

    static readonly string[] KnownScenes =
    {
        "MainMenu", "campusyard", "classroom", "Leaderboard", "MainScene"
    };

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (ScoreManager.Instance == null)
        {
            Debug.LogError("ScoreManager tidak ditemukan.");
            return;
        }

        if (!ScoreManager.Instance.IsAreaUnlocked(areaName))
        {
            Debug.Log(areaName + " masih terkunci. Dibutuhkan Level " + requiredLevel + ".");
            return;
        }

        string scene = ResolveScene(targetScene);
        if (string.IsNullOrEmpty(scene))
        {
            Debug.LogWarning("AreaGate: no valid scene for '" + targetScene + "' — stay.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    static string ResolveScene(string requested)
    {
        if (string.IsNullOrEmpty(requested))
            return "campusyard";

        // Exact known build scene
        for (int i = 0; i < KnownScenes.Length; i++)
        {
            if (string.Equals(KnownScenes[i], requested, System.StringComparison.OrdinalIgnoreCase))
                return KnownScenes[i];
        }

        // Legacy "Library" door → exit to campus
        if (requested.IndexOf("Library", System.StringComparison.OrdinalIgnoreCase) >= 0
            || requested.IndexOf("Lab", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return "campusyard";

        // Probe build settings
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrEmpty(path)) continue;
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, requested, System.StringComparison.OrdinalIgnoreCase))
                return name;
        }

        return "campusyard";
    }
}
