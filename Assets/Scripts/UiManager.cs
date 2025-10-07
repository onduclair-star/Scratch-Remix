using UnityEngine;
using UnityEngine.InputSystem;

public class UiManager : MonoBehaviour
{
    [HideInInspector] public bool engineMenuOpen = false, windowMenuOpen = false;

    public AnimManager animManager;
    public GameObject settingsMenu, windowMenu;
    public GameObject settingsHighlight, windowHighlight;
    public RectTransform toolbarRt;

    void Awake()
    {
        settingsHighlight.SetActive(false);
        settingsMenu.SetActive(false);
        windowHighlight.SetActive(false);
        windowMenu.SetActive(false);
    }

    void Start()
    {
        settingsHighlight.transform.SetAsFirstSibling();
        windowHighlight.transform.SetAsFirstSibling();
    }

    void Update()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        if (!RectTransformUtility.RectangleContainsScreenPoint(toolbarRt, mousePos))
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (engineMenuOpen) ShowOrHideEngineMenu();
                if (windowMenuOpen) ShowOrHideWindowMenu();
            }
            else
            {
                if (!engineMenuOpen && settingsHighlight.activeSelf) settingsHighlight.SetActive(false);
                if (!windowMenuOpen && windowHighlight.activeSelf) windowHighlight.SetActive(false);
            }
        }
    }

    public bool IsAnyMenuOpen() => engineMenuOpen || windowMenuOpen;

    public void ShowOrHideEngineMenu()
    {
        engineMenuOpen = !engineMenuOpen;

        if (engineMenuOpen)
        {
            settingsHighlight.SetActive(true);
            settingsMenu.SetActive(true);
            animManager.StartFadeIn();
        }
        else
        {
            animManager.StartFadeOut(() =>
            {
                settingsMenu.SetActive(false);
                settingsHighlight.SetActive(false);
            });
        }
    }

    public void ShowOrHideWindowMenu()
    {
        windowMenuOpen = !windowMenuOpen;

        if (windowMenuOpen)
        {
            windowHighlight.SetActive(true);
            windowMenu.SetActive(true);
            animManager.StartFadeIn();
        }
        else
        {
            animManager.StartFadeOut(() =>
            {
                windowMenu.SetActive(false);
                windowHighlight.SetActive(false);
            });
        }
    }
}