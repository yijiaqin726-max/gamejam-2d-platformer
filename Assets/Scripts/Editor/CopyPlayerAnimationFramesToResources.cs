using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CopyPlayerAnimationFramesToResources
{
    private const string MenuPath = "Tools/Animation/Copy Player Frames To Resources";

    [MenuItem(MenuPath)]
    private static void CopyFrames()
    {
        int copiedCount = 0;

        copiedCount += CopyFolder("Assets/Animations/Player/Idle/Frames", "Assets/Resources/Animations/Player/Idle/Frames");
        copiedCount += CopyFolders(
            new[]
            {
                "Assets/Animations/Player/RunLeftLegForward/Frames",
                "Assets/Animations/Player/RunRightLegForward/Frames"
            },
            "Assets/Resources/Animations/Player/Run/Frames");
        copiedCount += CopyFolder("Assets/Animations/Player/Jump/Frames", "Assets/Resources/Animations/Player/Jump/Frames");
        copiedCount += CopyLandFrames();
        copiedCount += CopyFolderWithFallback(
            "Assets/Animations/Player/Turn/Frames",
            "Assets/Animations/Player/RunToStop/Frames",
            "Assets/Resources/Animations/Player/Turn/Frames");

        AssetDatabase.Refresh();
        Debug.Log("[CopyPlayerAnimationFramesToResources] Copied " + copiedCount + " file(s) to Assets/Resources/Animations/Player.");
    }

    private static int CopyLandFrames()
    {
        string landSource = "Assets/Animations/Player/Land/Frames";
        string landDestination = "Assets/Resources/Animations/Player/Land/Frames";
        if (Directory.Exists(landSource))
            return CopyFolder(landSource, landDestination);

        string jumpSource = "Assets/Animations/Player/Jump/Frames";
        IEnumerable<string> jumpLandFrames = GetImageFiles(jumpSource)
            .Where(path => Path.GetFileNameWithoutExtension(path).EndsWith("_06")
                || Path.GetFileNameWithoutExtension(path).EndsWith("_07")
                || Path.GetFileNameWithoutExtension(path).EndsWith("_08")
                || Path.GetFileNameWithoutExtension(path).EndsWith("_09")
                || Path.GetFileNameWithoutExtension(path).EndsWith("_10"));

        return CopyFiles(jumpLandFrames, landDestination);
    }

    private static int CopyFolderWithFallback(string primarySource, string fallbackSource, string destination)
    {
        if (Directory.Exists(primarySource))
            return CopyFolder(primarySource, destination);

        return CopyFolder(fallbackSource, destination);
    }

    private static int CopyFolders(IEnumerable<string> sources, string destination)
    {
        int copiedCount = 0;
        foreach (string source in sources)
            copiedCount += CopyFolder(source, destination);

        return copiedCount;
    }

    private static int CopyFolder(string source, string destination)
    {
        return CopyFiles(GetImageFiles(source), destination);
    }

    private static int CopyFiles(IEnumerable<string> sourceFiles, string destination)
    {
        EnsureFolder(destination);

        int copiedCount = 0;
        foreach (string sourceFile in sourceFiles)
        {
            string fileName = Path.GetFileName(sourceFile);
            string destinationFile = (destination + "/" + fileName).Replace("\\", "/");

            if (AssetDatabase.LoadAssetAtPath<Object>(destinationFile) != null)
                AssetDatabase.DeleteAsset(destinationFile);

            if (AssetDatabase.CopyAsset(sourceFile.Replace("\\", "/"), destinationFile))
                copiedCount++;
        }

        return copiedCount;
    }

    private static IEnumerable<string> GetImageFiles(string folder)
    {
        if (!Directory.Exists(folder))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
            .Where(path => path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
            .Select(path => path.Replace("\\", "/"));
    }

    private static void EnsureFolder(string folder)
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
    }
}
