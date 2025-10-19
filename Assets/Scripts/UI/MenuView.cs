using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UIFadeController fadeController;
    public List<GameObject> fadeObjects = new();
    public GameObject highlight;

    public bool checkPointer;

    private bool isVisible;

    private void Awake()
    {
        highlight.SetActive(false);
        foreach (var go in fadeObjects) go.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (checkPointer)
            fadeController.Fade(fadeObjects, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (checkPointer)
            fadeController.Fade(fadeObjects, false);
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
