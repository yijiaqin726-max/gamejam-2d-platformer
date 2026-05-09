using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CircleCollider2D))]
public sealed class CollectibleCheckpointOrb : MonoBehaviour
{
    [SerializeField] private float floatAmplitude = 0.12f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.12f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private Color activeColor = new Color(1f, 0.843f, 0f, 1f);
    [SerializeField] private AudioClip collectSfx;
    [SerializeField] private float collectVolume = 0.7f;
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float shrinkAmount = 0.65f;

    private SpriteRenderer coreRenderer;
    private SpriteRenderer glowRenderer;
    private AudioSource audioSource;
    private CircleCollider2D circleCollider;
    private Vector3 startPosition;
    private Vector3 startScale;
    private bool isCollected;
    private float elapsedTime;

    private void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;

        Transform coreTransform = transform.Find("Visual_Core");
        Transform glowTransform = transform.Find("Visual_Glow");

        if (coreTransform != null)
        {
            coreRenderer = coreTransform.GetComponent<SpriteRenderer>();
        }

        if (glowTransform != null)
        {
            glowRenderer = glowTransform.GetComponent<SpriteRenderer>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }

        startPosition = transform.position;
        startScale = transform.localScale;
        isCollected = false;
        elapsedTime = 0f;

        if (coreRenderer != null)
        {
            coreRenderer.color = activeColor;
        }
        if (glowRenderer != null)
        {
            glowRenderer.color = activeColor;
        }
    }

    private void Update()
    {
        if (isCollected)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        float floatOffset = Mathf.Sin(elapsedTime * floatSpeed * Mathf.PI) * floatAmplitude;
        transform.position = startPosition + Vector3.up * floatOffset;

        float pulseScale = 1f + Mathf.Sin(elapsedTime * pulseSpeed * Mathf.PI) * pulseAmount;
        transform.localScale = startScale * pulseScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected)
        {
            return;
        }

        if (IsPlayerOrLightOrb(collision))
        {
            Collect(collision);
        }
    }

    private void Collect(Collider2D collision)
    {
        isCollected = true;

        CheckpointManager.GetInstance().SetCheckpoint(transform.position);
        Debug.Log($"Checkpoint orb collected at: {transform.position}");
        GrowLightOrb(collision);

        if (audioSource != null && collectSfx != null)
        {
            audioSource.PlayOneShot(collectSfx, collectVolume);
        }

        circleCollider.enabled = false;
        StartCoroutine(FadeAndDestroy());
    }

    private bool IsPlayerOrLightOrb(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            return true;

        return collision.GetComponentInParent<PrototypePlayerController>() != null
            || collision.GetComponentInParent<LightOrbFreeMoveController>() != null
            || collision.GetComponentInParent<LightOrbGrowthController>() != null;
    }

    private void GrowLightOrb(Collider2D collision)
    {
        LightOrbGrowthController growthController = collision.GetComponentInParent<LightOrbGrowthController>();
        if (growthController == null)
            return;

        growthController.GrowOnce();
    }

    private IEnumerator FadeAndDestroy()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        Color[] initialColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            initialColors[i] = renderers[i].color;
        }

        Vector3 initialScale = transform.localScale;
        float elapsedFade = 0f;

        while (elapsedFade < fadeDuration)
        {
            elapsedFade += Time.deltaTime;
            float t = elapsedFade / fadeDuration;

            for (int i = 0; i < renderers.Length; i++)
            {
                Color color = initialColors[i];
                color.a = Mathf.Lerp(initialColors[i].a, 0f, t);
                renderers[i].color = color;
            }

            float scale = Mathf.Lerp(1f, shrinkAmount, t);
            transform.localScale = initialScale * scale;

            yield return null;
        }

        Destroy(gameObject);
    }
}
