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
    [SerializeField] private LayerMask groundMask = ~0;
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

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private PrototypeFrameAnimator frameAnimator;
    private bool wasGrounded;
    private bool isLanding;
    private bool isTurning;
    private int facingDirection = 1;
    private int remainingAirJumps;
    private float footstepTimer;

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
        remainingAirJumps = 1;
        footstepTimer = 0f;
    }

    private void Update()
    {
        var grounded = IsGrounded();
        if (grounded && body.linearVelocity.y <= 0.01f)
        {
            remainingAirJumps = 1;
        }

        if (!wasGrounded && grounded && frameAnimator != null)
        {
            isLanding = true;
            isTurning = false;
            frameAnimator.SetState(PrototypeFrameAnimator.MotionState.Land, true);
        }

        if (isLanding)
        {
            var velocity = body.linearVelocity;
            velocity.x = 0f;
            body.linearVelocity = velocity;

            if (frameAnimator == null || frameAnimator.IsLandingComplete)
            {
                isLanding = false;
            }
            else
            {
                wasGrounded = grounded;
                return;
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
            var v = body.linearVelocity;
            v.x = 0f;
            body.linearVelocity = v;

            if (frameAnimator == null || frameAnimator.IsTurnComplete)
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

        var moveVelocity = body.linearVelocity;
        moveVelocity.x = horizontal * moveSpeed;
        body.linearVelocity = moveVelocity;

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            facingDirection = horizontal < 0f ? -1 : 1;
            spriteRenderer.flipX = facingDirection < 0;
        }

        var jumpPressed = WasJumpPressed();
        if (jumpPressed && grounded)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
            grounded = false;
            if (audioSource != null && jumpSfx != null)
            {
                audioSource.PlayOneShot(jumpSfx, jumpSfxVolume);
            }
        }
        else if (jumpPressed && !grounded && remainingAirJumps > 0)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, CalculateJumpVelocity(doubleJumpHeight));
            remainingAirJumps--;
            grounded = false;
            if (audioSource != null && doubleJumpSfx != null)
            {
                audioSource.PlayOneShot(doubleJumpSfx, jumpSfxVolume);
            }
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
            frameAnimator?.SetState(PrototypeFrameAnimator.MotionState.Jump);
        }
        else if (Mathf.Abs(horizontal) > 0.01f)
        {
            frameAnimator?.SetState(PrototypeFrameAnimator.MotionState.Run);
        }
        else
        {
            frameAnimator?.SetState(PrototypeFrameAnimator.MotionState.Idle);
        }

        wasGrounded = grounded;
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
}
