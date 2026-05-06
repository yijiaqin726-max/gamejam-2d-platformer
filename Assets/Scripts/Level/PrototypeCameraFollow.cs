using UnityEngine;

public sealed class PrototypeCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.18f;
    [SerializeField] private float fixedY;
    [SerializeField] private float minX;
    [SerializeField] private float maxX;

    private Vector3 velocity;

    public void Configure(Transform followTarget, float cameraY, float leftBound, float rightBound)
    {
        target = followTarget;
        fixedY = cameraY;
        minX = leftBound;
        maxX = rightBound;
        if (target != null)
        {
            var start = transform.position;
            start.x = Mathf.Clamp(target.position.x, minX, maxX);
            start.y = fixedY;
            transform.position = start;
            velocity = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        var desired = new Vector3(
            Mathf.Clamp(target.position.x, minX, maxX),
            fixedY,
            transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
