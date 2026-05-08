using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CleanMissingScriptsInScene
{
    private const string MenuPath = "Tools/Cleanup/Remove Missing Scripts In Open Scene";

    [MenuItem(MenuPath)]
    private static void RemoveMissingScriptsInOpenScene()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Remove Missing Scripts?",
            "This will remove missing script components from the currently open scene only.",
            "Remove",
            "Cancel");

        if (!confirmed)
            return;

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            Debug.LogWarning("[CleanMissingScriptsInScene] No valid open scene found.");
            return;
        }

        int cleanedGameObjectCount = 0;
        int removedMissingScriptCount = 0;
        List<string> cleanedPaths = new List<string>();

        GameObject[] roots = activeScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                GameObject gameObject = transforms[j].gameObject;
                int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (missingCount <= 0)
                    continue;

                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                cleanedGameObjectCount++;
                removedMissingScriptCount += missingCount;
                cleanedPaths.Add(GetHierarchyPath(gameObject));
            }
        }

        if (removedMissingScriptCount > 0)
            EditorSceneManager.MarkSceneDirty(activeScene);

        Debug.Log(
            $"[CleanMissingScriptsInScene] Cleaned {cleanedGameObjectCount} GameObject(s), removed {removedMissingScriptCount} Missing Script component(s).");

        for (int i = 0; i < cleanedPaths.Count; i++)
            Debug.Log("[CleanMissingScriptsInScene] " + cleanedPaths[i]);
    }

    private static string GetHierarchyPath(GameObject gameObject)
    {
        string path = gameObject.name;
        Transform current = gameObject.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
