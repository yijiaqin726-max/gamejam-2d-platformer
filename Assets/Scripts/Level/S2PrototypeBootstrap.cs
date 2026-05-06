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
    private const float CameraY = -0.05f;
    private const float CameraOrthoSize = 3.05f;
    private const float CameraAspect = 16f / 9f;
    private const float ScreenHeight = CameraOrthoSize * 2f;
    private const float GroundVisibleHeight = ScreenHeight * (1f / 10f);
    private const float GroundTopY = CameraY - CameraOrthoSize + GroundVisibleHeight;
    private const float GroundArtHeight = GroundVisibleHeight;
    private const float GrassHeight = 8.0f;
    private const float GrassClusterMinGap = 14.0f;
    private const float GrassClusterMaxGap = 26.0f;
    private const float GrassRootSink = 0.55f;
    private const float MapLeftX = -12f;
    private const float MapRightX = 60f;
    private const float PlayerPixelsPerUnit = 128f;
    private const float PlayerScale = 0.5f;
    private const float PlayerWorldWidth = 1.1f * PlayerScale;
    private const float PlayerWorldHeight = 3.0f * PlayerScale;
    private const float ColumnTargetWidth = PlayerWorldWidth * 7f;
    private const float TreeTopMargin = ScreenHeight * (1f / 10f);
    private const float MainTreeHeight = ScreenHeight - GroundVisibleHeight - TreeTopMargin;
    private const float TreeLeafBottomRatio = 0.18f;
    private const float TreeLeafHeightRatio = 0.78f;
    private const float TreeLeafCenterRatio = TreeLeafBottomRatio + TreeLeafHeightRatio * 0.5f;
    private const float TreeLeafVerticalOffset = -(MainTreeHeight * TreeLeafHeightRatio);
    private const float MainTreeLeafBottomHeight = MainTreeHeight * TreeLeafBottomRatio;
    private const float FullColumnHeight = MainTreeHeight * (2f / 3f);
    private const float BrokenColumnHeight = FullColumnHeight * 0.86f;
    private const float BrokenColumnColliderWidthRatio = 0.5f;
    private const float BrokenColumnColliderHeightRatio = 0.32f;
    private const float RockHeight = MainTreeLeafBottomHeight;
    private const int BehindPlayerGrassOrder = 8;
    private const int FrontPlayerGrassOrder = 24;
    private const int PillarSortingOrder = 30;
    private const int GroundSortingOrder = 40;
    private const float GroundedPropSink = 0.18f;
    private const float LowColumnHeight = 0.95f;
    private const float BackgroundHeight = 8.2f;
    private const float BackgroundCenterY = 0.0f;

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

    private static Transform CreateGroup(Transform parent, string name)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent);
        return obj.transform;
    }

    private static void CreateBackground(Transform parent)
    {
        var farBackground = CreateGroup(parent, "FarBackground");
        var midBackground = CreateGroup(parent, "MidBackground");
        var cloudBackground = CreateGroup(parent, "CloudBackground");
        var mapWidth = MapRightX - MapLeftX;
        var mapCenterX = (MapLeftX + MapRightX) * 0.5f;
        CreateBox(farBackground, "FarBackground watercolor fallback", new Vector2(mapCenterX, BackgroundCenterY), new Vector2(mapWidth + 6f, BackgroundHeight), new Color(0.70f, 0.86f, 0.95f, 1f), -100, false);
        TileBackgroundArt(farBackground, "FarBackground watercolor texture", "Assets/Art/Environment/RomanColumns/roman_columns_solid_blue_background.png", MapLeftX - 3f, MapRightX + 3f, BackgroundCenterY, BackgroundHeight);
        AddWatercolorClouds(cloudBackground);

        CreateSpriteHeight(midBackground, "MidBackground distant column", "Assets/Art/Environment/RomanColumns/roman_column_01.png", new Vector2(23f, GroundTopY), 5.2f, -55, new Color(0.33f, 0.55f, 1f, 0.42f));
        CreateSpriteHeight(midBackground, "MidBackground distant broken column", "Assets/Art/Environment/RomanColumns/roman_column_broken.png", new Vector2(27f, GroundTopY), 3.5f, -54, new Color(0.35f, 0.58f, 1f, 0.42f));
        CreateSpriteHeight(midBackground, "MidBackground distant ruin house", "Assets/Art/Environment/RomanColumns/roman_columns_house.png", new Vector2(52f, GroundTopY), 4.8f, -55, new Color(0.40f, 0.62f, 1f, 0.36f));
    }

    private static void AddWatercolorClouds(Transform parent)
    {
        AddWatercolorCloud(parent, -7.0f, 2.35f, 2.8f, 0.75f, -88);
        AddWatercolorCloud(parent, 2.2f, 2.75f, 3.6f, 0.95f, -88);
        AddWatercolorCloud(parent, 12.5f, 2.2f, 2.9f, 0.7f, -88);
        AddWatercolorCloud(parent, 24.0f, 2.9f, 4.2f, 1.0f, -88);
        AddWatercolorCloud(parent, 38.5f, 2.45f, 3.4f, 0.85f, -88);
        AddWatercolorCloud(parent, 53.0f, 2.8f, 3.8f, 0.95f, -88);
    }

    private static void AddWatercolorCloud(Transform parent, float x, float y, float width, float height, int sortingOrder)
    {
        var cloud = CreateGroup(parent, "watercolor cloud");
        var colorA = new Color(0.88f, 0.97f, 1f, 0.23f);
        var colorB = new Color(0.62f, 0.83f, 1f, 0.16f);
        AddCloudBlob(cloud, x - width * 0.32f, y - height * 0.02f, width * 0.34f, height * 0.78f, colorB, sortingOrder);
        AddCloudBlob(cloud, x - width * 0.12f, y + height * 0.08f, width * 0.46f, height * 0.95f, colorA, sortingOrder);
        AddCloudBlob(cloud, x + width * 0.15f, y + height * 0.02f, width * 0.52f, height, colorA, sortingOrder);
        AddCloudBlob(cloud, x + width * 0.38f, y - height * 0.05f, width * 0.36f, height * 0.74f, colorB, sortingOrder);
    }

    private static void AddCloudBlob(Transform parent, float x, float y, float width, float height, Color color, int sortingOrder)
    {
        var obj = new GameObject("cloud blob");
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(x, y, 0f);
        obj.transform.localScale = new Vector3(width, height, 1f);
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(color, true);
        renderer.sortingOrder = sortingOrder;
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
            CreateSpriteHeightCentered(parent, name + "_" + index, path, new Vector2(center, centerY), height, -95);
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
        var midgroundProps = CreateGroup(parent, "MidgroundProps");
        var foreground = CreateGroup(parent, "Foreground");
        var lightOrbs = CreateGroup(parent, "LightOrbs");
        var floorLeft = MapLeftX;
        var floorRight = MapRightX;

        // Ground is one continuous tiled SpriteRenderer using the real RomanColumns ground art.
        // No gaps and no pure-color filler blocks are generated.
        CreateTiledGroundArt(foreground, "continuous tiled ground sprite", "Assets/Art/Environment/RomanColumns/roman_columns_ground.png", floorLeft, floorRight, GroundArtHeight);
        AddReferenceGrassClusters(foreground);
        AddFloorCollider(parent, "continuous floor", floorLeft, floorRight);

        CreateBoxTop(parent, "left wall", new Vector2(MapLeftX - 0.5f, GroundTopY + 4f), new Vector2(1f, 10f), new Color(0f, 0f, 0f, 0f), -3, true);
        CreateBoxTop(parent, "right wall", new Vector2(MapRightX + 0.5f, GroundTopY + 4f), new Vector2(1f, 10f), new Color(0f, 0f, 0f, 0f), -3, true);

        AddReferenceSceneDressing(midgroundProps, foreground);

        AddStandableRock(parent, midgroundProps, 34.0f, RockHeight, 1.7f, "standable route rock");

        // Light orbs follow the forward path, lifting over obstacles.
        CreateLight(lightOrbs, 2.5f, GroundTopY + 1.8f);
        CreateLight(lightOrbs, 7.5f, GroundTopY + 1.5f);
        CreateLight(lightOrbs, 12.0f, GroundTopY + 1.8f);
        CreateLight(lightOrbs, 16.0f, GroundTopY + 1.6f);
        CreateLight(lightOrbs, 22.0f, GroundTopY + 2.5f);
        CreateLight(lightOrbs, 26.0f, GroundTopY + 1.8f);
        CreateLight(lightOrbs, 30.0f, GroundTopY + 2.9f);
        CreateLight(lightOrbs, 38.0f, GroundTopY + 1.8f);
        CreateLight(lightOrbs, 42.0f, GroundTopY + 1.6f);
        CreateLight(lightOrbs, 50.0f, GroundTopY + 1.7f);
        CreateLight(lightOrbs, 54.0f, GroundTopY + 2.5f);
        CreateLight(lightOrbs, 58.5f, GroundTopY + 1.8f);
    }

    private static void AddReferenceSceneDressing(Transform midground, Transform foreground)
    {
        AddGrassClump(foreground, 0.8f, BehindPlayerGrassOrder);
        AddGrassClump(foreground, 2.2f, FrontPlayerGrassOrder);
        AddGrassClump(foreground, 4.2f, BehindPlayerGrassOrder);
        AddTree(midground, foreground, -4.2f);
        AddStandableRock(midground, midground, -5.7f, RockHeight, 1.7f, "tree side rock");
        AddGrassClump(foreground, 10.8f, BehindPlayerGrassOrder);
        AddGrassClump(foreground, 12.5f, FrontPlayerGrassOrder);

        AddStandableRock(midground, midground, 13.2f, RockHeight, 1.7f, "low approach rock");
        AddColumn(midground, 18.0f, "roman_column_01.png", FullColumnHeight);
        AddBrokenColumnObstacle(midground, 20.8f);
        AddGrassClump(foreground, 14.2f, BehindPlayerGrassOrder);
        AddGrassClump(foreground, 19.2f, FrontPlayerGrassOrder);

        AddGrassClump(foreground, 23.0f, BehindPlayerGrassOrder);
        AddStandableRock(midground, midground, 25.8f, RockHeight, 1.7f, "mid route standable rock");
        AddColumn(midground, 30.8f, "roman_column_01.png", FullColumnHeight);
        AddBrokenColumnObstacle(midground, 34.4f);
        AddGrassClump(foreground, 29.2f, FrontPlayerGrassOrder);

        AddColumn(midground, 44.6f, "roman_column_01.png", FullColumnHeight);
        AddBrokenColumnObstacle(midground, 49.0f);
        AddGrassClump(foreground, 39.0f, BehindPlayerGrassOrder);
        AddGrassClump(foreground, 44.8f, FrontPlayerGrassOrder);

        CreateSpriteHeight(midground, "right ruin temple facade", "Assets/Art/Environment/RomanColumns/roman_columns_house.png", new Vector2(54.5f, GroundTopY), 4.6f, -15);
        AddStandableRock(midground, midground, 50.2f, RockHeight, 1.7f, "right approach standable rock");
        AddGrassClump(foreground, 51.8f, BehindPlayerGrassOrder);
        AddGrassClump(foreground, 57.5f, FrontPlayerGrassOrder);
    }

    private static void AddFloorCollider(Transform parent, string name, float startX, float endX)
    {
        var width = endX - startX;
        if (width <= 0f)
        {
            return;
        }
        CreateBoxTop(parent, name + " collider", new Vector2(startX + width * 0.5f, GroundTopY), new Vector2(width, GroundVisibleHeight), new Color(0f, 0f, 0f, 0f), -3, true);
    }

    private static void AddGrassStrip(Transform parent, float startX, float endX)
    {
        var width = endX - startX;
        if (width <= 0f)
        {
            return;
        }

        var sprite = LoadSprite("Assets/Art/Environment/RomanColumns/roman_columns_grass.png");
        if (sprite == null || sprite.bounds.size.y <= 0f)
        {
            return;
        }

        var clusterX = startX + 0.9f;
        var clusterIndex = 0;
        while (clusterX < endX - 0.01f)
        {
            var xJitter = Mathf.Lerp(-0.25f, 0.25f, PseudoRandom01(clusterIndex * 29 + 7));
            var sortingOrder = clusterIndex % 2 == 0 ? BehindPlayerGrassOrder : FrontPlayerGrassOrder;
            CreateSpriteHeight(parent, "grass_cluster_" + clusterIndex, "Assets/Art/Environment/RomanColumns/roman_columns_grass.png", new Vector2(clusterX + xJitter, GroundTopY - GrassRootSink), GrassHeight, sortingOrder);

            clusterX += Mathf.Lerp(GrassClusterMinGap, GrassClusterMaxGap, PseudoRandom01(clusterIndex * 31 + 7));
            clusterIndex++;
        }
    }

    private static void AddReferenceGrassClusters(Transform parent)
    {
        AddGrassClump(parent, -10.4f, BehindPlayerGrassOrder);
        AddGrassClump(parent, -6.7f, FrontPlayerGrassOrder);
        AddGrassClump(parent, 6.2f, BehindPlayerGrassOrder);
        AddGrassClump(parent, 11.6f, FrontPlayerGrassOrder);
        AddGrassClump(parent, 18.8f, BehindPlayerGrassOrder);
        AddGrassClump(parent, 27.5f, FrontPlayerGrassOrder);
        AddGrassClump(parent, 36.3f, BehindPlayerGrassOrder);
        AddGrassClump(parent, 47.2f, FrontPlayerGrassOrder);
        AddGrassClump(parent, 58.0f, BehindPlayerGrassOrder);
    }

    private static void AddColumn(Transform parent, float x, string fileName, float height)
    {
        CreateSpriteSize(parent, fileName, "Assets/Art/Environment/RomanColumns/" + fileName, new Vector2(x, GroundTopY - GroundedPropSink), new Vector2(ColumnTargetWidth, height), PillarSortingOrder);
        AddTopPlatform(parent, fileName + " standable top", x, GroundTopY + height - 0.05f, ColumnTargetWidth * 0.72f);
    }

    private static void AddBrokenColumnObstacle(Transform parent, float x)
    {
        CreateSpriteSize(parent, "roman_column_broken obstacle", "Assets/Art/Environment/RomanColumns/roman_column_broken.png", new Vector2(x, GroundTopY - GroundedPropSink), new Vector2(ColumnTargetWidth, BrokenColumnHeight), PillarSortingOrder);

        var obj = new GameObject("roman_column_broken obstacle collider");
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(x, GroundTopY + BrokenColumnHeight * BrokenColumnColliderHeightRatio * 0.5f, 0f);
        var box = obj.AddComponent<BoxCollider2D>();
        box.size = new Vector2(ColumnTargetWidth * BrokenColumnColliderWidthRatio, BrokenColumnHeight * BrokenColumnColliderHeightRatio);
    }

    private static void CreateTiledGroundArt(Transform parent, string name, string path, float startX, float endX, float height)
    {
        var sprite = LoadSprite(path);
        if (sprite == null)
        {
            return;
        }

        var width = endX - startX;
        if (width <= 0f)
        {
            return;
        }

        var obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(startX + width * 0.5f, GroundTopY - height * 0.5f, 0f);
        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = new Vector2(width, height);
        renderer.sortingOrder = GroundSortingOrder;
    }

    private static void AddStandableRock(Transform colliderParent, Transform visualParent, float x, float height, float topWidth, string name)
    {
        CreateSpriteHeight(visualParent, name + " visual", "Assets/Art/Environment/RomanColumns/roman_columns_rolling_stone_animated.png", new Vector2(x, GroundTopY - GroundedPropSink), height, 2);
        AddTopPlatform(colliderParent, name + " standable top", x, GroundTopY + height - 0.08f, topWidth);
    }

    private static void AddGrassClump(Transform parent, float x, int sortingOrder)
    {
        CreateSpriteHeight(parent, "grass clump", "Assets/Art/Environment/RomanColumns/roman_columns_grass.png", new Vector2(x, GroundTopY - GrassRootSink), GrassHeight, sortingOrder);
    }

    private static float PseudoRandom01(int seed)
    {
        var value = Mathf.Sin(seed * 12.9898f + 78.233f) * 43758.5453f;
        return value - Mathf.Floor(value);
    }

    private static void AddTree(Transform midground, Transform foreground, float x)
    {
        CreateSpriteHeight(midground, "large blue tree canopy", "Assets/Art/Environment/RomanColumns/roman_columns_tree_leaves.png", new Vector2(x, GroundTopY + MainTreeHeight * TreeLeafCenterRatio + TreeLeafVerticalOffset), MainTreeHeight * TreeLeafHeightRatio, -19);
        CreateSpriteHeight(midground, "large blue tree trunk", "Assets/Art/Environment/RomanColumns/roman_columns_tree_brunk.png", new Vector2(x, GroundTopY - GroundedPropSink), MainTreeHeight, -18);
        AddGrassClump(foreground, x - 1.0f, BehindPlayerGrassOrder);
        AddGrassClump(foreground, x + 1.2f, FrontPlayerGrassOrder);
    }

    private static void AddTopPlatform(Transform parent, string name, float x, float topY, float width)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = new Vector3(x, topY, 0f);
        var box = obj.AddComponent<BoxCollider2D>();
        box.size = new Vector2(width, 0.15f);
        box.usedByEffector = true;
        var effector = obj.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.surfaceArc = 160f;
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
        var glow = new GameObject("LightOrbPlaceholder glow");
        glow.transform.SetParent(parent);
        glow.transform.position = new Vector3(x, y, 0f);
        glow.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
        var glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = CreateCircleSprite(new Color(1f, 0.95f, 0.24f, 0.28f), true);
        glowRenderer.sortingOrder = 7;

        var orb = new GameObject("LightOrbPlaceholder");
        orb.transform.SetParent(parent);
        orb.transform.position = new Vector3(x, y, 0f);
        orb.transform.localScale = new Vector3(0.34f, 0.34f, 1f);
        var renderer = orb.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(new Color(1f, 0.86f, 0.05f, 1f), false);
        renderer.sortingOrder = 8;
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

    private static Sprite CreateCircleSprite(Color color, bool softEdge)
    {
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        var radius = size * 0.46f;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), center);
                var alpha = distance <= radius ? color.a : 0f;
                if (softEdge)
                {
                    alpha = Mathf.Clamp01(1f - distance / radius) * color.a;
                }

                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
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

    private static void CreateSpriteHeight(Transform parent, string name, string path, Vector2 bottomCenter, float height, int sortingOrder, Color color)
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
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    private static void CreateSpriteSize(Transform parent, string name, string path, Vector2 bottomCenter, Vector2 size, int sortingOrder)
    {
        var sprite = LoadSprite(path);
        if (sprite == null || sprite.bounds.size.x <= 0f || sprite.bounds.size.y <= 0f)
        {
            return;
        }

        var obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1f);
        obj.transform.position = new Vector3(bottomCenter.x, bottomCenter.y + size.y * 0.5f, 0f);
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
