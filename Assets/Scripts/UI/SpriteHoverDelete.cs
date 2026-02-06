using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpriteHoverDelete : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite closeIconSprite;
    [SerializeField] private Vector2 closeButtonSize = new(24, 24);
    [SerializeField] private Vector2 closeButtonPadding = new(6, 6);
    [SerializeField] private Color overlayColor = new(0, 0, 0, 0.55f);
    [SerializeField] private Vector2 dialogSize = new(320, 170);

    private SpritesManager spritesManager;
    private string spriteFilePath;
    private GameObject closeButtonGo;
    private GameObject dialogGo;
    private RectTransform cachedRect;
    private static Sprite roundedSprite;
    private Font customFont; 
    private bool isDeleting;

    public void Initialize(SpritesManager manager, string filePath, Sprite closeIcon, Font font = null)
    {
        spritesManager = manager;
        spriteFilePath = filePath;
        if (closeIcon != null)
        {
            closeIconSprite = closeIcon;
        }
        customFont = font;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDeleting) return;

        if (closeButtonGo == null)
        {
            CreateCloseButton();
        }
        else
        {
            closeButtonGo.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (dialogGo != null && dialogGo.activeSelf) return;

        cachedRect = cachedRect ? cachedRect : GetComponent<RectTransform>();
        if (RectTransformUtility.RectangleContainsScreenPoint(cachedRect, eventData.position, eventData.enterEventCamera))
        {
            return;
        }

        if (closeButtonGo != null)
        {
            closeButtonGo.SetActive(false);
        }
    }

    private void CreateCloseButton()
    {
        closeButtonGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeButtonGo.transform.SetParent(transform, false);

        RectTransform rt = closeButtonGo.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one;
        rt.sizeDelta = closeButtonSize;
        rt.anchoredPosition = new Vector2(-closeButtonPadding.x, -closeButtonPadding.y);

        Image img = closeButtonGo.GetComponent<Image>();
        img.raycastTarget = true;
        if (closeIconSprite != null)
        {
            img.sprite = closeIconSprite;
            img.preserveAspect = true;
        }
        else
        {
            img.sprite = GetRoundedSprite(12);
            img.type = Image.Type.Sliced;
            img.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            AddTextElement(closeButtonGo.transform, "X", 14, Color.black);
        }

        closeButtonGo.GetComponent<Button>().onClick.AddListener(ShowConfirmDialog);
    }

    private void ShowConfirmDialog()
    {
        if (dialogGo != null)
        {
            dialogGo.SetActive(true);
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        dialogGo = new GameObject("DeleteSpriteDialog", typeof(RectTransform), typeof(Image));
        dialogGo.transform.SetParent(canvas.transform, false);

        RectTransform overlayRt = dialogGo.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.sizeDelta = Vector2.zero;
        dialogGo.GetComponent<Image>().color = overlayColor;

        GameObject panel = new("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(dialogGo.transform, false);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.sizeDelta = dialogSize;
        panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);

        Image panelImg = panel.GetComponent<Image>();
        panelImg.sprite = GetRoundedSprite(20);
        panelImg.type = Image.Type.Sliced;

        AddTextElement(panel.transform, $"Delete \"{gameObject.name}\"?", 18, Color.black, new Vector2(0, 25));

        CreateDialogButton(panel.transform, "Delete", new Vector2(0.3f, 0.25f), new Color(1f, 0.45f, 0.45f)).onClick.AddListener(() => { _ = ConfirmDeleteAsync(); });
        CreateDialogButton(panel.transform, "Cancel", new Vector2(0.7f, 0.25f), new Color(0.9f, 0.9f, 0.9f)).onClick.AddListener(() => dialogGo.SetActive(false));
    }

    private async Task ConfirmDeleteAsync()
    {
        if (isDeleting) return;
        isDeleting = true;

        if (dialogGo != null) dialogGo.SetActive(false);
        if (closeButtonGo != null) closeButtonGo.SetActive(false);

        if (!string.IsNullOrEmpty(spriteFilePath))
        {
            await Task.Run(() =>
            {
                if (File.Exists(spriteFilePath)) File.Delete(spriteFilePath);
                string metaPath = spriteFilePath + ".shitbysr";
                if (File.Exists(metaPath)) File.Delete(metaPath);
            });
        }

        if (spritesManager != null) await spritesManager.ReloadSprites();
        else Destroy(gameObject);

        isDeleting = false;
    }

    private void AddTextElement(Transform parent, string text, int size, Color color, Vector2 offset = default)
    {
        GameObject textGo = new("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(parent, false);
        RectTransform rt = textGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = offset;

        Text uiText = textGo.GetComponent<Text>();
        uiText.text = text;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = customFont != null ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.fontSize = size;
        uiText.color = color;
    }

    private Button CreateDialogButton(Transform parent, string label, Vector2 anchor, Color btnColor)
    {
        GameObject buttonGo = new(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(parent, false);
        RectTransform rt = buttonGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 36);
        rt.anchorMin = rt.anchorMax = anchor;

        Image img = buttonGo.GetComponent<Image>();
        img.sprite = GetRoundedSprite(10);
        img.type = Image.Type.Sliced;
        img.color = btnColor;

        AddTextElement(buttonGo.transform, label, 16, Color.black);
        return buttonGo.GetComponent<Button>();
    }

    private Sprite GetRoundedSprite(int radius)
    {
        if (roundedSprite != null) return roundedSprite;
        const int size = 64;
        Texture2D tex = new(size, size, TextureFormat.RGBA32, false);
        Color[] cols = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                cols[y * size + x] = IsInside(x, y, size, radius) ? Color.white : Color.clear;
        tex.SetPixels(cols);
        tex.Apply();
        return roundedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, 100f, 0, SpriteMeshType.FullRect, Vector4.one * radius);
    }

    private bool IsInside(int x, int y, int s, int r)
    {
        if (x < r && y < r) return (x - r) * (x - r) + (y - r) * (y - r) <= r * r;
        if (x > s - r && y < r) return (x - (s - r)) * (x - (s - r)) + (y - r) * (y - r) <= r * r;
        if (x < r && y > s - r) return (x - r) * (x - r) + (y - (s - r)) * (y - (s - r)) <= r * r;
        if (x > s - r && y > s - r) return (x - (s - r)) * (x - (s - r)) + (y - (s - r)) * (y - (s - r)) <= r * r;
        return true;
    }

    private void OnDestroy() { if (dialogGo != null) Destroy(dialogGo); }
}
