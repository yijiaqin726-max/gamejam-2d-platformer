using UnityEngine;

public class RaisedFloor2D : MonoBehaviour
{
    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private float raiseSpeed = 2f;
    private bool isRaising = false;

    public void Initialize(Vector3 startPos, Vector3 endPos)
    {
        initialPosition = startPos;
        targetPosition = endPos;
        transform.position = initialPosition;
    }

    public void Raise()
    {
        if (!isRaising)
        {
            isRaising = true;
            Debug.Log($"[RaisedFloor] Starting raise from {initialPosition} to {targetPosition}");
        }
    }

    private void Update()
    {
        if (isRaising)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, raiseSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isRaising = false;
                Debug.Log($"[RaisedFloor] Reached target position: {targetPosition}");
            }
        }
    }
}
