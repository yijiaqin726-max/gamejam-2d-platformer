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
        Jump
    }

    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private float idleFps = 5f;
    [SerializeField] private float runFps = 10f;
    [SerializeField] private float jumpFps = 8f;

    private Sprite[] idleFrames;
    private Sprite[] runFrames;
    private Sprite[] jumpFrames;
    private MotionState currentState;
    private float timer;
    private int frameIndex;

    public void Configure(string[] idlePaths, string[] runPaths, string[] jumpPaths)
    {
        targetRenderer = targetRenderer != null ? targetRenderer : GetComponent<SpriteRenderer>();
        idleFrames = LoadSprites(idlePaths);
        runFrames = LoadSprites(runPaths);
        jumpFrames = LoadSprites(jumpPaths);
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
        if (timer < frameTime)
        {
            return;
        }

        timer -= frameTime;
        frameIndex = (frameIndex + 1) % frames.Length;
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
            _ => idleFrames ?? System.Array.Empty<Sprite>()
        };
    }

    private float GetFps()
    {
        return currentState switch
        {
            MotionState.Run => runFps,
            MotionState.Jump => jumpFps,
            _ => idleFps
        };
    }

    private static Sprite[] LoadSprites(string[] paths)
    {
        var sprites = new Sprite[paths.Length];
        for (var i = 0; i < paths.Length; i++)
        {
            sprites[i] = LoadSprite(paths[i]);
        }

        return System.Array.FindAll(sprites, sprite => sprite != null);
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
}
