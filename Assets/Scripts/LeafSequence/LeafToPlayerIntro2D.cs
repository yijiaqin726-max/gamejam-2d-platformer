using System.Collections;
using UnityEngine;

public sealed class LeafToPlayerIntro2D : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject playerVisualRoot;
    [SerializeField] private PrototypePlayerController playerController;
    [SerializeField] private PrototypeFrameAnimator playerAnimator;
    [SerializeField] private Rigidbody2D playerRigidbody;

    [SerializeField] private GameObject introLeaf;
    [SerializeField] private SpriteRenderer introLeafRenderer;
    [SerializeField] private Transform leafStartPoint;
    [SerializeField] private Transform leafEndPoint;

    [SerializeField] private float flyDuration = 1.5f;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float swayAmplitude = 0.2f;
    [SerializeField] private float swayFrequency = 2f;
    [SerializeField] private bool playOnStart = true;

    private Vector3 introLeafInitialScale = Vector3.one;

    private void Start()
    {
        ResolveReferences();

        if (playOnStart)
            StartCoroutine(PlayIntro());
    }

    public void Play()
    {
        ResolveReferences();
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        Debug.Log("Leaf to player intro begin");

        if (playerController != null)
            playerController.enabled = false;

        ClearPlayerVelocity();
        SetPlayerRenderersEnabled(false);

        if (introLeaf == null || leafStartPoint == null || leafEndPoint == null)
        {
            RestorePlayer();
            yield break;
        }

        introLeaf.SetActive(true);
        introLeaf.transform.position = leafStartPoint.position;
        introLeafInitialScale = introLeaf.transform.localScale;

        if (introLeafRenderer != null)
        {
            Color color = introLeafRenderer.color;
            color.a = 1f;
            introLeafRenderer.color = color;
        }

        float elapsedTime = 0f;
        while (elapsedTime < flyDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / Mathf.Max(0.01f, flyDuration));
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 pos = Vector3.Lerp(leafStartPoint.position, leafEndPoint.position, easedT);
            pos.x += Mathf.Sin(t * Mathf.PI * 2f * swayFrequency) * swayAmplitude;
            introLeaf.transform.position = pos;
            introLeaf.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 2f * swayFrequency) * 10f);

            ClearPlayerVelocity();
            yield return null;
        }

        introLeaf.transform.position = leafEndPoint.position;
        Debug.Log("Leaf reached player point");

        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / Mathf.Max(0.01f, fadeDuration));
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            introLeaf.transform.localScale = Vector3.Lerp(introLeafInitialScale, Vector3.zero, easedT);
            if (introLeafRenderer != null)
            {
                Color color = introLeafRenderer.color;
                color.a = 1f - easedT;
                introLeafRenderer.color = color;
            }

            ClearPlayerVelocity();
            yield return null;
        }

        introLeaf.SetActive(false);
        RestorePlayer();
    }

    private void RestorePlayer()
    {
        SetPlayerRenderersEnabled(true);

        if (playerAnimator != null)
            playerAnimator.SetState(PrototypeFrameAnimator.MotionState.Idle, true);

        if (playerController != null)
            playerController.enabled = true;

        Debug.Log("Player restored from leaf");
    }

    private void ResolveReferences()
    {
        if (playerController == null && player != null)
            playerController = player.GetComponent<PrototypePlayerController>();

        if (playerAnimator == null && player != null)
        {
            playerAnimator = player.GetComponent<PrototypeFrameAnimator>();
            if (playerAnimator == null)
                playerAnimator = player.GetComponentInChildren<PrototypeFrameAnimator>();
        }

        if (playerRigidbody == null && player != null)
            playerRigidbody = player.GetComponent<Rigidbody2D>();

        if (introLeafRenderer == null && introLeaf != null)
            introLeafRenderer = introLeaf.GetComponent<SpriteRenderer>();

        if (playerVisualRoot == null && player != null)
            playerVisualRoot = player.gameObject;
    }

    private void ClearPlayerVelocity()
    {
        if (playerRigidbody == null)
            return;

        playerRigidbody.linearVelocity = Vector2.zero;
        playerRigidbody.angularVelocity = 0f;
    }

    private void SetPlayerRenderersEnabled(bool enabled)
    {
        if (playerVisualRoot == null)
            return;

        SpriteRenderer[] renderers = playerVisualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = enabled;
    }
}
