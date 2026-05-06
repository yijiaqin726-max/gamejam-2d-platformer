using UnityEngine;

public class RaiseFloorTrigger2D : MonoBehaviour
{
    private RaisedFloor2D raisedFloor;

    public void SetRaisedFloor(RaisedFloor2D floor)
    {
        raisedFloor = floor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var stone = collision.GetComponent<PushableStone2D>();
        if (stone != null && raisedFloor != null)
        {
            raisedFloor.Raise();
            Debug.Log("[RaiseFloorTrigger] Stone entered trigger, raising floor");
        }
    }
}
