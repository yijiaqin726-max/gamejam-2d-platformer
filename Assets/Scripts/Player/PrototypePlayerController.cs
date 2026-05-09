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
    [SerializeField] private float groundCheckRayDistance = 0.12f;
    [SerializeField] private float footRaySpacing = 0.15f;
    [SerializeField] private float maxGroundAngle = 60f;
    [SerializeField] private int landingGroundConfirmFrames = 2;
    [SerializeField] private float nearGroundDistance = 0.18f;
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
    [SerializeField] private int debugFrameIndex;
    [SerializeField] private bool debugGroundedNow;
    [SerializeField] private bool debugConfirmedGrounded;
    [SerializeField] private bool debugGrounded;
    [SerializeField] private bool debugWasGrounded;
    [SerializeField] private bool debugNearGround;
    [SerializeField] private float debugNearGroundHitDistance;
    [SerializeField] private float debugVerticalVelocity;
    [SerializeField] private bool debugJustTriggeredDoubleJump;
    [SerializeField] private bool debugLandingRecovery;
    [SerializeField] private bool debugLandingRecoveryPlayedForThisLanding;
    [SerializeField] private float debugGroundCheckRayDistance;
    [SerializeField] private Vector2 debugGroundHitNormal = Vector2.up;
    [SerializeField] private float debugGroundAngle;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Collider2D bodyCollider;
    private PrototypeFrameAnimator frameAnimator;
    private bool wasGrounded;
    private bool isLanding;
    private bool isTurning;
    private bool currentAirJumpIsDouble;
    private bool justTriggeredDoubleJumpThisFrame;
    private bool landingRecoveryPlayedForThisLanding;
    private bool groundedNow;
    private bool confirmedGrounded;
    private int groundedConfirmFrameCount;
    private JumpAnimState jumpAnimationState;
    private JumpAnimState lastLoggedJumpAnimState;
    private int facingDirection = 1;
    private int remainingJumps;
    private float footstepTimer;
    private float jumpTakeoffTimer;
    private Vector2 groundHitNormal = Vector2.up;
    private float groundAngle;

    private enum JumpAnimState
    {
        Grounded,
        FirstJumpTakeoff,
        FirstJumpRising,
        DoubleJumpStart,
        DoubleJumpRising,
        Falling,
        LandingRecovery
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();
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
        groundedNow = CheckGroundedRaw();
        confirmedGrounded = groundedNow;
        groundedConfirmFrameCount = groundedNow ? Mathf.Max(1, landingGroundConfirmFrames) : 0;
        wasGrounded = confirmedGrounded;
        jumpAnimationState = JumpAnimState.Grounded;
        lastLoggedJumpAnimState = JumpAnimState.Grounded;
        remainingJumps = maxJumps;
        footstepTimer = 0f;
    }

    private void FixedUpdate()
    {
        UpdateGroundedConfirmation();
    }

    private void Update()
    {
        justTriggeredDoubleJumpThisFrame = false;

        bool grounded = confirmedGrounded;
        bool nearGround = IsNearGround(out float nearGroundHitDistance);

        if (wasGrounded && !grounded)
        {
            landingRecoveryPlayedForThisLanding = false;
        }

        if (grounded && body.linearVelocity.y <= 0.01f)
        {
            remainingJumps = maxJumps;
        }

        if (ShouldStartLandingRecovery(grounded))
        {
            StartLandingRecovery();
        }

        if (isLanding)
        {
            // 6-10 落地恢复帧期间锁输入：不能移动、不能跳、不能二段跳。
            Vector2 velocity = body.linearVelocity;
            velocity.x = 0f;
            body.linearVelocity = velocity;

            if (frameAnimator == null || !frameAnimator.HasJumpLandingRecoveryFrames)
            {
                isLanding = false;
                currentAirJumpIsDouble = false;
                jumpAnimationState = grounded ? JumpAnimState.Grounded : JumpAnimState.Falling;
                UpdateJumpAnimationDebug(grounded, nearGround, nearGroundHitDistance);
                wasGrounded = grounded;
                return;
            }

            if (frameAnimator.IsLandingComplete)
            {
                isLanding = false;
                currentAirJumpIsDouble = false;
                jumpAnimationState = grounded ? JumpAnimState.Grounded : JumpAnimState.Falling;
                UpdateJumpAnimationDebug(grounded, nearGround, nearGroundHitDistance);
                wasGrounded = grounded;
                return;
            }
            else
            {
                UpdateJumpAnimationDebug(grounded, nearGround, nearGroundHitDistance);
                wasGrounded = grounded;
                return;
            }
        }

        float horizontal = GetHorizontalInput();

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
                Vector2 turnVelocity = body.linearVelocity;
                turnVelocity.x = 0f;
                body.linearVelocity = turnVelocity;

                if (frameAnimator.IsTurnComplete)
                {
                    facingDirection = -facingDirection;
                    spriteRenderer.flipX = facingDirection < 0;
                    isTurning = false;
                }
                else
                {
                    UpdateJumpAnimationDebug(grounded, nearGround, nearGroundHitDistance);
                    wasGrounded = grounded;
                    return;
                }
            }
        }

        Vector2 moveVelocity = body.linearVelocity;
        moveVelocity.x = horizontal * moveSpeed;
        body.linearVelocity = moveVelocity;

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            facingDirection = horizontal < 0f ? -1 : 1;
            spriteRenderer.flipX = facingDirection < 0;
        }

        bool jumpPressed = WasJumpPressed();
        if (jumpPressed && remainingJumps > 0)
        {
            if (grounded)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
                jumpAnimationState = JumpAnimState.FirstJumpTakeoff;
                jumpTakeoffTimer = jumpTakeoffFrameHoldSeconds;
                currentAirJumpIsDouble = false;
                landingRecoveryPlayedForThisLanding = false;
                frameAnimator?.PlayJumpTakeoff();
                PlayJumpSfx(jumpSfx);
            }
            else
            {
                // 二段跳高度保持原项目逻辑：仍然使用 doubleJumpHeight + CalculateJumpVelocity。
                body.linearVelocity = new Vector2(body.linearVelocity.x, CalculateJumpVelocity(doubleJumpHeight));
                jumpAnimationState = JumpAnimState.DoubleJumpStart;
                jumpTakeoffTimer = jumpTakeoffFrameHoldSeconds;
                currentAirJumpIsDouble = true;
                justTriggeredDoubleJumpThisFrame = true;
                frameAnimator?.PlayDoubleJumpTakeoff();
                PlayJumpSfx(doubleJumpSfx);
            }

            remainingJumps--;
            grounded = false;
            nearGround = false;
            nearGroundHitDistance = float.PositiveInfinity;
        }

        UpdateFootsteps(grounded, horizontal);
        UpdateJumpAnimationState(grounded, nearGround);
        UpdateJumpAnimationDebug(grounded, nearGround, nearGroundHitDistance);
        wasGrounded = grounded;
    }

    private void UpdateJumpAnimationState(bool grounded, bool nearGround)
    {
        if (!grounded)
        {
            bool holdingTakeoff = (jumpAnimationState == JumpAnimState.FirstJumpTakeoff
                || jumpAnimationState == JumpAnimState.DoubleJumpStart)
                && jumpTakeoffTimer > 0f;

            if (holdingTakeoff)
            {
                // 起跳帧只短暂保持，不改变 Rigidbody2D 的高度或速度。
                jumpTakeoffTimer -= Time.deltaTime;
                return;
            }

            if (body.linearVelocity.y > 0.01f)
            {
                jumpAnimationState = currentAirJumpIsDouble ? JumpAnimState.DoubleJumpRising : JumpAnimState.FirstJumpRising;
                frameAnimator?.PlayJumpRisingLoop();
                return;
            }

            jumpAnimationState = JumpAnimState.Falling;
            frameAnimator?.PlayJumpFallingLoop();
            return;
        }

        if (ShouldStartLandingRecovery(grounded))
        {
            StartLandingRecovery();
            return;
        }

        currentAirJumpIsDouble = false;
        jumpAnimationState = JumpAnimState.Grounded;

        if (Mathf.Abs(body.linearVelocity.x) > 0.01f)
        {
            frameAnimator?.SetState(PrototypeFrameAnimator.MotionState.Run);
        }
        else
        {
            frameAnimator?.SetState(PrototypeFrameAnimator.MotionState.Idle);
        }
    }

    private bool ShouldStartLandingRecovery(bool grounded)
    {
        if (isLanding || landingRecoveryPlayedForThisLanding || frameAnimator == null || !frameAnimator.HasJumpLandingRecoveryFrames)
        {
            return false;
        }

        return !wasGrounded && grounded && body.linearVelocity.y <= 0.05f;
    }

    private void StartLandingRecovery()
    {
        isLanding = true;
        isTurning = false;
        landingRecoveryPlayedForThisLanding = true;
        jumpAnimationState = JumpAnimState.LandingRecovery;
        frameAnimator?.PlayJumpLandingRecovery();
    }

    private void UpdateFootsteps(bool grounded, float horizontal)
    {
        if (grounded
            && Mathf.Abs(horizontal) > minFootstepSpeed
            && !isLanding
            && !isTurning
            && grassFootstepSfx != null
            && grassFootstepSfx.Length > 0
            && audioSource != null)
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
    }

    private void PlayJumpSfx(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, jumpSfxVolume);
        }
    }

    private void UpdateJumpAnimationDebug(bool grounded, bool nearGround, float nearGroundHitDistance)
    {
        debugJumpAnimState = jumpAnimationState;
        debugFrameIndex = frameAnimator != null ? frameAnimator.CurrentFrameIndex : -1;
        debugGroundedNow = groundedNow;
        debugConfirmedGrounded = confirmedGrounded;
        debugGrounded = grounded;
        debugWasGrounded = wasGrounded;
        debugNearGround = nearGround;
        debugNearGroundHitDistance = nearGroundHitDistance;
        debugVerticalVelocity = body != null ? body.linearVelocity.y : 0f;
        debugJustTriggeredDoubleJump = justTriggeredDoubleJumpThisFrame;
        debugLandingRecovery = jumpAnimationState == JumpAnimState.LandingRecovery;
        debugLandingRecoveryPlayedForThisLanding = landingRecoveryPlayedForThisLanding;
        debugGroundCheckRayDistance = GetEffectiveGroundCheckDistance();
        debugGroundHitNormal = groundHitNormal;
        debugGroundAngle = groundAngle;

        if (logJumpAnimationDebug && lastLoggedJumpAnimState != jumpAnimationState)
        {
            Debug.Log("[JumpAnim] state=" + jumpAnimationState
                + ", frame=" + debugFrameIndex
                + ", groundedNow=" + debugGroundedNow
                + ", confirmedGrounded=" + debugConfirmedGrounded
                + ", grounded=" + grounded
                + ", wasGrounded=" + wasGrounded
                + ", nearGround=" + nearGround
                + ", hitDistance=" + nearGroundHitDistance
                + ", groundNormal=" + debugGroundHitNormal
                + ", groundAngle=" + debugGroundAngle
                + ", yVelocity=" + debugVerticalVelocity
                + ", justDoubleJump=" + debugJustTriggeredDoubleJump
                + ", landingRecovery=" + debugLandingRecovery
                + ", landingPlayed=" + debugLandingRecoveryPlayedForThisLanding);
            lastLoggedJumpAnimState = jumpAnimationState;
        }
    }

    private void UpdateGroundedConfirmation()
    {
        groundedNow = CheckGroundedRaw();
        if (groundedNow)
        {
            groundedConfirmFrameCount++;
        }
        else
        {
            groundedConfirmFrameCount = 0;
            confirmedGrounded = false;
        }

        if (groundedConfirmFrameCount >= Mathf.Max(1, landingGroundConfirmFrames))
        {
            confirmedGrounded = true;
        }
    }

    private bool CheckGroundedRaw()
    {
        if (body != null && body.linearVelocity.y > 0.05f)
        {
            groundHitNormal = Vector2.up;
            groundAngle = 0f;
            return false;
        }

        Vector2[] origins = GetGroundCheckOrigins();
        float distance = GetEffectiveGroundCheckDistance();
        groundHitNormal = Vector2.up;
        groundAngle = 0f;

        for (int originIndex = 0; originIndex < origins.Length; originIndex++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(origins[originIndex], Vector2.down, distance, groundMask);
            for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                RaycastHit2D hit = hits[hitIndex];
                if (!IsValidGroundHit(hit, out float angle))
                {
                    continue;
                }

                groundHitNormal = hit.normal;
                groundAngle = angle;
                return true;
            }
        }

        return false;
    }

    private Vector2 GetGroundCheckOrigin()
    {
        if (bodyCollider != null)
        {
            Bounds bounds = bodyCollider.bounds;
            return new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        }

        return (Vector2)transform.position + Vector2.down * 0.15f;
    }

    private Vector2[] GetGroundCheckOrigins()
    {
        Vector2 center = GetGroundCheckOrigin();
        float spacing = Mathf.Max(0.01f, footRaySpacing);

        if (bodyCollider != null)
        {
            Bounds bounds = bodyCollider.bounds;
            spacing = Mathf.Min(spacing, Mathf.Max(0.01f, bounds.extents.x * 0.85f));
        }

        return new[]
        {
            center + Vector2.left * spacing,
            center,
            center + Vector2.right * spacing
        };
    }

    private float GetEffectiveGroundCheckDistance()
    {
        return Mathf.Max(0.08f, groundCheckRayDistance);
    }

    private bool IsValidGroundHit(RaycastHit2D hit, out float angle)
    {
        angle = 0f;
        if (!hit.collider || hit.collider.isTrigger)
        {
            return false;
        }

        Transform hitTransform = hit.collider.transform;
        if (hitTransform == transform || hitTransform.IsChildOf(transform))
        {
            return false;
        }

        angle = Vector2.Angle(hit.normal, Vector2.up);
        return angle <= Mathf.Clamp(maxGroundAngle, 0f, 89f);
    }

    private bool IsNearGround(out float hitDistance)
    {
        hitDistance = float.PositiveInfinity;
        if (body != null && body.linearVelocity.y > 0.01f)
        {
            return false;
        }

        Vector2 origin;
        if (bodyCollider != null)
        {
            origin = GetGroundCheckOrigin();
        }
        else
        {
            origin = (Vector2)transform.position + Vector2.down * 0.15f;
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, Mathf.Max(0.01f, nearGroundDistance), groundMask);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            if (!hit.collider)
            {
                continue;
            }

            // 脚底检测只认外部地面，跳过玩家自己或玩家子物体上的 Collider。
            if (hit.collider.isTrigger)
            {
                continue;
            }

            Transform hitTransform = hit.collider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
            {
                continue;
            }

            hitDistance = hit.distance;
            return hit.distance <= nearGroundDistance;
        }

        return false;
    }

    private float CalculateJumpVelocity(float jumpHeight)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * body.gravityScale);
        return Mathf.Sqrt(2f * gravity * jumpHeight);
    }

    private static float GetHorizontalInput()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return 0f;
        }

        bool left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
        bool right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
        return (right ? 1f : 0f) - (left ? 1f : 0f);
#else
        return Input.GetAxisRaw("Horizontal");
#endif
    }

    private static bool WasJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
#else
        return Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
#endif
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D drawCollider = bodyCollider != null ? bodyCollider : GetComponent<Collider2D>();
        Vector2 origin;
        if (drawCollider != null)
        {
            Bounds bounds = drawCollider.bounds;
            origin = new Vector2(bounds.center.x, bounds.min.y);
        }
        else
        {
            origin = (Vector2)transform.position + Vector2.down * 0.15f;
        }

        Vector2[] origins = drawCollider != null
            ? GetGroundCheckOriginsForCollider(drawCollider)
            : new[] { origin + Vector2.left * footRaySpacing, origin, origin + Vector2.right * footRaySpacing };
        float distance = Mathf.Max(0.08f, groundCheckRayDistance);

        for (int i = 0; i < origins.Length; i++)
        {
            bool hitGround = TryGetGroundHitForGizmo(origins[i], distance);
            Gizmos.color = hitGround ? Color.green : Color.red;
            Gizmos.DrawSphere(origins[i], 0.025f);
            Gizmos.DrawLine(origins[i], origins[i] + Vector2.down * distance);
        }
    }

    private Vector2[] GetGroundCheckOriginsForCollider(Collider2D drawCollider)
    {
        Bounds bounds = drawCollider.bounds;
        Vector2 center = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        float spacing = Mathf.Min(Mathf.Max(0.01f, footRaySpacing), Mathf.Max(0.01f, bounds.extents.x * 0.85f));
        return new[]
        {
            center + Vector2.left * spacing,
            center,
            center + Vector2.right * spacing
        };
    }

    private bool TryGetGroundHitForGizmo(Vector2 origin, float distance)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, distance, groundMask);
        for (int i = 0; i < hits.Length; i++)
        {
            if (IsValidGroundHit(hits[i], out _))
            {
                return true;
            }
        }

        return false;
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
