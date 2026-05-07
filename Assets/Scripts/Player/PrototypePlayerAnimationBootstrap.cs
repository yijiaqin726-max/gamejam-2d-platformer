using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Linq;
#endif

public sealed class PrototypePlayerAnimationBootstrap : MonoBehaviour
{
    private void Start()
    {
#if UNITY_EDITOR
        var frameAnimator = GetComponent<PrototypeFrameAnimator>();
        if (frameAnimator == null)
        {
            Debug.LogError("PrototypeFrameAnimator not found on this GameObject");
            return;
        }

        var idlePaths = CollectSpritePaths("Assets/Animations/Player/Idle/Frames");
        var runLeftPaths = CollectSpritePaths("Assets/Animations/Player/RunLeftLegForward/Frames");
        var runRightPaths = CollectSpritePaths("Assets/Animations/Player/RunRightLegForward/Frames");
        var runPaths = runLeftPaths.Concat(runRightPaths).ToArray();
        var jumpPaths = CollectSpritePaths("Assets/Animations/Player/Jump/Frames");
        var landPaths = CollectSpritePaths("Assets/Animations/Player/Land/Frames");
        var turnPaths = CollectSpritePaths("Assets/Animations/Player/Turn/Frames");

        if (idlePaths.Length == 0)
        {
            Debug.LogError("No Idle animation frames found in Assets/Animations/Player/Idle/Frames");
        }

        frameAnimator.Configure(idlePaths, runPaths, jumpPaths, landPaths, turnPaths);
#endif
    }

#if UNITY_EDITOR
    private static string[] CollectSpritePaths(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"Animation folder not found: {folderPath}");
            return System.Array.Empty<string>();
        }

        var files = Directory.GetFiles(folderPath, "*.png")
            .Concat(Directory.GetFiles(folderPath, "*.jpg"))
            .Concat(Directory.GetFiles(folderPath, "*.jpeg"))
            .ToArray();

        if (files.Length == 0)
        {
            Debug.LogWarning($"No image files found in {folderPath}");
            return System.Array.Empty<string>();
        }

        var assetPaths = files
            .Select(f => f.Replace("\\", "/"))
            .OrderBy(f => Path.GetFileNameWithoutExtension(f))
            .ToArray();

        return assetPaths;
    }
#endif
}
