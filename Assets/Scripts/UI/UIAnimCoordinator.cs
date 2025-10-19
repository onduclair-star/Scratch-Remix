using System.Collections.Generic;
using UnityEngine;

public class UIAnimCoordinator : MonoBehaviour
{
    public static bool isLowQuality = false;

    [Header("References")]
    public UIManager rootManager;
    public UIFadeController fadeController;
    public UIToolbarAnimator toolbarAnimator;

    public float menuExtraDelay = 1f;

    private float lastCloseTime = -Mathf.Infinity;
    private bool lastMenuOpen = false;

    private void Update()
    {
        bool menuOpen = rootManager.IsAnyMenuOpen;

        if (isLowQuality)
        {
            toolbarAnimator.SetVisible(menuOpen);
            return;
        }

        lastMenuOpen = menuOpen;

        bool forceShow = menuOpen || (Time.time - lastCloseTime < menuExtraDelay);
        toolbarAnimator.SetVisible(forceShow);
    }
}
