using UnityEngine;
using System.Collections;

/// <summary>
/// 乌鸦障碍生成器
/// 在三条固定高度的 lane 中随机生成乌鸦
/// 乌鸦从右向左移动
/// </summary>
public sealed class CrowLaneSpawner : MonoBehaviour
{
    [SerializeField] private CrowObstacle crowPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float[] laneYPositions = new float[] { -1f, 0.5f, 2f };
    [SerializeField] private float spawnIntervalMin = 1.2f;
    [SerializeField] private float spawnIntervalMax = 2.3f;
    [SerializeField] private float crowSpeed = 5f;
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
        if (crowPrefab == null || spawnPoint == null)
            return;

        // 随机选择一条 lane
        int laneIndex = Random.Range(0, laneYPositions.Length);
        float laneY = laneYPositions[laneIndex];

        // 实例化乌鸦
        CrowObstacle crow = Instantiate(crowPrefab, spawnPoint.position, Quaternion.identity);
        crow.transform.position = new Vector3(crow.transform.position.x, laneY, 0f);
        crow.SetMoveSpeed(crowSpeed);
    }
}
