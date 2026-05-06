using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class PrototypePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpVelocity = 8f;
    [SerializeField] private LayerMask groundMask = ~0;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private PrototypeFrameAnimator frameAnimator;
    private bool wasGrounded;
    private bool isLanding;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        frameAnimator = GetComponent<PrototypeFrameAnimator>();
        body.freezeRotation = true;
        wasGrounded = true;
    }

    private void Update()
    {
        var grounded = IsGrounded();

        if (!wasGrounded && grounded && frameAnimator != null)
        {
            isLanding = true;
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
        var moveVelocity = body.linearVelocity;
        moveVelocity.x = horizontal * moveSpeed;
        body.linearVelocity = moveVelocity;

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            spriteRenderer.flipX = horizontal < 0f;
        }

        if (WasJumpPressed() && grounded)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpVelocity);
            grounded = false;
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
        var origin = (Vector2)transform.position + Vector2.down * 0.15f;
        return Physics2D.Raycast(origin, Vector2.down, 0.55f, groundMask);
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
