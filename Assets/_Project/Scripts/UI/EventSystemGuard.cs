using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Exactly one active EventSystem. Disables duplicates immediately (Destroy is end-of-frame).
/// </summary>
public static class EventSystemGuard
{
    static bool hooked;
    static EventSystemKeeper keeper;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        hooked = false;
        keeper = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        Ensure();
        if (hooked) return;
        hooked = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene _, LoadSceneMode __)
    {
        Ensure();
    }

    public static void Ensure()
    {
        EnsureKeeper();

        var all = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
        if (all == null || all.Length == 0)
        {
            var go = new GameObject("EventSystem");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            return;
        }

        EventSystem keep = PickKeep(all);
        for (int i = 0; i < all.Length; i++)
        {
            var es = all[i];
            if (es == null || es == keep) continue;

            // Stop Update spam this frame (Destroy waits end of frame)
            es.enabled = false;
            var module = es.GetComponent<BaseInputModule>();
            if (module != null) module.enabled = false;

            if (Application.isPlaying)
                Object.Destroy(es.gameObject);
        }

        if (keep != null)
        {
            keep.enabled = true;
            if (keep.GetComponent<BaseInputModule>() == null)
                keep.gameObject.AddComponent<StandaloneInputModule>();
            else
            {
                var m = keep.GetComponent<BaseInputModule>();
                if (m != null) m.enabled = true;
            }
        }
    }

    static EventSystem PickKeep(EventSystem[] all)
    {
        // Prefer DontDestroyOnLoad so scene reloads don't thrash
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;
            if (all[i].gameObject.scene.name == "DontDestroyOnLoad")
                return all[i];
        }
        return all[0];
    }

    static void EnsureKeeper()
    {
        if (keeper != null) return;
        var go = new GameObject("EventSystemKeeper");
        Object.DontDestroyOnLoad(go);
        keeper = go.AddComponent<EventSystemKeeper>();
    }

    sealed class EventSystemKeeper : MonoBehaviour
    {
        void LateUpdate()
        {
            var all = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
            if (all == null || all.Length <= 1) return;

            EventSystem keep = PickKeep(all);
            for (int i = 0; i < all.Length; i++)
            {
                var es = all[i];
                if (es == null || es == keep) continue;
                es.enabled = false;
                var module = es.GetComponent<BaseInputModule>();
                if (module != null) module.enabled = false;
                Destroy(es.gameObject);
            }

            if (keep != null && !keep.enabled)
                keep.enabled = true;
        }
    }
}
