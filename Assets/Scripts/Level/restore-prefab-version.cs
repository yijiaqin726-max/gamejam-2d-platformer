using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Offset")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Follow")]
    public float smoothTime = 0.15f;
    public bool followX = true;
    public bool followY = false;

    [Header("Bounds")]
    public bool useBounds = true;
    public float minX = -10f;
    public float maxX = 60f;
    public float fixedY = 0f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 current = transform.position;

        float targetX = followX ? target.position.x + offset.x : current.x;
        float targetY = followY ? target.position.y + offset.y : fixedY;
        float targetZ = offset.z;

        if (useBounds)
        {
            targetX = Mathf.Clamp(targetX, minX, maxX);
        }

        Vector3 desired = new Vector3(targetX, targetY, targetZ);

        transform.position = Vector3.SmoothDamp(
            current,
            desired,
            ref velocity,
            smoothTime
        );
    }
}