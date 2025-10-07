using UnityEngine;
using System.IO;

public class SpriteToTexture : MonoBehaviour
{
    public SpriteRenderer targetSprite;
    public Camera renderCamera;
    public int outputWidth = 8192;
    public int outputHeight = 4608;
    public string fileName = "ExportedSprite.png";

    void Awake()
    {
        ExportSprite();
    }

    [ContextMenu("Export Sprite To PNG")]
    public void ExportSprite()
    {
        if (targetSprite == null || renderCamera == null)
        {
            Debug.LogError("请指定 targetSprite 和 renderCamera");
            return;
        }

        // 创建临时 RenderTexture
        RenderTexture rt = new(outputWidth, outputHeight, 24);
        renderCamera.targetTexture = rt;
        renderCamera.Render();

        // 读取像素
        RenderTexture.active = rt;
        Texture2D tex = new(outputWidth, outputHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, outputWidth, outputHeight), 0, 0);
        tex.Apply();

        // 保存到 PNG
        byte[] bytes = tex.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, bytes);
        Debug.Log("导出完成: " + path);

        // 清理
        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        DestroyImmediate(tex);
    }
}
