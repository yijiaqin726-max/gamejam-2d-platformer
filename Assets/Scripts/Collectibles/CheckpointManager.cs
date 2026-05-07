using UnityEngine;

public sealed class CheckpointManager : MonoBehaviour
{
    private static CheckpointManager instance;
    private Vector3 currentCheckpoint;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            currentCheckpoint = playerObj.transform.position;
        }
        else
        {
            currentCheckpoint = Vector3.zero;
        }
    }

    public static CheckpointManager GetInstance()
    {
        if (instance == null)
        {
            GameObject managerObj = new GameObject("CheckpointManager");
            instance = managerObj.AddComponent<CheckpointManager>();
        }
        return instance;
    }

    public void SetCheckpoint(Vector3 position)
    {
        currentCheckpoint = position;
        Debug.Log($"Checkpoint set at: {position}");
    }

    public Vector3 GetCheckpoint()
    {
        return currentCheckpoint;
    }
}
