using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight unscaled UI motion (no external tween package required).
/// ponytail: DOTween Free if richer timeline/sequence needed later.
/// </summary>
public static class UIMotion
{
    static readonly Dictionary<EntityId, Coroutine> Active = new Dictionary<EntityId, Coroutine>();
    static MonoBehaviour Runner;

    public static void EnsureInit()
    {
        if (Runner != null) return;
        var go = new GameObject("UIMotionRunner");
        Object.DontDestroyOnLoad(go);
        Runner = go.AddComponent<UIMotionHost>();
    }

    public static void Kill(Component target)
    {
        if (target == null) return;
        EntityId id = target.GetEntityId();
        if (Active.TryGetValue(id, out var co) && Runner != null)
        {
            Runner.StopCoroutine(co);
            Active.Remove(id);
        }
    }

    public static void FadeCanvas(CanvasGroup cg, float to, float duration = 0.22f, float from = -1f)
    {
        if (cg == null) return;
        EnsureInit();
        Kill(cg);
        if (from >= 0f) cg.alpha = from;
        EntityId id = cg.GetEntityId();
        Active[id] = Runner.StartCoroutine(FadeRoutine(cg, cg.alpha, to, duration, id));
    }

    public static void PopIn(RectTransform rt, float duration = 0.22f)
    {
        if (rt == null) return;
        EnsureInit();
        Kill(rt);
        rt.localScale = Vector3.one * 0.92f;
        EntityId id = rt.GetEntityId();
        Active[id] = Runner.StartCoroutine(ScaleRoutine(rt, rt.localScale, Vector3.one, duration, id));
    }

    public static void PunchScale(RectTransform rt, float punch = 0.06f, float duration = 0.12f)
    {
        if (rt == null) return;
        EnsureInit();
        Kill(rt);
        EntityId id = rt.GetEntityId();
        Active[id] = Runner.StartCoroutine(PunchRoutine(rt, punch, duration, id));
    }

    public static void SlideFadeIn(CanvasGroup cg, RectTransform rt, Vector2 fromOffset, float duration = 0.28f)
    {
        if (cg == null || rt == null) return;
        EnsureInit();
        Kill(cg);
        Vector2 to = rt.anchoredPosition;
        rt.anchoredPosition = to + fromOffset;
        cg.alpha = 0f;
        EntityId id = cg.GetEntityId();
        Active[id] = Runner.StartCoroutine(SlideFadeRoutine(cg, rt, to + fromOffset, to, 0f, 1f, duration, id));
    }

    public static void FadeAndLoad(CanvasGroup cg, string sceneName, float duration = 0.25f)
    {
        if (cg == null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            return;
        }
        EnsureInit();
        Kill(cg);
        EntityId id = cg.GetEntityId();
        Active[id] = Runner.StartCoroutine(FadeLoadRoutine(cg, sceneName, duration, id));
    }

    static IEnumerator FadeRoutine(CanvasGroup cg, float from, float to, float duration, EntityId id)
    {
        float t = 0f;
        duration = Mathf.Max(0.01f, duration);
        while (t < duration && cg != null)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        if (cg != null) cg.alpha = to;
        Active.Remove(id);
    }

    static IEnumerator ScaleRoutine(RectTransform rt, Vector3 from, Vector3 to, float duration, EntityId id)
    {
        float t = 0f;
        duration = Mathf.Max(0.01f, duration);
        while (t < duration && rt != null)
        {
            t += Time.unscaledDeltaTime;
            rt.localScale = Vector3.LerpUnclamped(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
            yield return null;
        }
        if (rt != null) rt.localScale = to;
        Active.Remove(id);
    }

    static IEnumerator PunchRoutine(RectTransform rt, float punch, float duration, EntityId id)
    {
        if (rt == null)
        {
            Active.Remove(id);
            yield break;
        }
        Vector3 baseScale = Vector3.one;
        Vector3 peak = Vector3.one * (1f + punch);
        float half = Mathf.Max(0.01f, duration * 0.5f);
        float t = 0f;
        while (t < half && rt != null)
        {
            t += Time.unscaledDeltaTime;
            rt.localScale = Vector3.LerpUnclamped(baseScale, peak, Mathf.SmoothStep(0f, 1f, t / half));
            yield return null;
        }
        t = 0f;
        while (t < half && rt != null)
        {
            t += Time.unscaledDeltaTime;
            rt.localScale = Vector3.LerpUnclamped(peak, baseScale, Mathf.SmoothStep(0f, 1f, t / half));
            yield return null;
        }
        if (rt != null) rt.localScale = baseScale;
        Active.Remove(id);
    }

    static IEnumerator SlideFadeRoutine(
        CanvasGroup cg, RectTransform rt,
        Vector2 fromPos, Vector2 toPos,
        float fromA, float toA, float duration, EntityId id)
    {
        float t = 0f;
        duration = Mathf.Max(0.01f, duration);
        while (t < duration && cg != null && rt != null)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            rt.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, k);
            cg.alpha = Mathf.Lerp(fromA, toA, k);
            yield return null;
        }
        if (rt != null) rt.anchoredPosition = toPos;
        if (cg != null) cg.alpha = toA;
        Active.Remove(id);
    }

    static IEnumerator FadeLoadRoutine(CanvasGroup cg, string sceneName, float duration, EntityId id)
    {
        float from = cg.alpha;
        float t = 0f;
        duration = Mathf.Max(0.01f, duration);
        while (t < duration && cg != null)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, 0f, t / duration);
            yield return null;
        }
        Active.Remove(id);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    class UIMotionHost : MonoBehaviour { }
}
