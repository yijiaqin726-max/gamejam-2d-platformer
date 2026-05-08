using UnityEngine;

/// <summary>
/// 乌鸦障碍
/// 从右向左移动，碰到叶子则触发复位
/// 飞出屏幕后自销毁
/// </summary>
public sealed class CrowObstacle : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float destroyX = -30f;

    private float currentSpeed;
    private bool hasHitLeaf;

    private void Start()
    {
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        // 向左移动
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime, Space.World);

        // 飞出屏幕后销毁
        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHitLeaf)
            return;

        LeafFlightController leafFlight = collision.GetComponentInParent<LeafFlightController>();
        if (leafFlight == null)
            return;

        hasHitLeaf = true;
        Debug.Log("Leaf hit crow, resetting");
        leafFlight.ResetFlightToStart();
        Destroy(gameObject);
    }

    public void SetMoveSpeed(float speed)
    {
        currentSpeed = speed;
    }
}
