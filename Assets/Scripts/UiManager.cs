using UnityEngine;

public class UiManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private MenuView engineMenu;
    [SerializeField] private MenuView windowMenu;

    [System.Serializable]
    public class AreaPair
    {
        public string name;
        public GameObject area;
        public GameObject background;
    }

    [Header("Areas")]
    [SerializeField] private AreaPair[] areas;

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
            bool active = a.name == name;
            if (a.area) a.area.SetActive(active);
            if (a.background) a.background.SetActive(active);
        }
    }
}
