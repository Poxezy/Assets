using UnityEngine;

/// <summary>
/// Scene-owned setup. One "Systems" GO per gameplay scene.
/// Books / doors / lighting / quiz / quest boot from here — not RuntimeInitialize.
/// </summary>
public class GameplaySceneSetup : MonoBehaviour
{
    [Header("What to run (this scene)")]
    [SerializeField] bool wireBooks = true;
    [SerializeField] bool spawnDoors = true;
    [SerializeField] bool applyLighting = true;
    [SerializeField] bool ensureQuiz = true;
    [SerializeField] bool ensureQuest = true;
    [SerializeField] bool ensureEventSystem = true;

    bool ran;

    void Start()
    {
        if (ran) return;
        ran = true;

        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene == "MainMenu" || scene == "Leaderboard")
            return;

        if (ensureEventSystem)
            EventSystemGuard.Ensure();

        // School FBX "Boole" meshes often ship materials > submeshes
        MeshMaterialClamp.ClampActiveScene();

        if (applyLighting)
            RoomLightingBootstrap.ApplyForActiveScene();

        if (wireBooks)
            BookPickupBootstrap.WireBooksInActiveScene();

        if (spawnDoors)
            SceneDoorBootstrap.SpawnForActiveScene();

        if (ensureQuiz)
            MetaEdu.Quiz.QuizManager.EnsureSystems();

        if (ensureQuest)
            MetaEdu.Quest.QuestManager.EnsureSystems();

        Debug.Log("GameplaySceneSetup: ready @ " + scene + " (scene-owned)");
    }
}
