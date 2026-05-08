using UnityEngine;
using System.Collections;

public sealed class ShadowDeathZone2D : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnCooldown = 0.5f;

    private bool isRespawning;

    private void Awake()
    {
        if (respawnPoint == null)
        {
            GameObject respawnObj = GameObject.Find("RespawnPoint");
            if (respawnObj != null)
            {
                respawnPoint = respawnObj.transform;
            }
            else
            {
                Debug.LogError("ShadowDeathZone2D: RespawnPoint not found. Please assign it in Inspector or ensure a GameObject named 'RespawnPoint' exists in the scene.");
            }
        }

        isRespawning = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isRespawning)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (respawnPoint == null)
        {
            Debug.LogError("ShadowDeathZone2D: RespawnPoint is null. Cannot respawn player.");
            return;
        }

        Debug.Log("Player touched shadow, respawning.");
        StartCoroutine(RespawnPlayer(other.gameObject));
    }

    private IEnumerator RespawnPlayer(GameObject player)
    {
        isRespawning = true;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        player.transform.position = respawnPoint.position;

        yield return new WaitForSeconds(respawnCooldown);

        isRespawning = false;
    }
}
