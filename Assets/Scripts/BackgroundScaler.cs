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
        if (cameraTransform)
            mainCamera = cameraTransform.GetComponent<Camera>();

        sr = bgTransform ? bgTransform.GetComponent<SpriteRenderer>() : null;
        if (!mainCamera) mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (!bgTransform || !sr || !sr.sprite || !mainCamera) return;

        // 跟随相机
        bgTransform.position = new Vector3(
            cameraTransform.position.x,
            cameraTransform.position.y,
            bgTransform.position.z);

        // 自适应缩放（自由裁切模式）
        float screenHeight = mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCamera.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;

        // 这里改为取较大的缩放因子，保证铺满整个屏幕，超出的部分会被裁切
        float scaleX = screenWidth / spriteSize.x;
        float scaleY = screenHeight / spriteSize.y;
        float finalScale = Mathf.Max(scaleX, scaleY);  // AspectFill 效果

        bgTransform.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}
