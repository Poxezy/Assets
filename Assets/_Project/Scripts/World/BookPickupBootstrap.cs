using UnityEngine;

/// <summary>
/// Wires book objects as KnowledgeItem pickups.
/// Call from GameplaySceneSetup (scene Systems GO) — no RuntimeInitialize.
/// </summary>
public class BookPickupBootstrap : MonoBehaviour
{
    const int DefaultPoints = 50;

    void Start()
    {
        WireBooksInActiveScene();
    }

    public static void WireBooksInActiveScene()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene == "MainMenu" || scene == "Leaderboard") return;
        WireBooksInScene(scene);
    }

    static void WireBooksInScene(string sceneName)
    {
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        int wired = 0;

        for (int i = 0; i < all.Length; i++)
        {
            Transform t = all[i];
            if (t == null) continue;

            string n = t.name;
            if (n.IndexOf("book", System.StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            // Skip child meshes under an already-wired book
            if (t.GetComponentInParent<KnowledgeItem>() != null)
                continue;

            // Only root-ish book objects (has renderer or is named book*)
            if (t.GetComponentInChildren<Renderer>() == null)
                continue;

            // Prefer topmost transform whose name contains book
            if (t.parent != null &&
                t.parent.name.IndexOf("book", System.StringComparison.OrdinalIgnoreCase) >= 0)
                continue;

            var item = t.gameObject.AddComponent<KnowledgeItem>();
            string id = sceneName + "_" + Sanitize(n) + "_" + t.GetEntityId();
            item.Configure(id, "Knowledge Book", DefaultPoints, once: true);

            if (t.GetComponent<ItemVisualEffect>() == null)
                t.gameObject.AddComponent<ItemVisualEffect>();

            // KnowledgeItem.Awake already ran before Configure — force physics if needed
            EnsurePickupTrigger(t.gameObject);
            EnsureKinematicBody(t.gameObject);
            wired++;
        }

        if (wired > 0)
            Debug.Log("BookPickupBootstrap: wired " + wired + " books in " + sceneName);
    }

    static void EnsurePickupTrigger(GameObject go)
    {
        var cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].isTrigger) return;
        }

        var box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;

        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
                b.Encapsulate(rends[i].bounds);

            Vector3 localCenter = go.transform.InverseTransformPoint(b.center);
            Vector3 localSize = go.transform.InverseTransformVector(b.size);
            localSize = new Vector3(
                Mathf.Abs(localSize.x) * 1.5f + 0.6f,
                Mathf.Abs(localSize.y) * 1.5f + 0.6f,
                Mathf.Abs(localSize.z) * 1.5f + 0.6f);
            localSize = Vector3.Max(localSize, new Vector3(1.5f, 1.5f, 1.5f));
            box.center = localCenter;
            box.size = localSize;
        }
        else
        {
            box.center = new Vector3(0f, 0.5f, 0f);
            box.size = new Vector3(1.5f, 1.5f, 1.5f);
        }
    }

    static void EnsureKinematicBody(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    static string Sanitize(string name)
    {
        char[] chars = name.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (!(char.IsLetterOrDigit(c) || c == '_'))
                chars[i] = '_';
        }
        return new string(chars);
    }
}
