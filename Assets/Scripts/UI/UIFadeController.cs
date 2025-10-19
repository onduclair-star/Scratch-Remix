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
    public Vector2 scaleFrom = new(0.8f, 0.8f);
    public Vector2 scaleTo = new(1f, 1f);

    private Coroutine currentFade;
    private float currentAlpha = 0f;

    public Coroutine Fade(List<GameObject> objects, bool fadeIn, Action onComplete = null)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(objects, fadeIn, onComplete));
        return currentFade;
    }

    private IEnumerator FadeRoutine(List<GameObject> objects, bool fadeIn, Action onComplete)
    {
        float duration = fadeIn ? fadeInDuration : fadeOutDuration;
        AnimationCurve curve = fadeIn ? fadeInCurve : fadeOutCurve;

        List<Graphic> graphics = new();
        List<Transform> transforms = new();

        foreach (var go in objects)
        {
            if (go.TryGetComponent(out Graphic g)) graphics.Add(g);
            transforms.Add(go.transform);
            if (fadeIn) go.SetActive(true);
        }

        float targetAlpha = fadeIn ? 1f : 0f;
        float startAlpha = currentAlpha;

        float alphaDistance = Mathf.Abs(targetAlpha - startAlpha);
        float adjustedDuration = duration * alphaDistance;

        float time = 0f;
        while (time < adjustedDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = curve.Evaluate(time / adjustedDuration);
            currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            foreach (var g in graphics)
            {
                Color c = g.material.color;
                c.a = currentAlpha;
                g.material.color = c;
            }

            if (enableScaleEffect)
            {
                Vector3 scale = Vector3.Lerp(scaleFrom, scaleTo, currentAlpha);
                foreach (var tr in transforms) tr.localScale = scale;
            }

            yield return null;
        }

        currentAlpha = targetAlpha;
        foreach (var g in graphics)
        {
            Color c = g.material.color;
            c.a = currentAlpha;
            g.material.color = c;
        }

        if (!fadeIn)
        {
            foreach (var go in objects) go.SetActive(false);
        }

        onComplete?.Invoke();
        currentFade = null;
    }
}
