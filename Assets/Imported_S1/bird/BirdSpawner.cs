using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSpawner : MonoBehaviour
{
    public GameObject birdPrefab;
    public Transform player;

    public float spawnInterval = 2.5f;
    public float spawnX = 950f;
    public float minY = 12f;
    public float maxY = 58f;

    private float timer;

    void Update()
    {
        if (player == null) return;

        PrototypePlayerController pc = player.GetComponent<PrototypePlayerController>();
        if (pc != null && pc.IsLeafFlying())
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnBird();
                timer = 0;
            }
        }
    }

    void SpawnBird()
    {
        float y = Random.Range(minY, maxY);
        Instantiate(birdPrefab, new Vector3(spawnX, y, 0), Quaternion.identity);
    }
}