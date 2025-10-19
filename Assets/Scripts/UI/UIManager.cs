using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
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

    public void ToggleEngineMenu()
    {
        if (engineMenu.IsVisible) engineMenu.Hide();
        else engineMenu.Show();
    }

    public void ToggleWindowMenu()
    {
        if (windowMenu.IsVisible) windowMenu.Hide();
        else windowMenu.Show();
    }

    public void ShowArea(string name)
    {
        foreach (var a in areas)
        {
            a.area.SetActive(a.name == name);
            a.background.SetActive(a.name == name);
        }
    }

    public List<GameObject> GetAllFadeObjects()
    {
        List<GameObject> result = new List<GameObject>();
        result.AddRange(engineMenu.fadeObjects);
        result.AddRange(windowMenu.fadeObjects);
        return result;
    }
}
