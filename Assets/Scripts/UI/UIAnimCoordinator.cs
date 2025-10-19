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

        var fadeTargets = new List<GameObject>();
        foreach (Transform child in toolbarAnimator.transform)
            fadeTargets.Add(child.gameObject);

        if (lastMenuOpen && !menuOpen)
        {
            lastCloseTime = Time.time;
            fadeController.Fade(fadeTargets, false);
        }
        else if (!lastMenuOpen && menuOpen)
        {
            fadeController.Fade(fadeTargets, true);
        }

        lastMenuOpen = menuOpen;

        bool forceShow = menuOpen || (Time.time - lastCloseTime < menuExtraDelay);
        toolbarAnimator.SetVisible(forceShow);
    }
}
