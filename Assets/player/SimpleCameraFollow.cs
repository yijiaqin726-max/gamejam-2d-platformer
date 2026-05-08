using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("跟随设置")]
    public float smoothSpeed = 0.15f;
    public Vector3 offset = new Vector3(0, 1, -10);

    void LateUpdate()
    {
        if (target == null) return; // 没有目标就不执行，避免报错

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}