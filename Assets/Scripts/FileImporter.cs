using UnityEngine;
using System.IO;
using System.Linq;
using System;

public enum ImportType
{
    Image,
    Audio
}

[Serializable]
public class FileMetadata
{
    public long importTimestamp;
    public string originalName;
}

public static class FileImporter
{
    static string Root => Application.persistentDataPath;
    static string TempRoot => Path.Combine(Root, "Temp");
    static string ProjectsRoot => Path.Combine(Root, "Projects");

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
        string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
        string[] supportedExtensions;
        string subDir;

        switch (type)
        {
            case ImportType.Image:
                supportedExtensions = SupportedImageExtensions;
                subDir = "Sprites";
                break;
            case ImportType.Audio:
                supportedExtensions = SupportedAudioExtensions;
                subDir = "Audio";
                break;
            default:
                return;
        }

        if (!supportedExtensions.Contains(extension)) return;

        string targetDir = Path.Combine(TempRoot, subDir);
        Directory.CreateDirectory(targetDir);
        
        string fileName = Path.GetFileName(sourcePath);
        string targetPath = Path.Combine(targetDir, fileName);

        File.Copy(sourcePath, targetPath, overwrite: true);
        SaveMetadata(targetPath);
    }

    public static void ImportDirectory(string sourceDir, ImportType type)
    {
        string[] supportedExtensions;
        string subDir;

        switch (type)
        {
            case ImportType.Image:
                supportedExtensions = SupportedImageExtensions;
                subDir = "Sprites";
                break;
            case ImportType.Audio:
                supportedExtensions = SupportedAudioExtensions;
                subDir = "Audio";
                break;
            default:
                return;
        }

        string[] supportedFiles = Directory
            .GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)
            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToArray();

        if (supportedFiles.Length == 0) return;

        string dirName = Path.GetFileName(sourceDir);
        string targetDir = Path.Combine(TempRoot, subDir, dirName);
        Directory.CreateDirectory(targetDir);

        foreach (var file in supportedFiles)
        {
            string relativePath = Path.GetRelativePath(sourceDir, file);
            string targetFilePath = Path.Combine(targetDir, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
            File.Copy(file, targetFilePath, overwrite: true);
            SaveMetadata(targetFilePath);
        }
    }

    public static void FinalizeProject(string projectName)
    {
        if (!Directory.Exists(TempRoot)) return;

        string targetProjectDir = Path.Combine(ProjectsRoot, projectName);
        
        if (Directory.Exists(targetProjectDir))
            Directory.Delete(targetProjectDir, true);

        Directory.CreateDirectory(ProjectsRoot);
        // Use Move for efficiency
        Directory.Move(TempRoot, targetProjectDir);
        
        // Re-create TempRoot after move if needed for further imports
        Directory.CreateDirectory(TempRoot);
    }

    public static void ClearTemp()
    {
        if (Directory.Exists(TempRoot))
        {
            Directory.Delete(TempRoot, true);
        }
    }

    private static void SaveMetadata(string filePath)
    {
        FileMetadata meta = new()
        {
            importTimestamp = DateTime.Now.Ticks,
            originalName = Path.GetFileName(filePath)
        };
        string json = JsonUtility.ToJson(meta);
        File.WriteAllText(filePath + ".shitbysr", json);
    }
}
