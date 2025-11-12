// Use for non-UI background images to always cover the camera view

using UnityEngine;

[ExecuteAlways]
public class BackgroundScaler : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform bgTransform;

    private SpriteRenderer sr;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = cameraTransform.GetComponent<Camera>();
        sr = bgTransform.GetComponent<SpriteRenderer>();
        if (!mainCamera) mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        bgTransform.position = new Vector3(
            cameraTransform.position.x,
            cameraTransform.position.y,
            bgTransform.position.z);

        float screenHeight = mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCamera.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;

        float scaleX = screenWidth / spriteSize.x;
        float scaleY = screenHeight / spriteSize.y;
        float finalScale = Mathf.Max(scaleX, scaleY);

        bgTransform.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}
