using UnityEngine;

public class ShadowDeathZone : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private static Vector3 defaultRespawnPosition;

    public static void SetDefaultRespawnPosition(Vector3 pos)
    {
        defaultRespawnPosition = pos;
    }

    private void Awake()
    {
        if (respawnPoint == null)
        {
            var rp = GameObject.Find("RespawnPoint");
            if (rp != null)
            {
                respawnPoint = rp.transform;
            }
        }

        if (respawnPoint != null)
        {
            SetDefaultRespawnPosition(respawnPoint.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.gameObject;
        var targetPos = respawnPoint != null ? respawnPoint.position : defaultRespawnPosition;

        // Reset velocity
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Respawn
        player.transform.position = targetPos;

        // Flash effect
        StartCoroutine(FlashPlayer(player));
    }

    private System.Collections.IEnumerator FlashPlayer(GameObject player)
    {
        var renderer = player.GetComponentInChildren<SpriteRenderer>();
        if (renderer == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            renderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            renderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
