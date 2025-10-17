using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ScreenOverlayManager : MonoBehaviour
{
    [Range(0.1f, 0.9f)]
    public float widthPercent = 0.2f;
    [Range(0.1f, 0.9f)]
    public float heightPercent = 0.4f;

    private SpriteRenderer overlaySprite;
    private Camera mainCamera;

    void Start()
    {
        CreateOverlaySprite();
    }

    void Awake()
    {
        overlaySprite = GetComponent<SpriteRenderer>();
    }

    void CreateOverlaySprite()
    {
        mainCamera = Camera.main;
        UpdateOverlayPosition();
    }

    void UpdateOverlayPosition()
    {
        float screenHeight = mainCamera.orthographicSize * 2;
        float screenWidth = screenHeight * mainCamera.aspect;

        float overlayWidth = screenWidth * widthPercent;
        float overlayHeight = screenHeight * heightPercent;

        float posX = mainCamera.transform.position.x + (screenWidth / 2) - (overlayWidth / 2);
        float posY = mainCamera.transform.position.y + (screenHeight / 2) - (overlayHeight / 2);

        overlaySprite.transform.position = new Vector3(posX, posY, mainCamera.transform.position.z + 1);
        overlaySprite.transform.localScale = new Vector3(overlayWidth, overlayHeight, 1);
    }

    void Update()
    {
        mainCamera = Camera.main;

        UpdateOverlayPosition();
    }
}