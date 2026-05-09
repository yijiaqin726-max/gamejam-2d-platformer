using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class LightOrbFreeMoveController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private CircleCollider2D lightOrbCollider;
    [SerializeField] private bool useLightOrbCollider = false;

    private Rigidbody2D cachedRigidbody;
    private Collider2D[] cachedColliders;
    private float originalGravityScale;
    private RigidbodyConstraints2D originalConstraints;
    private bool[] originalColliderEnabled;
    private bool[] originalColliderIsTrigger;
    private bool isControlEnabled;
    private bool hasLoggedMoveInput;
    private Vector2 moveInput;

    private void Awake()
    {
        // 缓存物理组件。建议把本脚本挂在 Player 根物体上。
        cachedRigidbody = GetComponent<Rigidbody2D>();
        cachedColliders = GetComponentsInChildren<Collider2D>(true);

        if (cachedRigidbody != null)
        {
            originalGravityScale = cachedRigidbody.gravityScale;
            originalConstraints = cachedRigidbody.constraints;
        }

        CacheOriginalColliderState();

        if (lightOrbCollider != null && useLightOrbCollider)
            lightOrbCollider.enabled = false;

        enabled = false;
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        hasLoggedMoveInput = false;
    }

    private void Update()
    {
        if (!isControlEnabled)
            return;

        // 只读取上下左右移动，不读取跳跃。
        moveInput = ReadMoveInput();
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
    }

    private void FixedUpdate()
    {
        if (!isControlEnabled || cachedRigidbody == null)
            return;

        // 每次都只保留旋转冻结，避免 Y 轴位置被冻结导致 W/S 无效。
        cachedRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        cachedRigidbody.gravityScale = 0f;
        cachedRigidbody.linearVelocity = moveInput * moveSpeed;

        if (!hasLoggedMoveInput && moveInput.sqrMagnitude > 0f)
        {
            Debug.Log("LightOrbFreeMoveController moveInput=" + moveInput);
            hasLoggedMoveInput = true;
        }
    }

    public void EnableLightOrbControl()
    {
        if (cachedRigidbody != null)
        {
            cachedRigidbody.gravityScale = 0f;
            cachedRigidbody.linearVelocity = Vector2.zero;
            cachedRigidbody.angularVelocity = 0f;
            cachedRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log("LightOrbFreeMoveController constraints=" + cachedRigidbody.constraints + ", gravityScale=" + cachedRigidbody.gravityScale);
        }

        ApplyLightOrbColliderState();
        moveInput = Vector2.zero;
        hasLoggedMoveInput = false;
        isControlEnabled = true;
        enabled = true;
    }

    public void DisableLightOrbControl()
    {
        isControlEnabled = false;
        moveInput = Vector2.zero;

        if (cachedRigidbody != null)
        {
            cachedRigidbody.gravityScale = originalGravityScale;
            cachedRigidbody.linearVelocity = Vector2.zero;
            cachedRigidbody.angularVelocity = 0f;
            cachedRigidbody.constraints = originalConstraints;
        }

        RestoreColliderState();
        enabled = false;
    }

    private Vector2 ReadMoveInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                horizontal -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                horizontal += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                vertical -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                vertical += 1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Mathf.Approximately(horizontal, 0f))
            horizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Approximately(vertical, 0f))
            vertical = Input.GetAxisRaw("Vertical");
#endif

        return new Vector2(horizontal, vertical);
    }

    private void ApplyLightOrbColliderState()
    {
        if (cachedColliders == null)
            return;

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] == null)
                continue;

            // 光点形态不穿墙：Collider 必须保持启用且不是 Trigger。
            cachedColliders[i].enabled = true;
            cachedColliders[i].isTrigger = false;
        }

        // 可选：如果单独配置了小球碰撞体，则光点形态启用它。
        if (lightOrbCollider != null && useLightOrbCollider)
        {
            lightOrbCollider.enabled = true;
            lightOrbCollider.isTrigger = false;
        }
    }

    private void CacheOriginalColliderState()
    {
        if (cachedColliders == null)
            return;

        originalColliderEnabled = new bool[cachedColliders.Length];
        originalColliderIsTrigger = new bool[cachedColliders.Length];

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] == null)
                continue;

            originalColliderEnabled[i] = cachedColliders[i].enabled;
            originalColliderIsTrigger[i] = cachedColliders[i].isTrigger;
        }
    }

    private void RestoreColliderState()
    {
        if (cachedColliders == null || originalColliderEnabled == null || originalColliderIsTrigger == null)
            return;

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] == null)
                continue;

            cachedColliders[i].enabled = originalColliderEnabled[i];
            cachedColliders[i].isTrigger = originalColliderIsTrigger[i];
        }
    }
}
