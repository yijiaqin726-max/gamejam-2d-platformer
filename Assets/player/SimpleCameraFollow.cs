using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("拖入你的Player角色")]
    public Transform target;
    [Header("跟随平滑度（0.1-0.3之间）")]
    public float smoothSpeed = 0.15f;
    [Header("相机偏移，让角色在画面里更舒服")]
    public Vector3 offset = new Vector3(0, 1, -10);

    void LateUpdate()
    {
        // 如果没给目标，直接不执行
        if (target == null) return;

        // 计算相机应该去的位置
        Vector3 desiredPosition = target.position + offset;
        // 平滑插值，让移动不生硬
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // 应用位置（Z轴固定死，防止相机乱跑）
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, offset.z);
    }
}
