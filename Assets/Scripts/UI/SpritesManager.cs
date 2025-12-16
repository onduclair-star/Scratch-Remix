using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class SpritesManager : MonoBehaviour
{
    [HideInInspector]
    public List<Sprite> sprites = new();

    [SerializeField]
    private Transform spritesContainer;

    [SerializeField]
    private Vector2 defaultSpriteSize = new(100, 100);

    [SerializeField]
    private GameObject[] uiToSkip;


    async void Start()
    {
        await ReloadSprites();
    }

    /// <summary>
    /// 对外统一刷新入口（启动 / Import 后都用它）
    /// </summary>
    public async Task ReloadSprites()
    {
        ClearUI();
        sprites.Clear();

        string spritesPath = GetSpritesPath();
        if (!Directory.Exists(spritesPath))
            Directory.CreateDirectory(spritesPath);

        string[] files = Directory.GetFiles(spritesPath);

        foreach (var file in files)
        {
            byte[] data = await ReadFileAsync(file);
            if (data == null) continue;

            Texture2D tex = new(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data)) continue;

            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f
            );

            sprites.Add(sprite);
            CreateSpriteUI(sprite, Path.GetFileNameWithoutExtension(file));
        }
    }

    // /// <summary>
    // /// 对外统一刷新声音入口（启动 / Import 后都用它）
    // /// </summary>
    // public async Task ReloadAudios()
    // {
    //     // TODO: 爱咋地咋地，现在没用 :)
    // }

    private void CreateSpriteUI(Sprite sprite, string name)
    {
        GameObject go = new(name);
        go.transform.SetParent(spritesContainer, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = defaultSpriteSize;

        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
    }

    private void ClearUI()
    {
        HashSet<GameObject> skip = new(uiToSkip);

        for (int i = spritesContainer.childCount - 1; i >= 0; i--)
        {
            var child = spritesContainer.GetChild(i).gameObject;
            if (skip.Contains(child)) continue;

            Destroy(child);
        }
    }

    private static string GetSpritesPath()
    {
        return Path.Combine(
            Application.persistentDataPath,
            "ScratchRemix",
            "Sprites"
        );
    }

    private static async Task<byte[]> ReadFileAsync(string path)
    {
        try
        {
            return await Task.Run(() => File.ReadAllBytes(path));
        }
        catch (IOException e)
        {
            Debug.LogError($"[SpritesManager] Failed to load {path}\n{e}");
            return null;
        }
    }
}
