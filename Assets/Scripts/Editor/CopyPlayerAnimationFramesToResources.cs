using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CopyPlayerAnimationFramesToResources
{
    private const string MenuPath = "Tools/Animation/Copy Player Frames To Resources";
    private const string ResourceRoot = "Assets/Resources/Animations/Player";

    [MenuItem(MenuPath)]
    private static void CopyFrames()
    {
        int idleCount = CopyCategory(
            "Idle",
            new[] { "Assets/Animations/Player/Idle/Frames" },
            new[] { "idle" });

        int runCount = CopyCategory(
            "Run",
            new[]
            {
                "Assets/Animations/Player/Run/Frames",
                "Assets/Animations/Player/RunLeftLegForward/Frames",
                "Assets/Animations/Player/RunRightLegForward/Frames"
            },
            new[] { "run" });

        int jumpCount = CopyCategory(
            "Jump",
            new[] { "Assets/Animations/Player/Jump/Frames" },
            new[] { "jump" });

        int landCount = CopyCategory(
            "Land",
            new[] { "Assets/Animations/Player/Land/Frames" },
            new[] { "land" },
            GetJumpLandFallbackFiles());

        int turnCount = CopyCategory(
            "Turn",
            new[]
            {
                "Assets/Animations/Player/Turn/Frames",
                "Assets/Animations/Player/RunToStop/Frames"
            },
            new[] { "turn", "stop" });

        AssetDatabase.Refresh();
        ImportAsSingleSprites(ResourceRoot);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CopyPlayerAnimationFramesToResources] Idle copied = " + idleCount);
        Debug.Log("[CopyPlayerAnimationFramesToResources] Run copied = " + runCount);
        Debug.Log("[CopyPlayerAnimationFramesToResources] Jump copied = " + jumpCount);
        Debug.Log("[CopyPlayerAnimationFramesToResources] Land copied = " + landCount);
        Debug.Log("[CopyPlayerAnimationFramesToResources] Turn copied = " + turnCount);

        if (idleCount == 0)
            Debug.LogWarning("[CopyPlayerAnimationFramesToResources] Idle frames not found.");
        if (runCount == 0)
            Debug.LogWarning("[CopyPlayerAnimationFramesToResources] Run frames not found.");
        if (jumpCount == 0)
            Debug.LogWarning("[CopyPlayerAnimationFramesToResources] Jump frames not found.");
        if (landCount == 0)
            Debug.LogWarning("[CopyPlayerAnimationFramesToResources] Land frames not found.");
        if (turnCount == 0)
            Debug.LogWarning("[CopyPlayerAnimationFramesToResources] Turn frames not found.");
    }

    private static int CopyCategory(string category, string[] standardSources, string[] fallbackKeywords, IEnumerable<string> extraFallbackFiles = null)
    {
        string destination = ResourceRoot + "/" + category + "/Frames";
        EnsureFolder(destination);
        ClearDestinationImages(destination);

        List<string> sourceFiles = new List<string>();
        foreach (string source in standardSources)
            sourceFiles.AddRange(GetImageFiles(source, SearchOption.TopDirectoryOnly));

        if (sourceFiles.Count == 0)
            sourceFiles.AddRange(SearchKnownFallbackRoots(fallbackKeywords));

        if (sourceFiles.Count == 0)
            sourceFiles.AddRange(SearchAllAssetsByKeywords(fallbackKeywords));

        if (sourceFiles.Count == 0 && extraFallbackFiles != null)
            sourceFiles.AddRange(extraFallbackFiles);

        sourceFiles = sourceFiles
            .Distinct()
            .OrderBy(Path.GetFileNameWithoutExtension)
            .ToList();

        int copiedCount = 0;
        foreach (string sourceFile in sourceFiles)
        {
            string destinationFile = GetUniqueDestinationPath(destination, Path.GetFileName(sourceFile));
            if (AssetDatabase.CopyAsset(sourceFile, destinationFile))
                copiedCount++;
        }

        return copiedCount;
    }

    private static IEnumerable<string> SearchKnownFallbackRoots(string[] keywords)
    {
        string[] roots =
        {
            "Assets/Imported_S1/player",
            "Assets/Imported_S1/Player",
            "Assets/Imported_S1"
        };

        return roots.SelectMany(root => SearchFolderByKeywords(root, keywords));
    }

    private static IEnumerable<string> SearchAllAssetsByKeywords(string[] keywords)
    {
        return SearchFolderByKeywords("Assets", keywords);
    }

    private static IEnumerable<string> SearchFolderByKeywords(string folder, string[] keywords)
    {
        if (!Directory.Exists(folder))
            return Enumerable.Empty<string>();

        return GetImageFiles(folder, SearchOption.AllDirectories)
            .Where(path => keywords.Any(keyword => Path.GetFileNameWithoutExtension(path).ToLowerInvariant().Contains(keyword.ToLowerInvariant())
                || path.ToLowerInvariant().Contains("/" + keyword.ToLowerInvariant() + "/")));
    }

    private static IEnumerable<string> GetJumpLandFallbackFiles()
    {
        return GetImageFiles("Assets/Animations/Player/Jump/Frames", SearchOption.TopDirectoryOnly)
            .Where(path =>
            {
                string name = Path.GetFileNameWithoutExtension(path);
                return name.EndsWith("_06") || name.EndsWith("_07") || name.EndsWith("_08") || name.EndsWith("_09") || name.EndsWith("_10");
            });
    }

    private static IEnumerable<string> GetImageFiles(string folder, SearchOption searchOption)
    {
        if (!Directory.Exists(folder))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(folder, "*.*", searchOption)
            .Where(path => path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
            .Select(path => path.Replace("\\", "/"));
    }

    private static string GetUniqueDestinationPath(string destinationFolder, string fileName)
    {
        string destinationPath = destinationFolder + "/" + fileName;
        if (!File.Exists(destinationPath))
            return destinationPath;

        string name = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        int index = 1;

        do
        {
            destinationPath = destinationFolder + "/" + name + "_" + index + extension;
            index++;
        }
        while (File.Exists(destinationPath));

        return destinationPath;
    }

    private static void ImportAsSingleSprites(string root)
    {
        foreach (string imagePath in GetImageFiles(root, SearchOption.AllDirectories))
        {
            TextureImporter importer = AssetImporter.GetAtPath(imagePath) as TextureImporter;
            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128f;
            importer.SaveAndReimport();
        }
    }

    private static void EnsureFolder(string folder)
    {
        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }

    private static void ClearDestinationImages(string destination)
    {
        foreach (string imagePath in GetImageFiles(destination, SearchOption.TopDirectoryOnly))
            AssetDatabase.DeleteAsset(imagePath);
    }
}
