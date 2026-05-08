using UnityEngine;

public sealed class LeafSequenceTrigger2D : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private BezierFallingLeaf guideLeaf;
    [SerializeField] private LeafFlightController leafFlightController;
    [SerializeField] private CrowLaneSpawner crowLaneSpawner;
    [SerializeField] private GameObject playerVisualRoot;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private float delayBeforeLeafMode = 0.2f;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered)
            return;

        if (!collision.CompareTag("Player"))
            return;

        Debug.Log("Leaf sequence triggered");

        if (player == null)
            player = collision.transform;

        hasTriggered = true;
        StartCoroutine(PlayLeafSequence());
    }

    private System.Collections.IEnumerator PlayLeafSequence()
    {
        if (playerController != null)
            playerController.enabled = false;

        if (guideLeaf != null)
            yield return StartCoroutine(guideLeaf.PlayFall());

        yield return new WaitForSeconds(delayBeforeLeafMode);

        Debug.Log("Entering leaf flight mode");

        if (playerController != null)
            playerController.enabled = false;

        ClearPlayerVelocity();

        if (playerVisualRoot != null)
            SetPlayerRenderersEnabled(false);

        if (leafFlightController != null)
        {
            Debug.Log("Calling LeafFlightController.BeginFlight");
            leafFlightController.BeginFlight();
        }
        else
        {
            Debug.LogError("LeafSequenceTrigger2D: leafFlightController is missing");
        }

        if (crowLaneSpawner != null)
            crowLaneSpawner.StartSpawning();
    }

    private void ClearPlayerVelocity()
    {
        if (player == null)
            return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void SetPlayerRenderersEnabled(bool enabled)
    {
        SpriteRenderer[] renderers = playerVisualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = enabled;
    }
}
