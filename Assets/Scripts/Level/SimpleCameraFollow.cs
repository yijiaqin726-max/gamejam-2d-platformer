using UnityEngine;

public sealed class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, -10f);
    [SerializeField] private float smoothTime = 0.15f;

    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = false;

    [SerializeField] private bool useBounds = true;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 60f;
    [SerializeField] private float fixedY = 1.2f;

    private Vector3 velocity = Vector3.zero;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = transform.position;

        if (followX)
        {
            targetPosition.x = target.position.x + offset.x;
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            }
        }

        if (followY)
        {
            targetPosition.y = target.position.y + offset.y;
        }
        else
        {
            targetPosition.y = fixedY;
        }

        targetPosition.z = offset.z;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
