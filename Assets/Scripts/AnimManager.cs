using System.Collections.Generic;
using UnityEngine;

public class AnimManager : MonoBehaviour
{
    public static bool isLowQuality = false;

    [Header("References")]
    public UiManager uiManager;
    public FadeController fadeController;
    public RectTransform toolbar;
    public ToolbarHoverController hoverController;

    [SerializeField] private float menuExtraDelay = 1f;
    private float lastCloseTime = -Mathf.Infinity;
    private bool lastMenuOpen = false;

    private void Awake()
    {
        if (toolbar && hoverController)
            hoverController.Initialize(toolbar);
    }

    private void Update()
    {
        if (!uiManager) return;

        bool menuOpen = uiManager.IsAnyMenuOpen;

        if (isLowQuality)
        {
            // 低画质模式，简单 alpha 显示 toolbar 内部元素
            hoverController.UpdateToolbarPosition(menuOpen);
            return;
        }

        // 只对 toolbar 内部元素 Fade，不改父物体
        var fadeTargets = new List<GameObject>();
        foreach (Transform child in toolbar)
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
        hoverController.UpdateToolbarPosition(forceShow);
    }
}
