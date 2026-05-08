using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFollowWithBounds : MonoBehaviour
{
    [Header("跟随目标（拖入你的Player角色）")]
    public Transform target;

    [Header("跟随设置")]
    [Tooltip("相机跟随的平滑度，数值越小越丝滑")]
    public float smoothSpeed = 0.15f;
    [Tooltip("相机相对于角色的偏移，比如(0,1,-10)可以让角色偏下一点，看得更远")]
    public Vector3 offset = new Vector3(0, 1, -10);

    [Header("场景边界限制（防止相机跑出地图）")]
    [Tooltip("相机能到达的最左边界")]
    public float minX;
    [Tooltip("相机能到达的最右边界")]
    public float maxX;
    [Tooltip("相机能到达的最低边界")]
    public float minY;
    [Tooltip("相机能到达的最高边界")]
    public float maxY;

    // 相机的正交半宽/半高，用来计算边界
    private float cameraHalfWidth;
    private float cameraHalfHeight;

    void Start()
    {
        // 计算相机的可视范围半宽和半高
        cameraHalfHeight = GetComponent<Camera>().orthographicSize;
        cameraHalfWidth = cameraHalfHeight * Screen.width / Screen.height;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 计算相机的目标位置
        Vector3 desiredPosition = target.position + offset;

        // 2. 平滑插值，让相机移动不生硬
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 3. 限制相机在场景边界内，不超出地图
        float clampedX = Mathf.Clamp(smoothedPosition.x, minX + cameraHalfWidth, maxX - cameraHalfWidth);
        float clampedY = Mathf.Clamp(smoothedPosition.y, minY + cameraHalfHeight, maxY - cameraHalfHeight);

        // 4. 应用最终位置，Z轴固定不变
        transform.position = new Vector3(clampedX, clampedY, offset.z);
    }

    // 用Gizmos在Scene视图里画出相机的边界，方便你调整
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}