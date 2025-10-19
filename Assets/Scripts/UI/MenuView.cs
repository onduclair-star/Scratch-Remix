using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuView : MonoBehaviour
{

    public UIFadeController fadeController;
    public List<GameObject> fadeObjects = new();
    public GameObject highlight;

    private bool isVisible;

    private void Awake()
    {
        highlight.SetActive(false);
        foreach (var go in fadeObjects) go.SetActive(false);
    }

    public void Show(Action onComplete = null)
    {
        isVisible = true;
        highlight.SetActive(true);
        fadeController.Fade(fadeObjects, true, onComplete);
        UIManager.shouldShow = true;
    }

    public void Hide(Action onComplete = null)
    {
        isVisible = false;
        fadeController.Fade(fadeObjects, false, () =>
        {
            highlight.SetActive(false);
            onComplete?.Invoke();
        });
        UIManager.shouldShow = false;
    }

    public bool IsVisible => isVisible;
}
