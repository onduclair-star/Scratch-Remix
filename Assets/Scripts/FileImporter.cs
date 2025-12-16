using UnityEngine;
using System.IO;
using System.Linq;

public enum ImportType
{
    Image,
    Audio
}

public static class FileImporter
{
    static string Root => Path.Combine(Application.persistentDataPath, "ScratchRemix");
    static string SpriteRoot => Path.Combine(Root, "Sprites");
    static string AudioRoot  => Path.Combine(Root, "Audio");

    static readonly string[] SupportedImageExtensions =
    {
        ".png", ".jpg", ".jpeg", ".bmp", ".tga", ".tif", ".tiff",
        ".gif", ".psd", ".hdr", ".exr", ".ktx", ".pvr"
    };

    static readonly string[] SupportedAudioExtensions =
    {
        ".wav", ".ogg", ".mp3", ".aiff", ".mod", ".it", ".s3m", ".xm"
    };

    public static void ImportFile(string sourcePath, ImportType type)
    {
        if (!File.Exists(sourcePath))
            return;

        string extension = Path.GetExtension(sourcePath).ToLowerInvariant();

        string[] supportedExtensions;
        string targetRoot;
        string typeName;

        switch (type)
        {
            case ImportType.Image:
                supportedExtensions = SupportedImageExtensions;
                targetRoot = SpriteRoot;
                typeName = "image";
                break;

            case ImportType.Audio:
                supportedExtensions = SupportedAudioExtensions;
                targetRoot = AudioRoot;
                typeName = "audio";
                break;

            default:
                Debug.LogWarning($"[Import] Unknown import type: {type}");
                return;
        }

        if (!supportedExtensions.Contains(extension))
        {
            Debug.LogWarning($"[Import] Unsupported {typeName} format: {extension} ({sourcePath})");
            return;
        }

        Directory.CreateDirectory(targetRoot);

        string fileName = Path.GetFileName(sourcePath);
        string targetPath = Path.Combine(targetRoot, fileName);

        File.Copy(sourcePath, targetPath, overwrite: true);

        Debug.Log($"[Import {typeName}] {fileName} → {targetPath}");
    }

    // For future folder import UI
    public static void ImportDirectory(string sourceDir, ImportType type)
    {
        if (!Directory.Exists(sourceDir))
            return;

        string[] supportedExtensions;
        string targetRoot;
        string typeName;

        switch (type)
        {
            case ImportType.Image:
                supportedExtensions = SupportedImageExtensions;
                targetRoot = SpriteRoot;
                typeName = "image";
                break;

            case ImportType.Audio:
                supportedExtensions = SupportedAudioExtensions;
                targetRoot = AudioRoot;
                typeName = "audio";
                break;

            default:
                return;
        }

        string[] supportedFiles = Directory
            .GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)
            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToArray();

        if (supportedFiles.Length == 0)
        {
            Debug.LogWarning($"[Import Folder] No supported {typeName} files found in {sourceDir}");
            return;
        }

        string dirName = Path.GetFileName(sourceDir);
        string targetDir = Path.Combine(targetRoot, dirName);
        Directory.CreateDirectory(targetDir);

        int importedCount = 0;

        foreach (var file in supportedFiles)
        {
            string relativePath = Path.GetRelativePath(sourceDir, file);
            string targetFilePath = Path.Combine(targetDir, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
            File.Copy(file, targetFilePath, overwrite: true);
            importedCount++;
        }

        Debug.Log($"[Import Folder {typeName}] Imported {importedCount} files → {targetDir}");
    }
}
