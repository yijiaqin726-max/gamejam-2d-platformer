using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class PrototypePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpVelocity = 22f;
    [SerializeField] private float doubleJumpForce = 18f;

    // 新增：变身叶子飞上天的参数
    [Header("变身叶子")]
    public float leafFlySpeed = 10f;       // 飞得多快
    public float leafFlyTime = 2.5f;       // 飞多久
    public GameObject leafEffectPrefab;   // 拖入叶子预制体（可选）

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Animator ani;
    private bool isGrounded;
    private bool canDoubleJump;

    // 新增：变身状态
    private bool isLeafFlying = false;
    private float flyTimer = 0;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();
        body.freezeRotation = true;
    }

    private void Update()
    {
        // 如果正在变身飞叶子 → 跳过原有操作
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

    // 核心功能：被叶子触发 → 变身飞上天
    public void TurnToLeafAndFly()
    {
        if (isLeafFlying) return;

        isLeafFlying = true;
        flyTimer = leafFlyTime;

        // 禁用玩家控制
        body.velocity = Vector2.zero;
        body.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;

        // 隐藏玩家，显示叶子效果
        spriteRenderer.enabled = false;
        ani.enabled = false;

        // 如果你有叶子预制体，会自动生成
        if (leafEffectPrefab != null)
        {
            Instantiate(leafEffectPrefab, transform);
        }
    }

    // 变身飞行逻辑
    void FlyAsLeaf()
    {
        // 向上飞 + 旋转
        transform.Translate(Vector2.up * leafFlySpeed * Time.deltaTime);
        transform.Rotate(0, 0, 120f * Time.deltaTime);

        // 计时结束 → 恢复玩家
        flyTimer -= Time.deltaTime;
        if (flyTimer <= 0)
        {
            RecoverFromLeaf();
        }
    }

    // 恢复玩家
    void RecoverFromLeaf()
    {
        isLeafFlying = false;

        // 恢复控制
        body.isKinematic = false;
        GetComponent<Collider2D>().enabled = true;

        // 恢复显示
        spriteRenderer.enabled = true;
        ani.enabled = true;
        transform.rotation = Quaternion.identity;

        // 销毁身上的叶子
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
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