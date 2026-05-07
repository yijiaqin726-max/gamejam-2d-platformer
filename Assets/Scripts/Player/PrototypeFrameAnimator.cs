using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class PrototypeFrameAnimator : MonoBehaviour
{
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

    public bool IsLandingComplete => landingComplete;
    public bool IsTurnComplete => turnComplete;
    public bool HasTurnFrames => turnFrames != null && turnFrames.Length > 0;

    public void Configure(string[] idlePaths, string[] runPaths, string[] jumpPaths, string[] landPaths, string[] turnPaths, float spritePixelsPerUnit = 128f)
    {
        targetRenderer = targetRenderer != null ? targetRenderer : GetComponent<SpriteRenderer>();
        pixelsPerUnit = spritePixelsPerUnit;
        idleFrames = LoadSprites(idlePaths, pixelsPerUnit);
        runFrames = LoadSprites(runPaths, pixelsPerUnit);
        jumpFrames = LoadSprites(jumpPaths, pixelsPerUnit);
        landFrames = LoadSprites(landPaths ?? System.Array.Empty<string>(), pixelsPerUnit);
        turnFrames = LoadSprites(turnPaths ?? System.Array.Empty<string>(), pixelsPerUnit);
        SetState(MotionState.Idle, true);
    }

    public void SetState(MotionState state, bool forceRestart = false)
    {
        if (!forceRestart && currentState == state)
        {
            return;
        }

        currentState = state;
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

    private void Awake()
    {
        targetRenderer = targetRenderer != null ? targetRenderer : GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        var frames = GetFrames();
        if (targetRenderer == null || frames.Length == 0)
        {
            return;
        }

        timer += Time.deltaTime;
        var frameTime = 1f / Mathf.Max(1f, GetFps());
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
        else
        {
            frameIndex = (frameIndex + 1) % frames.Length;
        }

        ApplyFrame();
    }

    private void ApplyFrame()
    {
        var frames = GetFrames();
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

    private static Sprite[] LoadSprites(string[] paths, float pixelsPerUnit)
    {
        var sprites = new Sprite[paths.Length];
        for (var i = 0; i < paths.Length; i++)
        {
            sprites[i] = LoadSprite(paths[i], pixelsPerUnit);
        }

        return System.Array.FindAll(sprites, sprite => sprite != null);
    }

    private static Sprite LoadSprite(string path, float pixelsPerUnit)
    {
#if UNITY_EDITOR
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
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
