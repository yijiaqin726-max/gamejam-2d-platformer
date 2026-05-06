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
    private const float MapLeftX = 0f;
    private const float MapRightX = 60f;
    private const float CameraY = 0.1f;
    private const float CameraOrthoSize = 3.6f;
    private const float CameraAspect = 16f / 9f;
    private const float PlayerPixelsPerUnit = 128f;
    private const float PlayerScale = 0.5f;
    private const float BackgroundHeight = 9f;
    private const float BackgroundCenterY = 0.1f;

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
        var cameraTransform = CreateCamera(root.transform);
        CreateBackground(root.transform);
        CreateLevel(root.transform);
        var playerTransform = CreatePlayer(root.transform);
        AttachCameraFollow(cameraTransform, playerTransform);
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

    private static Transform CreateCamera(Transform parent)
    {
        var cameraObject = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
        cameraObject.transform.SetParent(parent);
        cameraObject.transform.position = new Vector3(MapLeftX + CameraOrthoSize * CameraAspect, CameraY, -10f);
        var camera = cameraObject.GetComponent<Camera>() != null ? cameraObject.GetComponent<Camera>() : cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = CameraOrthoSize;
        camera.backgroundColor = new Color(0.76f, 0.82f, 0.9f);
        cameraObject.tag = "MainCamera";
        return cameraObject.transform;
    }

    private static void CreateBackground(Transform parent)
    {
        var mapWidth = MapRightX - MapLeftX;
        var mapCenterX = (MapLeftX + MapRightX) * 0.5f;
        CreateBox(parent, "sky fallback", new Vector2(mapCenterX, BackgroundCenterY), new Vector2(mapWidth + 6f, BackgroundHeight), new Color(0.66f, 0.79f, 0.92f), -30, false);
        TileBackgroundArt(parent, "S2 blue temple background", "Assets/Art/Environment/RomanColumns/roman_columns_solid_blue_background.png", MapLeftX - 3f, MapRightX + 3f, BackgroundCenterY, BackgroundHeight);
    }

    private static void TileBackgroundArt(Transform parent, string name, string path, float startX, float endX, float centerY, float height)
    {
        var sprite = LoadSprite(path);
        if (sprite == null || sprite.bounds.size.x <= 0f || sprite.bounds.size.y <= 0f)
        {
            return;
        }

        var scale = height / sprite.bounds.size.y;
        var tileWidth = sprite.bounds.size.x * scale;
        if (tileWidth <= 0f)
        {
            return;
        }

        var cursor = startX;
        var index = 0;
        while (cursor < endX - 0.01f)
        {
            var center = cursor + tileWidth * 0.5f;
            CreateSpriteHeightCentered(parent, name + "_" + index, path, new Vector2(center, centerY), height, -20);
            cursor += tileWidth;
            index++;
        }
    }

    private static void CreateSpriteHeightCentered(Transform parent, string name, string path, Vector2 center, float height, int sortingOrder)
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
        obj.transform.position = new Vector3(center.x, center.y, 0f);
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }

    private static void CreateLevel(Transform parent)
    {
        var pit1CenterX = 19f;
        var pit2CenterX = 45f;
        var pitHalfWidth = 1.2f;
        var floorALeft = MapLeftX;
        var floorARight = pit1CenterX - pitHalfWidth;
        var floorBLeft = pit1CenterX + pitHalfWidth;
        var floorBRight = pit2CenterX - pitHalfWidth;
        var floorCLeft = pit2CenterX + pitHalfWidth;
        var floorCRight = MapRightX;

        AddFloorSegment(parent, "floor A", "Assets/Art/Environment/RomanColumns/roman_columns_ground.png", floorALeft, floorARight);
        AddFloorSegment(parent, "floor B", "Assets/Art/Environment/RomanColumns/roman_columns_ground_grass.png", floorBLeft, floorBRight);
        AddFloorSegment(parent, "floor C", "Assets/Art/Environment/RomanColumns/roman_columns_ground_grass.png", floorCLeft, floorCRight);

        AddPit(parent, "pit 1", pit1CenterX, pitHalfWidth);
        AddPit(parent, "pit 2", pit2CenterX, pitHalfWidth);

        CreateBoxTop(parent, "left wall", new Vector2(MapLeftX - 0.5f, GroundTopY + 4f), new Vector2(1f, 10f), new Color(0f, 0f, 0f, 0f), -3, true);
        CreateBoxTop(parent, "right wall", new Vector2(MapRightX + 0.5f, GroundTopY + 4f), new Vector2(1f, 10f), new Color(0f, 0f, 0f, 0f), -3, true);

        // Act 1 (0-12m): tutorial - arch entry, pushable stone, first columns.
        AddColumn(parent, 1.5f, "roman_columns_arch.png", 2.2f);
        AddColumn(parent, 5.5f, "roman_column_01.png", 1.6f);
        AddColumn(parent, 10.0f, "roman_column_01.png", 1.6f);

        // Act 2 (12-20m): climbable tree leading toward the first pit.
        AddTree(parent, 13.0f, 3.2f);
        AddColumn(parent, 17.0f, "roman_column_thin.png", 1.9f);

        // Act 3 (20-30m): transition arch + mixed columns after the pit.
        AddColumn(parent, 20.5f, "roman_columns_arch.png", 2.0f);
        AddColumn(parent, 23.5f, "roman_column_01.png", 1.6f);
        AddColumn(parent, 28.0f, "roman_columns_arch.png", 2.6f);

        // Act 4 (30-45m): gauntlet of columns, second tree break, lead into pit 2.
        AddColumn(parent, 32.0f, "roman_column_01.png", 1.6f);
        AddColumn(parent, 36.0f, "roman_column_02.png", 1.9f);
        AddColumn(parent, 39.0f, "roman_columns_arch.png", 2.0f);
        AddColumn(parent, 43.0f, "roman_column_01.png", 1.6f);

        // Act 5 (46-60m): final stretch - broken column + thin column + exit.
        AddColumn(parent, 48.0f, "roman_column_broken.png", 1.6f);
        AddColumn(parent, 52.0f, "roman_column_thin.png", 2.1f);
        AddColumn(parent, 57.0f, "roman_column_02.png", 1.9f);

        // Pushable stone in Act 1 entry zone.
        CreateBox(parent, "pushable stone preview", new Vector2(3.5f, GroundTopY + 0.43f), new Vector2(0.75f, 0.75f), new Color(0.95f, 0.68f, 0.72f, 0.75f), 4, true);
        CreateSpriteHeight(parent, "stone art", "Assets/Art/Environment/RomanColumns/roman_columns_rolling_stone_animated.png", new Vector2(3.5f, GroundTopY), 0.75f, 5);

        // Light orbs follow the forward path, lifting over obstacles.
        CreateLight(parent, 2.5f, GroundTopY + 1.8f);
        CreateLight(parent, 7.5f, GroundTopY + 1.5f);
        CreateLight(parent, 12.0f, GroundTopY + 1.8f);
        CreateLight(parent, 16.0f, GroundTopY + 1.6f);
        CreateLight(parent, 19.0f, GroundTopY - 0.35f);
        CreateLight(parent, 22.0f, GroundTopY + 2.5f);
        CreateLight(parent, 26.0f, GroundTopY + 1.8f);
        CreateLight(parent, 30.0f, GroundTopY + 2.9f);
        CreateLight(parent, 34.0f, GroundTopY + 1.5f);
        CreateLight(parent, 38.0f, GroundTopY + 1.8f);
        CreateLight(parent, 42.0f, GroundTopY + 1.6f);
        CreateLight(parent, 45.0f, GroundTopY - 0.35f);
        CreateLight(parent, 50.0f, GroundTopY + 1.7f);
        CreateLight(parent, 54.0f, GroundTopY + 2.5f);
        CreateLight(parent, 58.5f, GroundTopY + 1.8f);
    }

    private static void AddFloorSegment(Transform parent, string name, string artPath, float startX, float endX)
    {
        var width = endX - startX;
        if (width <= 0f)
        {
            return;
        }
        CreateBoxTop(parent, name + " collider", new Vector2(startX + width * 0.5f, GroundTopY), new Vector2(width, 0.6f), new Color(0f, 0f, 0f, 0f), -3, true);
        TileGroundArt(parent, name + " art", artPath, startX, endX, 2.5f);
    }

    private static void AddPit(Transform parent, string name, float centerX, float halfWidth)
    {
        CreateBoxTop(parent, name + " bottom collider", new Vector2(centerX, GroundTopY - 1.25f), new Vector2(halfWidth * 2f, 0.35f), new Color(0f, 0f, 0f, 0f), -3, true);
        CreateSpriteHeight(parent, name + " art", "Assets/Art/Environment/RomanColumns/roman_columns_pit.png", new Vector2(centerX, GroundTopY - 0.02f), 2.15f, -1);
        CreateSpriteHeight(parent, name + " floor art", "Assets/Art/Environment/RomanColumns/roman_columns_pit_floor.png", new Vector2(centerX, GroundTopY - 1.25f), 1.3f, -1);
    }

    private static void TileGroundArt(Transform parent, string name, string path, float startX, float endX, float height)
    {
        var sprite = LoadSprite(path);
        if (sprite == null || sprite.bounds.size.x <= 0f || sprite.bounds.size.y <= 0f)
        {
            return;
        }

        var scale = height / sprite.bounds.size.y;
        var tileWidth = sprite.bounds.size.x * scale;
        if (tileWidth <= 0f)
        {
            return;
        }

        var cursor = startX;
        var index = 0;
        while (cursor < endX - 0.01f)
        {
            var center = cursor + tileWidth * 0.5f;
            CreateSpriteHeight(parent, name + "_" + index, path, new Vector2(center, GroundTopY), height, -2);
            cursor += tileWidth;
            index++;
        }
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

    private static Transform CreatePlayer(Transform parent)
    {
        var player = new GameObject("Player Prototype");
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(MapLeftX + 3.5f, GroundTopY + 1.1f, 0f);
        player.transform.localScale = new Vector3(PlayerScale, PlayerScale, 1f);
        player.AddComponent<SpriteRenderer>().sortingOrder = 20;
        player.AddComponent<Rigidbody2D>().gravityScale = 2.4f;
        var collider = player.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.1f, 3.0f);
        collider.offset = new Vector2(0f, -0.2f);

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
                "Assets/Animations/Player/Jump/Frames/player_jump_05.png"
            },
            new[]
            {
                "Assets/Animations/Player/Jump/Frames/player_jump_06.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_07.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_08.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_09.png",
                "Assets/Animations/Player/Jump/Frames/player_jump_10.png"
            },
            new[]
            {
                "Assets/Animations/Player/RunToStop/Frames/player_run_to_stop_01.png",
                "Assets/Animations/Player/RunToStop/Frames/player_run_to_stop_02.png",
                "Assets/Animations/Player/RunToStop/Frames/player_run_to_stop_03.png",
                "Assets/Animations/Player/RunToStop/Frames/player_run_to_stop_04.png",
                "Assets/Animations/Player/RunToStop/Frames/player_run_to_stop_05.png"
            },
            PlayerPixelsPerUnit);
        player.AddComponent<PrototypePlayerController>();
        return player.transform;
    }

    private static void AttachCameraFollow(Transform cameraTransform, Transform playerTransform)
    {
        if (cameraTransform == null || playerTransform == null)
        {
            return;
        }

        var follow = cameraTransform.GetComponent<PrototypeCameraFollow>();
        if (follow == null)
        {
            follow = cameraTransform.gameObject.AddComponent<PrototypeCameraFollow>();
        }

        var halfWidth = CameraOrthoSize * CameraAspect;
        follow.Configure(playerTransform, CameraY, MapLeftX + halfWidth, MapRightX - halfWidth);
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
