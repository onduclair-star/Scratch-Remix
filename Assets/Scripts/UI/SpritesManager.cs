using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class SpritesManager : MonoBehaviour
{
    public Task InitializationTask { get; private set; }
    
    [HideInInspector]
    public List<Sprite> sprites = new();

    [SerializeField]
    private Transform spritesContainer;

    [SerializeField]
    private Vector2 defaultSpriteSize = new(100, 100);

    [SerializeField]
    private GameObject[] uiToSkip;

    void Awake()
    {
        // Assign the task immediately in Awake
        InitializationTask = ReloadSprites();
    }

    public async Task ReloadSprites()
    {
        ClearUI();
        sprites.Clear();

        string spritesPath = GetSpritesPath();
        if (!Directory.Exists(spritesPath))
            Directory.CreateDirectory(spritesPath);

        string[] files = Directory.GetFiles(spritesPath, "*.*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".shitbysr")).ToArray();

        List<SpriteLoadData> loadList = new();

        foreach (var file in files)
        {
            long ts = 0;
            string metaPath = file + ".shitbysr";
            if (File.Exists(metaPath))
            {
                var meta = JsonUtility.FromJson<FileMetadata>(File.ReadAllText(metaPath));
                ts = meta.importTimestamp;
            }
            else
            {
                ts = File.GetCreationTime(file).Ticks;
            }

            loadList.Add(new SpriteLoadData { filePath = file, timestamp = ts });
        }

        var sortedList = loadList.OrderBy(x => x.timestamp).ToList();

        int displayIndex = 0;
        for (int i = 0; i < sortedList.Count; i++)
        {
            string file = sortedList[i].filePath;
            byte[] data = await ReadFileAsync(file);
            
            Texture2D tex = new(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data)) continue;

            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f
            );

            string fileName = Path.GetFileNameWithoutExtension(file);
            sprite.name = fileName; // Set name so we can use it in DisplaySprites
            sprites.Add(sprite);
            
            CreateSpriteUI(sprite, fileName, displayIndex);
            displayIndex++;
        }
    }

    private void CreateSpriteUI(Sprite sprite, string name, int index)
    {
        GameObject go = new(name);
        go.transform.SetParent(spritesContainer, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = defaultSpriteSize;

        int n = index % 8; 
        int m = index / 8; 
        rt.anchoredPosition = new Vector2(n * 100 + 75, -m * 100 - 75);

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

    private static string GetSpritesPath() => Path.Combine(Application.persistentDataPath, "Temp", "Sprites");

    private static async Task<byte[]> ReadFileAsync(string path)
    {
        try { return await Task.Run(() => File.ReadAllBytes(path)); }
        catch (IOException) { return null; }
    }

    private struct SpriteLoadData { public string filePath; public long timestamp; }
}
