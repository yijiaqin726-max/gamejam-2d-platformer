using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class S2PrototypeBootstrap
{
    private const string SceneName = "S2_RomanColumns";
    private const string LegacyStartSceneName = "SampleScene";
    private const string RootName = "__S2_RomanColumns_Prototype";

#if UNITY_EDITOR
    [MenuItem("GameJam/Build S2 Prototype Preview")]
    private static void BuildFromMenu()
    {
        ClearExistingPreview();
        Build();
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BuildOnPlay()
    {
        if (ShouldBuildInScene(SceneManager.GetActiveScene().name))
        {
            Build();
        }
    }

    public static void Build()
    {
        if (GameObject.Find(RootName) != null)
        {
            return;
        }

        var root = new GameObject(RootName);
        CreateCamera(root.transform);
        CreateBackground(root.transform);
        CreateLevel(root.transform);
        CreatePlayer(root.transform);
    }

#if UNITY_EDITOR
    private static void ClearExistingPreview()
    {
        var existingRoot = GameObject.Find(RootName);
        if (existingRoot != null)
        {
            Object.DestroyImmediate(existingRoot);
        }
    }
#endif

    private static void CreateCamera(Transform parent)
    {
        var cameraObject = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
        cameraObject.transform.SetParent(parent);
        cameraObject.transform.position = new Vector3(-15f, 0.4f, -10f);
        var camera = cameraObject.GetComponent<Camera>() != null ? cameraObject.GetComponent<Camera>() : cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 3.2f;
        camera.backgroundColor = new Color(0.76f, 0.82f, 0.9f);
        cameraObject.tag = "MainCamera";
    }

    private static void CreateBackground(Transform parent)
    {
        CreateBox(parent, "sky fallback", new Vector2(0f, 0.9f), new Vector2(70f, 7f), new Color(0.66f, 0.79f, 0.92f), -30, false);
        CreateSprite(parent, "S2 blue temple background", "Assets/Art/Environment/RomanColumns/roman_columns_solid_blue_background.png", new Vector2(-3f, 0.8f), new Vector2(0.9f, 0.9f), -20);
    }

    private static void CreateLevel(Transform parent)
    {
        CreateBox(parent, "floor left", new Vector2(-14.5f, -2.15f), new Vector2(18f, 0.55f), new Color(0.09f, 0.1f, 0.11f), -3, true);
        CreateBox(parent, "floor right", new Vector2(12.5f, -2.15f), new Vector2(32f, 0.55f), new Color(0.09f, 0.1f, 0.11f), -3, true);
        CreateBox(parent, "pit floor", new Vector2(-3.8f, -2.95f), new Vector2(2.4f, 0.35f), new Color(0.24f, 0.25f, 0.26f), -2, true);

        CreateSprite(parent, "left ground art", "Assets/Art/Environment/RomanColumns/roman_columns_ground.png", new Vector2(-14.5f, -1.86f), new Vector2(0.85f, 0.18f), -2);
        CreateSprite(parent, "right ground art", "Assets/Art/Environment/RomanColumns/roman_columns_ground_grass.png", new Vector2(11.5f, -1.86f), new Vector2(1.1f, 0.18f), -2);

        AddColumnSet(parent, -9.5f, "roman_column_01.png", 0.38f);
        AddColumnSet(parent, -4.5f, "roman_columns_tree_trunk.png", 0.5f);
        CreateSprite(parent, "tree leaves", "Assets/Art/Environment/RomanColumns/roman_columns_tree_leaves.png", new Vector2(-4.5f, -0.4f), new Vector2(0.65f, 0.65f), 1);
        AddColumnSet(parent, 0.5f, "roman_columns_arch.png", 0.5f);
        AddColumnSet(parent, 6f, "roman_column_02.png", 0.42f);
        AddColumnSet(parent, 11f, "roman_column_broken.png", 0.4f);
        AddColumnSet(parent, 16f, "roman_column_thin.png", 0.42f);
        AddColumnSet(parent, 21f, "roman_columns_arch.png", 0.48f);
        AddColumnSet(parent, 28f, "roman_column_thin.png", 0.42f);

        CreateBox(parent, "pushable stone preview", new Vector2(-16.3f, -1.55f), new Vector2(0.8f, 0.8f), new Color(0.95f, 0.68f, 0.72f, 0.8f), 4, true);
        CreateSprite(parent, "stone art", "Assets/Art/Environment/RomanColumns/roman_columns_rolling_stone_animated.png", new Vector2(-16.3f, -1.55f), new Vector2(0.45f, 0.45f), 5);

        CreateLight(parent, -13f, -0.5f);
        CreateLight(parent, 0.5f, -0.25f);
        CreateLight(parent, 7.8f, -1.45f);
        CreateLight(parent, 21.5f, -0.4f);
        CreateLight(parent, 30f, -0.45f);
    }

    private static void AddColumnSet(Transform parent, float x, string fileName, float scale)
    {
        CreateSprite(parent, fileName, "Assets/Art/Environment/RomanColumns/" + fileName, new Vector2(x, -0.95f), new Vector2(scale, scale), 2);
    }

    private static void CreatePlayer(Transform parent)
    {
        var player = new GameObject("Player Prototype");
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(-18.7f, -1.45f, 0f);
        player.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        player.AddComponent<SpriteRenderer>().sortingOrder = 20;
        player.AddComponent<Rigidbody2D>().gravityScale = 2.4f;
        var collider = player.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.55f, 0.9f);

        var animator = player.AddComponent<PrototypeFrameAnimator>();
        animator.Configure(
            new[]
            {
                "Assets/Animations/Player/Idle/Frames/player_idle_01.png",
                "Assets/Animations/Player/Idle/Frames/player_idle_02.png",
                "Assets/Animations/Player/Idle/Frames/player_idle_03.png",
                "Assets/Animations/Player/Idle/Frames/player_idle_04.png"
            },
            new[]
            {
                "Assets/Animations/Player/RunLeftLegForward/Frames/player_run_left_leg_forward_01.png",
                "Assets/Animations/Player/RunLeftLegForward/Frames/player_run_left_leg_forward_02.png",
                "Assets/Animations/Player/RunLeftLegForward/Frames/player_run_left_leg_forward_03.png",
                "Assets/Animations/Player/RunRightLegForward/Frames/player_run_right_leg_forward_01.png",
                "Assets/Animations/Player/RunRightLegForward/Frames/player_run_right_leg_forward_02.png",
                "Assets/Animations/Player/RunRightLegForward/Frames/player_run_right_leg_forward_03.png"
            },
            new[]
            {
                "Assets/Animations/Player/Jump/Frames/player_jump_00.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_01.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_02.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_03.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_04.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_05.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_06.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_07.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_08.png"
            });
        player.AddComponent<PrototypePlayerController>();
    }

    private static void CreateLight(Transform parent, float x, float y)
    {
        CreateBox(parent, "light orb", new Vector2(x, y), new Vector2(0.35f, 0.35f), new Color(1f, 0.96f, 0.28f, 0.85f), 8, false);
    }

    private static void CreateSprite(Transform parent, string name, string path, Vector2 position, Vector2 scale, int sortingOrder)
    {
        var sprite = LoadSprite(path);
        if (sprite == null)
        {
            return;
        }

        var obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }

    private static void CreateBox(Transform parent, string name, Vector2 position, Vector2 scale, Color color, int sortingOrder, bool solid)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSquareSprite(color);
        renderer.sortingOrder = sortingOrder;

        if (solid)
        {
            obj.AddComponent<BoxCollider2D>();
        }
    }

    private static Sprite CreateSquareSprite(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private static Sprite LoadSprite(string path)
    {
#if UNITY_EDITOR
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        return texture != null
            ? Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f)
            : null;
#else
        return null;
#endif
    }

    private static bool ShouldBuildInScene(string sceneName)
    {
        // Prototype stage: build in any opened scene so Play works from Unity's default Untitled scene too.
        return true;
    }
}
