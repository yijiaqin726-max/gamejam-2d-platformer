using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class PrototypeFrameAnimator : MonoBehaviour
{
    private const string IdleResourcePath = "Animations/Player/Idle/Frames";
    private const string RunResourcePath = "Animations/Player/Run/Frames";
    private const string JumpResourcePath = "Animations/Player/Jump/Frames";
    private const string LandResourcePath = "Animations/Player/Land/Frames";
    private const string TurnResourcePath = "Animations/Player/Turn/Frames";

    private static readonly string[] DefaultIdlePaths =
    {
        "Assets/Animations/Player/Idle/Frames/player_idle_01.png",
        "Assets/Animations/Player/Idle/Frames/player_idle_02.png",
        "Assets/Animations/Player/Idle/Frames/player_idle_03.png",
        "Assets/Animations/Player/Idle/Frames/player_idle_04.png"
    };

    private static readonly string[] DefaultRunPaths =
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
    };

    private static readonly string[] DefaultJumpPaths =
    {
        "Assets/Animations/Player/Jump/Frames/player_jump_00.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_01.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_02.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_03.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_04.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_05.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_06.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_07.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_08.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_09.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_10.png"
    };

    private static readonly string[] DefaultLandPaths =
    {
        "Assets/Animations/Player/Jump/Frames/player_jump_06.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_07.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_08.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_09.png",
        "Assets/Animations/Player/Jump/Frames/player_jump_10.png"
    };

    private static readonly string[] DefaultTurnPaths =
    {
        "Assets/Animations/Player/RunToStop/Frames/player_run_to_stop_01.png",
        "Assets/Animations/Player/RunToStop/Frames/player_run_to_stop_02.png",
        "Assets/Animations/Player/RunToStop/Frames/player_run_to_stop_03.png",
        "Assets/Animations/Player/RunToStop/Frames/player_run_to_stop_04.png",
        "Assets/Animations/Player/RunToStop/Frames/player_run_to_stop_05.png"
    };

    public enum MotionState
    {
        Idle,
        Run,
        Jump,
        Land,
        Turn
    }

    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private float idleFps = 6f;
    [SerializeField] private float runFps = 16f;
    [SerializeField] private float jumpFps = 12f;
    [SerializeField] private float landFps = 12f;
    [SerializeField] private float turnFps = 18f;
    [SerializeField] private float idleFirstFrameHoldSeconds = 0.4f;
    [SerializeField] private float pixelsPerUnit = 128f;

    private Sprite[] idleFrames;
    private Sprite[] runFrames;
    private Sprite[] jumpFrames;
    private Sprite[] landFrames;
    private Sprite[] turnFrames;
    private MotionState currentState;
    private float timer;
    private int frameIndex;
    private bool landingComplete;
    private bool turnComplete;
    private bool useJumpFrameRange;
    private bool jumpFrameRangeLoops;
    private int jumpFrameRangeStart;
    private int jumpFrameRangeEnd;
    private static bool hasLoggedMissingFramesWarning;

    public bool IsLandingComplete => landingComplete || (!HasLandFrames && !HasJumpLandingRecoveryFrames);
    public bool IsTurnComplete => turnComplete || !HasTurnFrames;
    public bool HasIdleFrames => idleFrames != null && idleFrames.Length > 0;
    public bool HasRunFrames => runFrames != null && runFrames.Length > 0;
    public bool HasJumpFrames => jumpFrames != null && jumpFrames.Length > 0;
    public bool HasJumpLandingRecoveryFrames => jumpFrames != null && jumpFrames.Length > 10;
    public bool HasLandFrames => landFrames != null && landFrames.Length > 0;
    public bool HasTurnFrames => turnFrames != null && turnFrames.Length > 0;
    public int CurrentFrameIndex => frameIndex;
    public MotionState CurrentState => currentState;

    public void ConfigureDefaultPlayerFrames(float spritePixelsPerUnit = 128f)
    {
#if UNITY_EDITOR
        Configure(DefaultIdlePaths, DefaultRunPaths, DefaultJumpPaths, DefaultLandPaths, DefaultTurnPaths, spritePixelsPerUnit);
#else
        ConfigureFromResources();
#endif
    }

    public void ConfigureFromResources()
    {
        targetRenderer = targetRenderer != null ? targetRenderer : GetComponent<SpriteRenderer>();
        idleFrames = LoadSpritesFromResources(IdleResourcePath);
        runFrames = LoadSpritesFromResources(RunResourcePath);
        jumpFrames = LoadSpritesFromResources(JumpResourcePath);
        landFrames = LoadSpritesFromResources(LandResourcePath);
        turnFrames = LoadSpritesFromResources(TurnResourcePath);
        LogLoadedFrames();
        LogMissingFramesWarningIfNeeded();
        SetState(MotionState.Idle, true);
    }

    public void Configure(string[] idlePaths, string[] runPaths, string[] jumpPaths, string[] landPaths, string[] turnPaths, float spritePixelsPerUnit = 128f)
    {
        targetRenderer = targetRenderer != null ? targetRenderer : GetComponent<SpriteRenderer>();
        pixelsPerUnit = spritePixelsPerUnit;
        idleFrames = LoadSprites(idlePaths, pixelsPerUnit);
        runFrames = LoadSprites(runPaths, pixelsPerUnit);
        jumpFrames = LoadSprites(jumpPaths, pixelsPerUnit);
        landFrames = LoadSprites(landPaths ?? System.Array.Empty<string>(), pixelsPerUnit);
        turnFrames = LoadSprites(turnPaths ?? System.Array.Empty<string>(), pixelsPerUnit);
        LogLoadedFrames();
        LogMissingFramesWarningIfNeeded();
        SetState(MotionState.Idle, true);
    }

    public void SetState(MotionState state, bool forceRestart = false)
    {
        if (state == MotionState.Jump)
        {
            PlayJumpRisingLoop();
            return;
        }

        if (!HasFramesForState(state))
        {
            HandleMissingStateFrames(state);
            return;
        }

        if (!forceRestart && currentState == state)
        {
            return;
        }

        currentState = state;
        useJumpFrameRange = false;
        timer = 0f;
        frameIndex = 0;
        if (state == MotionState.Land)
        {
            landingComplete = false;
        }
        if (state == MotionState.Turn)
        {
            turnComplete = false;
        }
        ApplyFrame();
    }

    public void PlayJumpTakeoff()
    {
        SetJumpFrameRange(1, 1, false, false);
        // 一段跳刚离地只显示第 1 帧，不改变任何跳跃物理。
        SetJumpFrameRange(1, 1, false, false);
    }

    public void PlayDoubleJumpTakeoff()
    {
        SetJumpFrameRange(3, 3, false, false);
        // 二段跳发生在空中，不能播放 0/1/2；从第 3 帧开始。
        SetJumpFrameRange(3, 3, false, false);
    }

    public void PlayJumpRisingLoop()
    {
        SetJumpFrameRange(3, 4, true, false);
        // 上升阶段循环 3-4，绝不播放落地恢复帧。
        SetJumpFrameRange(3, 4, true, false);
    }

    public void PlayJumpFallingLoop()
    {
        SetJumpFrameRange(4, 5, true, false);
        // 下落阶段循环 4-5，落地前绝不播放 6-10。
        SetJumpFrameRange(4, 5, true, false);
    }

    public void PlayJumpLandingRecovery()
    {
        SetJumpFrameRange(6, 10, false, true);
        // 接近地面或真正落地后才播放 6-10，且只播放一次。
        SetJumpFrameRange(6, 10, false, true);
    }

    public void PlayJumpAirLoop()
    {
        SetJumpFrameRange(4, 5, true, false);
        // 空中通用下落循环，保留给需要强制 4-5 的调用方。
        SetJumpFrameRange(4, 5, true, false);
    }

    private void Awake()
    {
        targetRenderer = targetRenderer != null ? targetRenderer : GetComponent<SpriteRenderer>();
        if (!HasAnyFrames())
        {
            ConfigureDefaultPlayerFrames(pixelsPerUnit);
        }
    }

    private void Update()
    {
        Sprite[] frames = GetFrames();
        if (targetRenderer == null || frames.Length == 0)
        {
            if (currentState == MotionState.Land)
            {
                landingComplete = true;
            }
            if (currentState == MotionState.Turn)
            {
                turnComplete = true;
            }
            return;
        }

        timer += Time.deltaTime;
        float frameTime = 1f / Mathf.Max(1f, GetFps());
        if (currentState == MotionState.Idle && frameIndex == 0)
        {
            frameTime += Mathf.Max(0f, idleFirstFrameHoldSeconds);
        }
        if (timer < frameTime)
        {
            return;
        }

        timer -= frameTime;

        if (currentState == MotionState.Land)
        {
            if (frameIndex >= frames.Length - 1)
            {
                landingComplete = true;
                return;
            }
            frameIndex++;
        }
        else if (currentState == MotionState.Turn)
        {
            if (frameIndex >= frames.Length - 1)
            {
                turnComplete = true;
                return;
            }
            frameIndex++;
        }
        else if (currentState == MotionState.Jump && useJumpFrameRange)
        {
            if (jumpFrameRangeLoops)
            {
                frameIndex++;
                if (frameIndex > jumpFrameRangeEnd)
                {
                    frameIndex = jumpFrameRangeStart;
                }
            }
            else
            {
                if (frameIndex >= jumpFrameRangeEnd)
                {
                    if (jumpFrameRangeStart >= 6)
                    {
                        landingComplete = true;
                    }
                    return;
                }

                frameIndex++;
            }
        }
        else
        {
            frameIndex = (frameIndex + 1) % frames.Length;
        }

        ApplyFrame();
    }

    private void ApplyFrame()
    {
        Sprite[] frames = GetFrames();
        if (targetRenderer != null && frames.Length > 0)
        {
            targetRenderer.sprite = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        }
    }

    private Sprite[] GetFrames()
    {
        return currentState switch
        {
            MotionState.Run => runFrames ?? System.Array.Empty<Sprite>(),
            MotionState.Jump => jumpFrames ?? System.Array.Empty<Sprite>(),
            MotionState.Land => (landFrames != null && landFrames.Length > 0) ? landFrames : (idleFrames ?? System.Array.Empty<Sprite>()),
            MotionState.Turn => (turnFrames != null && turnFrames.Length > 0) ? turnFrames : (runFrames ?? System.Array.Empty<Sprite>()),
            _ => idleFrames ?? System.Array.Empty<Sprite>()
        };
    }

    private float GetFps()
    {
        return currentState switch
        {
            MotionState.Run => runFps,
            MotionState.Jump => jumpFps,
            MotionState.Land => landFps,
            MotionState.Turn => turnFps,
            _ => idleFps
        };
    }

    private bool HasAnyFrames()
    {
        return HasIdleFrames || HasRunFrames || HasJumpFrames || HasLandFrames || HasTurnFrames;
    }

    private void SetJumpFrameRange(int start, int end, bool loop, bool landingRecovery)
    {
        if (!HasJumpFrames)
        {
            if (landingRecovery)
            {
                landingComplete = true;
            }
            return;
        }

        int clampedStart = Mathf.Clamp(start, 0, jumpFrames.Length - 1);
        int clampedEnd = Mathf.Clamp(end, clampedStart, jumpFrames.Length - 1);

        if (currentState == MotionState.Jump
            && useJumpFrameRange
            && jumpFrameRangeStart == clampedStart
            && jumpFrameRangeEnd == clampedEnd
            && jumpFrameRangeLoops == loop)
        {
            return;
        }

        currentState = MotionState.Jump;
        useJumpFrameRange = true;
        jumpFrameRangeLoops = loop;
        jumpFrameRangeStart = clampedStart;
        jumpFrameRangeEnd = clampedEnd;
        frameIndex = clampedStart;
        timer = 0f;

        if (landingRecovery)
        {
            landingComplete = false;
        }

        ApplyFrame();
    }

    private bool HasFramesForState(MotionState state)
    {
        return state switch
        {
            MotionState.Run => HasRunFrames,
            MotionState.Jump => HasJumpFrames,
            MotionState.Land => HasLandFrames,
            MotionState.Turn => HasTurnFrames,
            _ => HasIdleFrames
        };
    }

    private void HandleMissingStateFrames(MotionState state)
    {
        if (state == MotionState.Land)
        {
            landingComplete = true;
        }
        if (state == MotionState.Turn)
        {
            turnComplete = true;
        }

        MotionState fallbackState = HasIdleFrames ? MotionState.Idle : (HasRunFrames ? MotionState.Run : currentState);
        if (fallbackState != currentState && HasFramesForState(fallbackState))
        {
            SetState(fallbackState, true);
            return;
        }

        timer = 0f;
        frameIndex = 0;
        ApplyFrame();
    }

    private void LogMissingFramesWarningIfNeeded()
    {
#if !UNITY_EDITOR
        if (!HasAnyFrames() && !hasLoggedMissingFramesWarning)
        {
            Debug.LogWarning("[PrototypeFrameAnimator] Animation frames missing in build; falling back without blocking control.");
            hasLoggedMissingFramesWarning = true;
        }
#endif
    }

    private void LogLoadedFrames()
    {
        Debug.Log("[PrototypeFrameAnimator] Loaded frames: Idle=" + idleFrames.Length
            + ", Run=" + runFrames.Length
            + ", Jump=" + jumpFrames.Length
            + ", Land=" + landFrames.Length
            + ", Turn=" + turnFrames.Length);
    }

    private static Sprite[] LoadSpritesFromResources(string resourcePath)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(resourcePath);
        if (sprites == null || sprites.Length == 0)
        {
            return System.Array.Empty<Sprite>();
        }

        return sprites
            .OrderBy(sprite => ExtractFrameNumber(sprite.name))
            .ThenBy(sprite => sprite.name)
            .ToArray();
    }

    private static int ExtractFrameNumber(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            return int.MaxValue;
        }

        int multiplier = 1;
        int value = 0;
        bool foundDigit = false;

        for (int i = spriteName.Length - 1; i >= 0; i--)
        {
            char c = spriteName[i];
            if (c >= '0' && c <= '9')
            {
                foundDigit = true;
                value += (c - '0') * multiplier;
                multiplier *= 10;
            }
            else if (foundDigit)
            {
                break;
            }
        }

        return foundDigit ? value : int.MaxValue;
    }

    private static Sprite[] LoadSprites(string[] paths, float pixelsPerUnit)
    {
        Sprite[] sprites = new Sprite[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            sprites[i] = LoadSprite(paths[i], pixelsPerUnit);
        }

        return System.Array.FindAll(sprites, sprite => sprite != null);
    }

    private static Sprite LoadSprite(string path, float pixelsPerUnit)
    {
#if UNITY_EDITOR
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture != null)
        {
            texture.filterMode = FilterMode.Bilinear;
        }

        return texture != null
            ? Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit)
            : null;
#else
        return null;
#endif
    }
}
