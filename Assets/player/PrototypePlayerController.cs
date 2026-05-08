using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class PrototypePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpVelocity = 14f;
    [SerializeField] private float doubleJumpForce = 11f;

    [Header("变身叶子")]
    public float leafUpDownControlSpeed = 4f;
    public float leafAutoRightSpeed = 3f;
    public GameObject leafEffectPrefab;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Animator ani;
    private bool isGrounded;
    private bool canDoubleJump;

    private bool isLeafFlying = false;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();
        body.freezeRotation = true;
    }

    private void Update()
    {
        if (isLeafFlying)
        {
            FlyAsLeaf();
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        body.velocity = new Vector2(h * moveSpeed, body.velocity.y);

        if (Mathf.Abs(h) > 0.01f)
        {
            spriteRenderer.flipX = h < 0;
        }

        ani.SetFloat("speed", Mathf.Abs(h));
        ani.SetBool("IsJumping", !isGrounded);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                body.velocity = new Vector2(body.velocity.x, jumpVelocity);
                isGrounded = false;
                canDoubleJump = true;
            }
            else if (canDoubleJump)
            {
                body.velocity = new Vector2(body.velocity.x, doubleJumpForce);
                canDoubleJump = false;
            }
        }
    }

    public void TurnToLeafAndFly()
    {
        if (isLeafFlying) return;

        isLeafFlying = true;

        body.velocity = Vector2.zero;
        body.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;

        spriteRenderer.enabled = false;
        ani.enabled = false;

        if (leafEffectPrefab != null)
        {
            Instantiate(leafEffectPrefab, transform);
        }
    }

    void FlyAsLeaf()
    {
        float v = Input.GetAxisRaw("Vertical");
        body.velocity = new Vector2(leafAutoRightSpeed, v * leafUpDownControlSpeed);
    }

    public bool IsLeafFlying()
    {
        return isLeafFlying;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            canDoubleJump = true;
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}