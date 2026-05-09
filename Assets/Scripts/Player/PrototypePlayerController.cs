using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public sealed class PrototypePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpVelocity = 8f;
    [SerializeField] private float doubleJumpHeight = 3.25f;
    [SerializeField] private float jumpTakeoffFrameHoldSeconds = 0.08f;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private AudioSource audioSource;

    [Header("Jump SFX")]
    [SerializeField] private AudioClip jumpSfx;
    [SerializeField] private AudioClip doubleJumpSfx;
    [SerializeField] private float jumpSfxVolume = 0.8f;

    [Header("Footstep SFX")]
    [SerializeField] private AudioClip[] grassFootstepSfx;
    [SerializeField] private float footstepVolume = 0.45f;
    [SerializeField] private float footstepInterval = 0.32f;
    [SerializeField] private float minFootstepSpeed = 0.1f;

    [Header("Jump Animation Debug")]
    [SerializeField] private bool logJumpAnimationDebug = false;
    [SerializeField] private JumpAnimState debugJumpAnimState;
    [SerializeField] private bool debugGrounded;
    [SerializeField] private bool debugWasGrounded;
    [SerializeField] private float debugVerticalVelocity;
    [SerializeField] private int debugFrameIndex;
    [SerializeField] private bool debugLandingRecovery;
    [SerializeField] private bool debugIsDoubleJump;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private PrototypeFrameAnimator frameAnimator;
    private bool wasGrounded;
    private bool isLanding;
    private bool isTurning;
    private JumpAnimState jumpAnimationState;
    private int facingDirection = 1;
    private int remainingJumps;
    private float footstepTimer;
    private float jumpTakeoffTimer;
    private bool currentAirJumpIsDouble;
    private JumpAnimState lastLoggedJumpAnimState;

    private enum JumpAnimState
    {
        Grounded,
        FirstJumpTakeoff,
        FirstJumpRising,
        DoubleJumpTakeoff,
        DoubleJumpRising,
        Falling,
        LandingRecovery
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        frameAnimator = GetComponent<PrototypeFrameAnimator>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource not found on Player Prototype. Please add an AudioSource component.");
            }
        }
        body.freezeRotation = true;
        wasGrounded = true;
        jumpAnimationState = JumpAnimState.Grounded;
        lastLoggedJumpAnimState = JumpAnimState.Grounded;
        remainingJumps = maxJumps;
        footstepTimer = 0f;
    }

    private void Update()
    {
        var grounded = IsGrounded();
        if (grounded && body.linearVelocity.y <= 0.01f)
        {
            remainingJumps = maxJumps;
        }

        if (!wasGrounded && grounded && frameAnimator != null && frameAnimator.HasJumpLandingRecoveryFrames)
        {
            // 只有真正落地后才进入 6-10 落地恢复帧。
            isLanding = true;
            isTurning = false;
            jumpAnimationState = JumpAnimState.LandingRecovery;
            frameAnimator.PlayJumpLandingRecovery();
        }

        if (isLanding)
        {
            if (frameAnimator == null || !frameAnimator.HasJumpLandingRecoveryFrames)
            {
                isLanding = false;
                jumpAnimationState = JumpAnimState.Grounded;
                currentAirJumpIsDouble = false;
            }
            else
            {
                // 落地恢复 6-10 播放期间锁输入：不能左右移动、不能跳跃、不能二段跳。
                var velocity = body.linearVelocity;
                velocity.x = 0f;
                body.linearVelocity = velocity;

                if (frameAnimator.IsLandingComplete)
                {
                    isLanding = false;
                    jumpAnimationState = JumpAnimState.Grounded;
                    currentAirJumpIsDouble = false;
                }
                else
                {
                    UpdateJumpAnimationDebug(grounded);
                    wasGrounded = grounded;
                    return;
                }
            }
        }

        var horizontal = GetHorizontalInput();

        if (!isTurning
            && grounded
            && frameAnimator != null
            && frameAnimator.HasTurnFrames
            && Mathf.Abs(horizontal) > 0.01f
            && (int)Mathf.Sign(horizontal) != facingDirection
            && Mathf.Abs(body.linearVelocity.x) > 0.1f)
        {
            isTurning = true;
            frameAnimator.SetState(PrototypeFrameAnimator.MotionState.Turn, true);
        }

        if (isTurning)
        {
            if (frameAnimator == null || !frameAnimator.HasTurnFrames)
            {
                isTurning = false;
            }
            else
            {
                var v = body.linearVelocity;
                v.x = 0f;
                body.linearVelocity = v;

                if (frameAnimator.IsTurnComplete)
                {
                    facingDirection = -facingDirection;
                    spriteRenderer.flipX = facingDirection < 0;
                    isTurning = false;
                }
                else
                {
                    wasGrounded = grounded;
                    return;
                }
            }
        }

        var moveVelocity = body.linearVelocity;
        moveVelocity.x = horizontal * moveSpeed;
        body.linearVelocity = moveVelocity;

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            facingDirection = horizontal < 0f ? -1 : 1;
            spriteRenderer.flipX = facingDirection < 0;
        }

        var jumpPressed = WasJumpPressed();
        if (jumpPressed && remainingJumps > 0)
        {
            if (grounded)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
                jumpAnimationState = JumpAnimState.FirstJumpTakeoff;
                jumpTakeoffTimer = jumpTakeoffFrameHoldSeconds;
                currentAirJumpIsDouble = false;
                frameAnimator?.PlayJumpTakeoff();
                if (audioSource != null && jumpSfx != null)
                {
                    audioSource.PlayOneShot(jumpSfx, jumpSfxVolume);
                }
            }
            else
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, CalculateJumpVelocity(doubleJumpHeight));
                jumpAnimationState = JumpAnimState.DoubleJumpTakeoff;
                jumpTakeoffTimer = jumpTakeoffFrameHoldSeconds;
                currentAirJumpIsDouble = true;
                frameAnimator?.PlayDoubleJumpTakeoff();
                if (audioSource != null && doubleJumpSfx != null)
                {
                    audioSource.PlayOneShot(doubleJumpSfx, jumpSfxVolume);
                }
            }
            remainingJumps--;
            grounded = false;
        }

        if (grounded && Mathf.Abs(horizontal) > minFootstepSpeed && !isLanding && !isTurning && grassFootstepSfx != null && grassFootstepSfx.Length > 0 && audioSource != null)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                int randomIndex = Random.Range(0, grassFootstepSfx.Length);
                audioSource.PlayOneShot(grassFootstepSfx[randomIndex], footstepVolume);
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        if (!grounded)
        {
            if ((jumpAnimationState == JumpAnimState.FirstJumpTakeoff || jumpAnimationState == JumpAnimState.DoubleJumpTakeoff) && jumpTakeoffTimer > 0f)
            {
                // 起跳瞬间短暂保持对应起跳帧，不改变物理跳跃高度。
                jumpTakeoffTimer -= Time.deltaTime;
            }
            else if (body.linearVelocity.y > 0.01f)
            {
                jumpAnimationState = currentAirJumpIsDouble ? JumpAnimState.DoubleJumpRising : JumpAnimState.FirstJumpRising;
                frameAnimator?.PlayJumpRisingLoop();
            }
            else
            {
                jumpAnimationState = JumpAnimState.Falling;
                frameAnimator?.PlayJumpFallingLoop();
            }
        }
        else if (Mathf.Abs(horizontal) > 0.01f)
        {
            jumpAnimationState = JumpAnimState.Grounded;
            currentAirJumpIsDouble = false;
            frameAnimator?.SetState(PrototypeFrameAnimator.MotionState.Run);
        }
        else
        {
            jumpAnimationState = JumpAnimState.Grounded;
            currentAirJumpIsDouble = false;
            frameAnimator?.SetState(PrototypeFrameAnimator.MotionState.Idle);
        }

        UpdateJumpAnimationDebug(grounded);
        wasGrounded = grounded;
    }

    private void UpdateJumpAnimationDebug(bool grounded)
    {
        debugJumpAnimState = jumpAnimationState;
        debugGrounded = grounded;
        debugWasGrounded = wasGrounded;
        debugVerticalVelocity = body != null ? body.linearVelocity.y : 0f;
        debugFrameIndex = frameAnimator != null ? frameAnimator.CurrentFrameIndex : -1;
        debugLandingRecovery = jumpAnimationState == JumpAnimState.LandingRecovery;
        debugIsDoubleJump = currentAirJumpIsDouble;

        if (logJumpAnimationDebug && lastLoggedJumpAnimState != jumpAnimationState)
        {
            Debug.Log("[JumpAnim] state=" + jumpAnimationState
                + ", grounded=" + grounded
                + ", wasGrounded=" + wasGrounded
                + ", y=" + debugVerticalVelocity
                + ", frame=" + debugFrameIndex
                + ", landingRecovery=" + debugLandingRecovery
                + ", doubleJump=" + debugIsDoubleJump);
            lastLoggedJumpAnimState = jumpAnimationState;
        }
    }

    private bool IsGrounded()
    {
        if (body != null && body.linearVelocity.y > 0.05f)
        {
            return false;
        }

        var origin = (Vector2)transform.position + Vector2.down * 0.15f;
        return Physics2D.Raycast(origin, Vector2.down, 0.55f, groundMask);
    }

    private float CalculateJumpVelocity(float jumpHeight)
    {
        var gravity = Mathf.Abs(Physics2D.gravity.y * body.gravityScale);
        return Mathf.Sqrt(2f * gravity * jumpHeight);
    }

    private static float GetHorizontalInput()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return 0f;
        }

        var left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
        var right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
        return (right ? 1f : 0f) - (left ? 1f : 0f);
#else
        return Input.GetAxisRaw("Horizontal");
#endif
    }

    private static bool WasJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
#else
        return Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
#endif
    }

    public bool IsLeafFlying()
    {
        return false;
    }

    public void TurnToLeafAndFly()
    {
        Debug.Log("TurnToLeafAndFly called - leaf transformation not implemented in current player controller");
    }
}
