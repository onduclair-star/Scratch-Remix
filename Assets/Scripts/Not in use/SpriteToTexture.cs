// Use to export a high-resolution texture from a sprite via a camera

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

    public void ExportSprite()
    {
        RenderTexture rt = new(outputWidth, outputHeight, 24);
        renderCamera.targetTexture = rt;
        renderCamera.Render();

        RenderTexture.active = rt;
        Texture2D tex = new(outputWidth, outputHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, outputWidth, outputHeight), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, bytes);
        Debug.Log("导出完成: " + path);

        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        DestroyImmediate(tex);
    }
}
