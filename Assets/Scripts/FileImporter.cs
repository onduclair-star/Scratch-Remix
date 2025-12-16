using UnityEngine;
using System.IO;

public static class FileImporter
{
    static string Root => Application.persistentDataPath;

    public static void ImportFile(string sourcePath)
    {
        if (!File.Exists(sourcePath))
            return;

        Directory.CreateDirectory(Root);

        string fileName = Path.GetFileName(sourcePath);
        string targetPath = Path.Combine(Root, fileName);

        File.Copy(sourcePath, targetPath, overwrite: true);

        Debug.Log($"[Import] {fileName} → {targetPath}");
    }

    public static void ImportDirectory(string sourceDir)
    {
        if (!Directory.Exists(sourceDir))
            return;

        string dirName = Path.GetFileName(sourceDir);
        string targetDir = Path.Combine(Root, dirName);

        CopyRecursive(sourceDir, targetDir);

        Debug.Log($"[Import Folder] {targetDir}");
    }

    static void CopyRecursive(string src, string dst)
    {
        Directory.CreateDirectory(dst);

        foreach (var file in Directory.GetFiles(src))
        {
            File.Copy(
                file,
                Path.Combine(dst, Path.GetFileName(file)),
                overwrite: true
            );
        }

        foreach (var dir in Directory.GetDirectories(src))
        {
            CopyRecursive(
                dir,
                Path.Combine(dst, Path.GetFileName(dir))
            );
        }
    }
}
