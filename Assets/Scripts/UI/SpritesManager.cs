using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class SpritesManager : MonoBehaviour
{
    public Task InitializationTask { get; private set; }
    public event System.Action SpritesReloaded;
    
    [HideInInspector] public List<Sprite> sprites = new();

    [SerializeField] private Transform spritesContainer;
    [SerializeField] private Vector2 defaultSpriteSize = new(100, 100);
    [SerializeField] private GameObject[] uiToSkip;
    [SerializeField] private Sprite closeIconSprite;

    private Font cachedCustomFont;

    void Awake()
    {
        InitializationTask = ReloadSprites();
    }

    public async Task ReloadSprites()
    {
        ClearUI();
        sprites.Clear();

        string spritesPath = GetSpritesPath();
        if (!Directory.Exists(spritesPath)) Directory.CreateDirectory(spritesPath);

        string[] files = Directory.GetFiles(spritesPath, "*.*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".shitbysr")).ToArray();

        List<SpriteLoadData> loadList = new();
        foreach (var file in files)
        {
            long ts = File.Exists(file + ".shitbysr") 
                ? JsonUtility.FromJson<FileMetadata>(File.ReadAllText(file + ".shitbysr")).importTimestamp 
                : File.GetCreationTime(file).Ticks;
            loadList.Add(new SpriteLoadData { filePath = file, timestamp = ts });
        }

        var sortedList = loadList.OrderBy(x => x.timestamp).ToList();

        for (int i = 0; i < sortedList.Count; i++)
        {
            string file = sortedList[i].filePath;
            byte[] data = await ReadFileAsync(file);
            if (data == null) continue;

            Texture2D tex = new(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data)) continue;

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            string fileName = Path.GetFileNameWithoutExtension(file);
            sprite.name = fileName;
            sprites.Add(sprite);
            
            CreateSpriteUI(sprite, fileName, i, file);
        }

        SpritesReloaded?.Invoke();
    }

    private void CreateSpriteUI(Sprite sprite, string name, int index, string filePath)
    {
        GameObject go = new(name);
        go.transform.SetParent(spritesContainer, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = defaultSpriteSize;

        rt.anchoredPosition = new Vector2((index % 8) * 100 + 75, -(index / 8) * 100 - 75);

        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;

        if (cachedCustomFont == null)
        {
            // IMPORTANT: Move font to Assets/Resources/Fonts/cangeqingyayuan.ttf
            cachedCustomFont = Resources.Load<Font>("Fonts/cangeqingyayuan");
        }

        SpriteHoverDelete hoverDelete = go.AddComponent<SpriteHoverDelete>();
        hoverDelete.Initialize(this, filePath, closeIconSprite, cachedCustomFont);
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

    private static string GetSpritesPath() => Path.Combine(Application.persistentDataPath, "Temp", "Sprites");

    private static async Task<byte[]> ReadFileAsync(string path)
    {
        try { return await Task.Run(() => File.ReadAllBytes(path)); }
        catch { return null; }
    }

    private struct SpriteLoadData { public string filePath; public long timestamp; }
    [System.Serializable] private struct FileMetadata { public long importTimestamp; }
}
