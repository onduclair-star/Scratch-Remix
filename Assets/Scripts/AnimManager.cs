using System;
using UnityEngine;

public class AnimManager : MonoBehaviour
{
    [HideInInspector] public static bool isLowQuality = false;

    public UiManager uiManager;
    public RectTransform toolbar;
    public FadeController fadeController;
    public ToolbarHoverController hoverController;

    [SerializeField] private float menuExtraDelay = 1f;

    private float menuClosedTime = -Mathf.Infinity;
    private bool lastMenuOpen = false;

    void Awake()
    {
        hoverController.Initialize(toolbar);
    }

    void Update()
    {
        if (isLowQuality)
        {
            bool mouseOver = RectTransformUtility.RectangleContainsScreenPoint(toolbar, Input.mousePosition, null);
            toolbar.gameObject.SetActive(mouseOver);
        }
        else
        {
            bool menuOpen = uiManager.IsAnyMenuOpen();

            if (lastMenuOpen && !menuOpen)
            {
                menuClosedTime = Time.time;
                fadeController.FadeOutGroup(0); // 假设菜单组是0
            }
            else if (!lastMenuOpen && menuOpen)
            {
                fadeController.FadeInGroup(0);
            }
            lastMenuOpen = menuOpen;

            bool forceShow = menuOpen || (Time.time - menuClosedTime < menuExtraDelay);
            hoverController.UpdateToolbarPosition(forceShow);
        }
    }

    public void StartFadeIn() => fadeController.FadeInGroup(0);
    public void StartFadeOut(Action onComplete = null) => fadeController.FadeOutGroup(0, onComplete);
}