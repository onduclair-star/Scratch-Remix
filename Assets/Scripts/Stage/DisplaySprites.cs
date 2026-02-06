using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpritesManager))]
public class DisplaySprites : MonoBehaviour
{
    public GameObject container;
    private SpritesManager spritesManager;

    async void Start()
    {
        spritesManager = GetComponent<SpritesManager>();
        spritesManager.SpritesReloaded += RebuildSprites;
        await spritesManager.InitializationTask;
        RebuildSprites();
    }

    private void OnDisable()
    {
        if (spritesManager != null)
        {
            spritesManager.SpritesReloaded -= RebuildSprites;
        }
    }

    private void RebuildSprites()
    {
        if (container == null || spritesManager == null) return;

        for (int i = container.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(container.transform.GetChild(i).gameObject);
        }

        foreach (var sprite in spritesManager.sprites)
        {
            GameObject go = new(sprite.name);
            go.transform.SetParent(container.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(100, 100);

            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;

            go.AddComponent<SpriteDragger>();
        }
    }
}
