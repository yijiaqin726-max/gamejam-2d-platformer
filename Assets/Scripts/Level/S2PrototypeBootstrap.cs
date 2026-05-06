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
    private const float GroundTopY = -1.8f;

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
        cameraObject.transform.position = new Vector3(-16f, 0.1f, -10f);
        var camera = cameraObject.GetComponent<Camera>() != null ? cameraObject.GetComponent<Camera>() : cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 3.6f;
        camera.backgroundColor = new Color(0.76f, 0.82f, 0.9f);
        cameraObject.tag = "MainCamera";
    }

    private static void CreateBackground(Transform parent)
    {
        CreateBox(parent, "sky fallback", new Vector2(0f, 0.9f), new Vector2(70f, 7f), new Color(0.66f, 0.79f, 0.92f), -30, false);
        CreateSpriteHeight(parent, "S2 blue temple background", "Assets/Art/Environment/RomanColumns/roman_columns_solid_blue_background.png", new Vector2(12f, -0.25f), 7f, -20);
    }

    private static void CreateLevel(Transform parent)
    {
        CreateBoxTop(parent, "floor left", new Vector2(-15.4f, GroundTopY), new Vector2(17.2f, 0.6f), new Color(0.08f, 0.09f, 0.1f), -3, true);
        CreateBoxTop(parent, "floor right", new Vector2(8.8f, GroundTopY), new Vector2(29.6f, 0.6f), new Color(0.08f, 0.09f, 0.1f), -3, true);
        CreateBoxTop(parent, "pit bottom collider", new Vector2(-5f, GroundTopY - 1.25f), new Vector2(2.4f, 0.35f), new Color(0.18f, 0.19f, 0.2f), -3, true);

        CreateSpriteHeight(parent, "left ground art", "Assets/Art/Environment/RomanColumns/roman_columns_ground.png", new Vector2(-15.4f, GroundTopY), 2.5f, -2);
        CreateSpriteHeight(parent, "right ground art", "Assets/Art/Environment/RomanColumns/roman_columns_ground_grass.png", new Vector2(8.8f, GroundTopY), 2.5f, -2);
        CreateSpriteHeight(parent, "pit art", "Assets/Art/Environment/RomanColumns/roman_columns_pit.png", new Vector2(-5f, GroundTopY - 0.02f), 2.15f, -1);
        CreateSpriteHeight(parent, "pit floor art", "Assets/Art/Environment/RomanColumns/roman_columns_pit_floor.png", new Vector2(-5f, GroundTopY - 1.25f), 1.3f, -1);

        AddColumn(parent, -11.8f, "roman_columns_arch.png", 1.2f);
        AddColumn(parent, -7.5f, "roman_column_01.png", 1.95f);
        AddTree(parent, -1.6f, 2.35f);
        AddColumn(parent, 3.2f, "roman_columns_arch.png", 1.75f);
        AddColumn(parent, 8.8f, "roman_column_02.png", 2.1f);
        AddColumn(parent, 14.8f, "roman_column_broken.png", 1.75f);
        AddColumn(parent, 20.2f, "roman_columns_arch.png", 1.6f);
        AddColumn(parent, 26f, "roman_column_01.png", 1.75f);
        AddColumn(parent, 31.5f, "roman_column_thin.png", 1.95f);

        CreateBox(parent, "pushable stone preview", new Vector2(-14.7f, GroundTopY + 0.43f), new Vector2(0.75f, 0.75f), new Color(0.95f, 0.68f, 0.72f, 0.75f), 4, true);
        CreateSpriteHeight(parent, "stone art", "Assets/Art/Environment/RomanColumns/roman_columns_rolling_stone_animated.png", new Vector2(-14.7f, GroundTopY), 0.75f, 5);

        CreateLight(parent, -12.8f, GroundTopY + 1.45f);
        CreateLight(parent, -4.9f, GroundTopY - 0.35f);
        CreateLight(parent, -1.4f, GroundTopY + 2.75f);
        CreateLight(parent, 4.2f, GroundTopY + 1.7f);
        CreateLight(parent, 13.8f, GroundTopY + 1.35f);
        CreateLight(parent, 20.5f, GroundTopY + 1.9f);
        CreateLight(parent, 29.6f, GroundTopY + 1.9f);
    }

    private static void AddColumn(Transform parent, float x, string fileName, float height)
    {
        CreateSpriteHeight(parent, fileName, "Assets/Art/Environment/RomanColumns/" + fileName, new Vector2(x, GroundTopY), height, 2);
    }

    private static void AddTree(Transform parent, float x, float height)
    {
        CreateSpriteHeight(parent, "tree trunk", "Assets/Art/Environment/RomanColumns/roman_columns_tree_trunk.png", new Vector2(x, GroundTopY), height, 1);
        CreateSpriteHeight(parent, "tree leaves", "Assets/Art/Environment/RomanColumns/roman_columns_tree_leaves.png", new Vector2(x, GroundTopY + height * 0.45f), height * 1.1f, 2);
    }

    private static void CreatePlayer(Transform parent)
    {
        var player = new GameObject("Player Prototype");
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(-18.6f, GroundTopY + 0.47f, 0f);
        player.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        player.AddComponent<SpriteRenderer>().sortingOrder = 20;
        player.AddComponent<Rigidbody2D>().gravityScale = 2.4f;
        var collider = player.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.36f, 0.86f);

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
                "Assets/Animations/Player/RunLeftLegForward/Frames/player_run_left_leg_forward_04.png",
                "Assets/Animations/Player/RunLeftLegForward/Frames/player_run_left_leg_forward_05.png",
                "Assets/Animations/Player/RunRightLegForward/Frames/player_run_right_leg_forward_01.png",
                "Assets/Animations/Player/RunRightLegForward/Frames/player_run_right_leg_forward_02.png",
                "Assets/Animations/Player/RunRightLegForward/Frames/player_run_right_leg_forward_03.png",
                "Assets/Animations/Player/RunRightLegForward/Frames/player_run_right_leg_forward_04.png",
                "Assets/Animations/Player/RunRightLegForward/Frames/player_run_right_leg_forward_05.png"
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
        CreateBox(parent, "light orb", new Vector2(x, y), new Vector2(0.22f, 0.22f), new Color(1f, 0.96f, 0.28f, 0.9f), 8, false);
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

    private static void CreateBoxTop(Transform parent, string name, Vector2 topCenter, Vector2 scale, Color color, int sortingOrder, bool solid)
    {
        CreateBox(parent, name, new Vector2(topCenter.x, topCenter.y - scale.y * 0.5f), scale, color, sortingOrder, solid);
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

    private static void CreateSpriteHeight(Transform parent, string name, string path, Vector2 bottomCenter, float height, int sortingOrder)
    {
        var sprite = LoadSprite(path);
        if (sprite == null || sprite.bounds.size.y <= 0f)
        {
            return;
        }

        var obj = new GameObject(name);
        obj.transform.SetParent(parent);
        var scale = height / sprite.bounds.size.y;
        obj.transform.localScale = new Vector3(scale, scale, 1f);
        obj.transform.position = new Vector3(bottomCenter.x, bottomCenter.y + height * 0.5f, 0f);
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }

    private static bool ShouldBuildInScene(string sceneName)
    {
        // Prototype stage: build in any opened scene so Play works from Unity's default Untitled scene too.
        return true;
    }
}
