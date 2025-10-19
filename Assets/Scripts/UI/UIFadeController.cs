using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFadeController : MonoBehaviour
{
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.5f;
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public bool enableScaleEffect = true;
    public Vector2 scaleFrom = new Vector2(0.8f, 0.8f);
    public Vector2 scaleTo = new Vector2(1f, 1f);

    public Coroutine Fade(List<GameObject> objects, bool fadeIn, Action onComplete = null)
    {
        return StartCoroutine(FadeRoutine(objects, fadeIn, onComplete));
    }

    private IEnumerator FadeRoutine(List<GameObject> objects, bool fadeIn, Action onComplete)
    {
        float duration = fadeIn ? fadeInDuration : fadeOutDuration;
        AnimationCurve curve = fadeIn ? fadeInCurve : fadeOutCurve;

        List<Graphic> graphics = new List<Graphic>();
        List<Transform> transforms = new List<Transform>();

        foreach (var go in objects)
        {
            if (go.TryGetComponent(out Graphic g)) graphics.Add(g);
            transforms.Add(go.transform);
            if (fadeIn) go.SetActive(true);
        }

        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = curve.Evaluate(time / duration);
            float alpha = fadeIn ? t : 1f - t;

            foreach (var g in graphics)
            {
                Color c = g.material.color;
                c.a = alpha;
                g.material.color = c;
            }

            if (enableScaleEffect)
            {
                Vector3 scale = Vector3.Lerp(scaleFrom, scaleTo, alpha);
                foreach (var tr in transforms) tr.localScale = scale;
            }

            yield return null;
        }

        float finalAlpha = fadeIn ? 1f : 0f;
        foreach (var g in graphics)
        {
            Color c = g.material.color;
            c.a = finalAlpha;
            g.material.color = c;
        }

        if (!fadeIn)
        {
            foreach (var go in objects) go.SetActive(false);
        }

        onComplete?.Invoke();
    }
}
