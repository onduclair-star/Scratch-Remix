using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class FadeTarget
{
    public GameObject target;
}

public class FadeController : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scale Effect")]
    [SerializeField] private bool enableScaleEffect = true;
    [SerializeField] private Vector2 scaleFrom = new(0.8f, 0.8f);
    [SerializeField] private Vector2 scaleTo = new(1f, 1f);

    /// <summary>
    /// Fade 一组 GameObject
    /// </summary>
    public Coroutine Fade(List<GameObject> objects, bool fadeIn, Action onComplete = null)
    {
        if (objects == null || objects.Count == 0) return null;
        return StartCoroutine(FadeRoutine(objects, fadeIn, onComplete));
    }

    private IEnumerator FadeRoutine(List<GameObject> objects, bool fadeIn, Action onComplete)
    {
        float duration = fadeIn ? fadeInDuration : fadeOutDuration;
        AnimationCurve curve = fadeIn ? fadeInCurve : fadeOutCurve;

        var graphics = new List<Graphic>();
        var renderers = new List<Renderer>();
        var transforms = new List<Transform>();

        foreach (var go in objects)
        {
            if (!go) continue;

            if (go.TryGetComponent(out Graphic g)) graphics.Add(g);
            if (go.TryGetComponent(out Renderer r)) renderers.Add(r);

            transforms.Add(go.transform);

            if (fadeIn) go.SetActive(true);
        }

        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = curve.Evaluate(time / duration);
            float alpha = fadeIn ? t : 1f - t;

            // Graphic 透明度
            foreach (var g in graphics)
                if (g) g.material.color = SetAlpha(g.material.color, alpha);

            // Renderer 透明度
            foreach (var r in renderers)
            {
                if (r && r.material.HasProperty("_Color"))
                {
                    Color c = r.material.color;
                    c.a = alpha;
                    r.material.color = c;
                }
            }

            // 缩放效果
            if (enableScaleEffect)
            {
                Vector3 scale = Vector3.Lerp(scaleFrom, scaleTo, alpha);
                foreach (var tr in transforms)
                    if (tr) tr.localScale = scale;
            }

            yield return null;
        }

        float finalAlpha = fadeIn ? 1f : 0f;

        foreach (var g in graphics)
            if (g) g.material.color = SetAlpha(g.material.color, finalAlpha);

        foreach (var r in renderers)
            if (r && r.material.HasProperty("_Color"))
            {
                Color c = r.material.color;
                c.a = finalAlpha;
                r.material.color = c;
            }

        if (!fadeIn)
        {
            foreach (var go in objects)
                if (go) go.SetActive(false);
        }

        onComplete?.Invoke();
    }

    private Color SetAlpha(Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }
}
