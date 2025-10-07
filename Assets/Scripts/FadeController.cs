using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class FadeGroup
{
    public string groupName; // 可选，方便识别
    public List<GameObject> targets;
}

public class FadeController : MonoBehaviour
{
    [Header("Fade Groups")]
    [SerializeField] private List<FadeGroup> fadeGroups;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scale Effect")]
    [SerializeField] private bool enableScaleEffect = true;
    [SerializeField] private Vector2 scaleFrom = new Vector2(0.8f, 0.8f);
    [SerializeField] private Vector2 scaleTo = new Vector2(1f, 1f);

    private readonly List<Material> currentMaterials = new();
    private readonly List<Transform> currentTransforms = new();
    private float currentAlpha = 0f;

    private Coroutine fadeCoroutine;

    // 淡入指定组
    public void FadeInGroup(int groupIndex) => StartFadeGroup(groupIndex, true);

    // 淡出指定组
    public void FadeOutGroup(int groupIndex, Action onComplete = null) => StartFadeGroup(groupIndex, false, onComplete);

    // 淡入淡出组
    private void StartFadeGroup(int groupIndex, bool fadeIn, Action onComplete = null)
    {
        if (groupIndex < 0 || groupIndex >= fadeGroups.Count) return;

        var group = fadeGroups[groupIndex]?.targets;
        if (group == null || group.Count == 0) return;

        // 清空列表
        currentMaterials.Clear();
        currentTransforms.Clear();

        foreach (var go in group)
        {
            if (go == null) continue;

            if (go.TryGetComponent<Renderer>(out var r)) currentMaterials.Add(r.material);
            else if (go.TryGetComponent<Image>(out var img)) currentMaterials.Add(img.material);

            currentTransforms.Add(go.transform);
            if (fadeIn) go.SetActive(true);
        }

        // 安全 Coroutine，不 Stop 之前的，允许淡入淡出同时进行
        fadeCoroutine = StartCoroutine(FadeRoutine(fadeIn, onComplete));
    }

    // 单个对象淡入淡出
    public void FadeObject(GameObject go, bool fadeIn, float duration)
    {
        if (go == null) return;
        StartCoroutine(FadeGORoutine(go, fadeIn, duration));
    }

    private IEnumerator FadeGORoutine(GameObject go, bool fadeIn, float duration)
    {
        Material mat = null;
        if (go.TryGetComponent<Renderer>(out var r)) mat = r.material;
        else if (go.TryGetComponent<Image>(out var img)) mat = img.material;

        if (mat == null)
        {
            go.SetActive(fadeIn);
            yield break;
        }

        if (fadeIn) go.SetActive(true);

        float startAlpha = mat.GetColor("_Color").a;
        float targetAlpha = fadeIn ? 1f : 0f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;
            Color c = mat.GetColor("_Color");
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            mat.SetColor("_Color", c);
            yield return null;
        }

        Color final = mat.GetColor("_Color");
        final.a = targetAlpha;
        mat.SetColor("_Color", final);

        if (!fadeIn) go.SetActive(false);
    }

    private IEnumerator FadeRoutine(bool fadeIn, Action onComplete = null)
    {
        float duration = fadeIn ? fadeInDuration : fadeOutDuration;
        AnimationCurve curve = fadeIn ? fadeInCurve : fadeOutCurve;

        float startAlpha = currentAlpha;
        float targetAlpha = fadeIn ? 1f : 0f;

        Vector3 startScale = scaleFrom;
        Vector3 endScale = scaleTo;

        if (!fadeIn && enableScaleEffect && currentTransforms.Count > 0)
        {
            startScale = currentTransforms[0].localScale;
            endScale = scaleFrom * 0.9f;
        }

        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = curve.Evaluate(time / duration);

            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));

            if (enableScaleEffect)
            {
                for (int i = 0; i < currentTransforms.Count; i++)
                {
                    if (currentTransforms[i] != null)
                        currentTransforms[i].localScale = Vector3.Lerp(startScale, endScale, t);
                }
            }

            yield return null;
        }

        SetAlpha(targetAlpha);

        // 💡 FadeOut 安全关闭对象
        if (!fadeIn)
        {
            foreach (var tr in currentTransforms)
            {
                if (tr != null && tr.gameObject != null)
                    tr.gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        }
    }

    private void SetAlpha(float alpha)
    {
        currentAlpha = alpha;
        foreach (var mat in currentMaterials)
        {
            if (mat != null && mat.HasProperty("_Color"))
            {
                Color c = mat.GetColor("_Color");
                c.a = alpha;
                mat.SetColor("_Color", c);
            }
        }
    }
}