using UnityEngine;
using System.Collections;

public sealed class CrowLaneSpawner : MonoBehaviour
{
    [SerializeField] private CrowObstacle crowPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform followTarget;
    [SerializeField] private float spawnAheadDistance = 14f;
    [SerializeField] private float[] laneYPositions = new float[] { -1f, 0.5f, 2f };
    [SerializeField] private float spawnIntervalMin = 2.0f;
    [SerializeField] private float spawnIntervalMax = 3.2f;
    [SerializeField] private float crowSpeed = 9f;
    [SerializeField] private float startDelay = 1.0f;
    [SerializeField] private bool autoStart = false;

    private bool isSpawning;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        if (autoStart)
            StartSpawning();
    }

    public void StartSpawning()
    {
        if (isSpawning)
            return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (!isSpawning)
            return;

        isSpawning = false;
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
    }

    private IEnumerator SpawnLoop()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        while (isSpawning)
        {
            float interval = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(interval);

            if (!isSpawning)
                break;

            SpawnCrow();
        }
    }

    private void SpawnCrow()
    {
        if (crowPrefab == null || laneYPositions == null || laneYPositions.Length == 0)
            return;

        int laneIndex = Random.Range(0, laneYPositions.Length);
        float laneY = laneYPositions[laneIndex];
        Vector3 spawnPosition;

        if (followTarget != null)
        {
            float spawnX = followTarget.position.x + spawnAheadDistance;
            spawnPosition = new Vector3(spawnX, laneY, 0f);
        }
        else
        {
            if (spawnPoint == null)
                return;

            spawnPosition = spawnPoint.position;
            spawnPosition.y = laneY;
            spawnPosition.z = 0f;
        }

        CrowObstacle crow = Instantiate(crowPrefab, spawnPosition, Quaternion.identity);
        crow.SetMoveSpeed(crowSpeed);
    }
}
