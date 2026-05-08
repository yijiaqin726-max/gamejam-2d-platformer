using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBird : MonoBehaviour
{
    [Header("飞行参数")]
    public float baseSpeed = 3f;
    public float speedVariation = 1f; // 速度随机波动范围
    public float verticalDrift = 0.5f; // 上下随机偏移的幅度

    private float moveSpeed;
    private float verticalOffset;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 随机速度（baseSpeed ± speedVariation）
        moveSpeed = baseSpeed + Random.Range(-speedVariation, speedVariation);
        // 随机上下偏移方向（让鸟有轻微的上下波动，不是完全水平）
        verticalOffset = Random.Range(-verticalDrift, verticalDrift);
    }

    void FixedUpdate()
    {
        // 向左飞 + 轻微上下偏移
        rb.velocity = new Vector2(-moveSpeed, verticalOffset);

        // 飞出屏幕左边界后自动销毁，防止场景堆太多对象
        if (transform.position.x < Camera.main.ScreenToWorldPoint(Vector3.zero).x - 2f)
        {
            Destroy(gameObject);
        }
    }
}
