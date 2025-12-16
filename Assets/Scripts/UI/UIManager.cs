using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SimpleFileBrowser;

[RequireComponent(typeof(SpritesManager))]
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

    private SpritesManager spritesManager;

    void Awake()
    {
        spritesManager = GetComponent<SpritesManager>();
    }

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

    public void OnClickImportSprites()
    {
        FileBrowser.SetFilters(
            showAllFilesFilter: false,
            new FileBrowser.Filter("Image Files", ".png", ".jpg", ".jpeg", ".bmp", ".tga")
        );
        FileBrowser.SetDefaultFilter("Image Files");

        FileBrowser.ShowLoadDialog(
            onSuccess: async paths =>
            {
                if (paths == null || paths.Length == 0) return;

                foreach (var path in paths)
                {
                    FileImporter.ImportFile(path, ImportType.Image);
                }

                await spritesManager.ReloadSprites();
            },
            onCancel: () => { },
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: true,
            title: "Import Sprites",
            loadButtonText: "Import"
        );
    }

    public void OnClickImportAudio()
    {
        FileBrowser.SetFilters(
            showAllFilesFilter: false,
            new FileBrowser.Filter("Audio Files", ".wav", ".ogg", ".mp3", ".aiff")
        );
        FileBrowser.SetDefaultFilter("Audio Files");

        FileBrowser.ShowLoadDialog(
            onSuccess: paths =>
            {
                if (paths == null || paths.Length == 0) return;
                foreach (var path in paths)
                {
                    FileImporter.ImportFile(path, ImportType.Audio);
                }

                // await spritesManager.ReloadAudios();
            },
            onCancel: () => { },
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: true,
            initialPath: null,
            initialFilename: null,
            title: "Import Audio",
            loadButtonText: "Import"
        );
    }
}
