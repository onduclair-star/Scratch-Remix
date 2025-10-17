using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuView : MonoBehaviour
{
    public FadeController fadeController;
    public List<GameObject> fadeObjects = new();
    public GameObject highlight;

    private bool isVisible;

    private void Awake()
    {
        if (highlight) highlight.SetActive(false);
        foreach (var go in fadeObjects)
            if (go) go.SetActive(false);
    }

    public void Show(Action onComplete = null)
    {
        if (isVisible) return;
        isVisible = true;

        if (highlight) highlight.SetActive(true);
        fadeController.Fade(fadeObjects, true, onComplete);
    }

    public void Hide(Action onComplete = null)
    {
        if (!isVisible) return;
        isVisible = false;

        fadeController.Fade(fadeObjects, false, () =>
        {
            if (highlight) highlight.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public bool IsVisible => isVisible;
}
