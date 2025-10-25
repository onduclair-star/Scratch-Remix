using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [HideInInspector]
    public static bool shouldShow = false;

    public GameObject toolbar;
    public MenuView engineMenu;
    public MenuView windowMenu;

    [System.Serializable]
    public class AreaPair
    {
        public string name;
        public GameObject area;
        public GameObject background;
    }

    public AreaPair[] areas;

    public bool IsAnyMenuOpen => engineMenu.IsVisible || windowMenu.IsVisible;

    void Update()
    {
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!IsPointerOverUI())
            {
                shouldShow = false;
                if (engineMenu.IsVisible)
                {
                    ToggleEngineMenu();
                }

                if (windowMenu.IsVisible)
                {
                    ToggleWindowMenu();
                }
            }
        }
    }

    private bool IsPointerOverUI()
    {
        PointerEventData pointerData = new(EventSystem.current)
        {
            position = UnityEngine.InputSystem.Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new();

        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            GameObject hitObject = result.gameObject;

            if (hitObject == toolbar || hitObject.transform.IsChildOf(toolbar.transform))
                return true;

            if (engineMenu.IsVisible && (hitObject == engineMenu.gameObject || hitObject.transform.IsChildOf(engineMenu.transform)))
                return true;
            if (windowMenu.IsVisible && (hitObject == windowMenu.gameObject || hitObject.transform.IsChildOf(windowMenu.transform)))
                return true;
        }

        return false;
    }

    public void ToggleEngineMenu()
    {
        if (engineMenu.IsVisible) engineMenu.Hide(onComplete: () => { engineMenu.transform.GetChild(1).gameObject.SetActive(false); });
        else engineMenu.Show(onComplete: () => { engineMenu.transform.GetChild(1).gameObject.SetActive(true); });
    }

    public void ToggleWindowMenu()
    {
        if (windowMenu.IsVisible) windowMenu.Hide(onComplete: () => { windowMenu.transform.GetChild(1).gameObject.SetActive(false); });
        else windowMenu.Show(onComplete: () => { windowMenu.transform.GetChild(1).gameObject.SetActive(true); });
    }

    public void ShowArea(string name)
    {
        foreach (var a in areas)
        {
            a.area.SetActive(a.name == name);
            a.background.SetActive(a.name == name);
        }
    }
}
