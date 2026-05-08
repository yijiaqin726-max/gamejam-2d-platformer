using System.Collections;
using System.Reflection;
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

    private static readonly FieldInfo GuideLeafEndPointField = typeof(BezierFallingLeaf).GetField(
        "endPoint",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private bool hasTriggered;
    private Vector3 lockedPlayerPosition;
    private Rigidbody2D lockedPlayerRb;
    private PrototypeFrameAnimator lockedPlayerAnimator;
    private bool hasLoggedForcedIdle;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered)
            return;

        if (!collision.CompareTag("Player"))
            return;

        Debug.Log("Leaf sequence triggered");

        if (player == null)
            player = collision.transform;

        LockPlayerAtCurrentPosition();

        hasTriggered = true;
        StartCoroutine(PlayLeafSequence());
    }

    private IEnumerator PlayLeafSequence()
    {
        Vector3 leafStartPosition = lockedPlayerPosition + new Vector3(0f, 0.5f, 0f);
        AlignGuideLeafEndPoint(leafStartPosition);

        if (guideLeaf != null)
        {
            IEnumerator fallRoutine = guideLeaf.PlayFall();
            bool loggedForcedPosition = false;
            while (fallRoutine.MoveNext())
            {
                ForceLockedPlayerPosition();
                if (!loggedForcedPosition)
                {
                    Debug.Log("Player transform forced during leaf fall");
                    loggedForcedPosition = true;
                }

                yield return fallRoutine.Current;
            }
        }

        float delayTimer = 0f;
        while (delayTimer < delayBeforeLeafMode)
        {
            ForceLockedPlayerPosition();
            delayTimer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Entering leaf flight mode");

        ForceLockedPlayerPosition();

        if (playerVisualRoot != null)
            SetPlayerRenderersEnabled(false);

        if (leafFlightController != null)
        {
            HideGuideLeafVisual();
            Debug.Log("Calling LeafFlightController.BeginFlight");
            Debug.Log("Leaf transform begins at locked player position");
            leafFlightController.BeginFlight(leafStartPosition);
            SwitchCameraToLeafFlightRoot();
        }
        else
        {
            Debug.LogError("LeafSequenceTrigger2D: leafFlightController is missing");
        }

        if (crowLaneSpawner != null)
            crowLaneSpawner.StartSpawning();
    }

    private void LockPlayerAtCurrentPosition()
    {
        if (player == null)
            return;

        lockedPlayerPosition = player.position;
        lockedPlayerRb = player.GetComponent<Rigidbody2D>();
        lockedPlayerAnimator = player.GetComponent<PrototypeFrameAnimator>();
        if (lockedPlayerAnimator == null)
            lockedPlayerAnimator = player.GetComponentInChildren<PrototypeFrameAnimator>();

        if (playerController == null)
        {
            playerController = player.GetComponent<PrototypePlayerController>();
            if (playerController == null)
                playerController = player.GetComponentInChildren<PrototypePlayerController>();
        }

        if (playerController != null)
            playerController.enabled = false;

        ClearPlayerVelocity();
        ForcePlayerIdle();
        player.position = lockedPlayerPosition;

        Debug.Log("Player locked at: " + lockedPlayerPosition);
    }

    private void ForceLockedPlayerPosition()
    {
        if (player != null)
            player.position = lockedPlayerPosition;

        ClearPlayerVelocity();
        ForcePlayerIdle();
    }

    private void ForcePlayerIdle()
    {
        if (lockedPlayerAnimator == null)
            return;

        lockedPlayerAnimator.SetState(PrototypeFrameAnimator.MotionState.Idle, true);
        if (!hasLoggedForcedIdle)
        {
            Debug.Log("Player forced to idle during leaf transform");
            hasLoggedForcedIdle = true;
        }
    }

    private void AlignGuideLeafEndPoint(Vector3 endPosition)
    {
        if (guideLeaf == null || GuideLeafEndPointField == null)
            return;

        Transform endPoint = GuideLeafEndPointField.GetValue(guideLeaf) as Transform;
        if (endPoint != null)
            endPoint.position = endPosition;
    }

    private void HideGuideLeafVisual()
    {
        if (guideLeaf == null)
            return;

        SpriteRenderer[] renderers = guideLeaf.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = false;

        Animator[] animators = guideLeaf.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
            animators[i].enabled = false;

        Debug.Log("Guide leaf hidden after transform");
    }

    private void SwitchCameraToLeafFlightRoot()
    {
        SimpleCameraFollow cameraFollow = FindAnyObjectByType<SimpleCameraFollow>();
        if (cameraFollow == null)
        {
            Debug.LogWarning("LeafSequenceTrigger2D: SimpleCameraFollow not found");
            return;
        }

        cameraFollow.SetTarget(leafFlightController.transform);
        Debug.Log("Camera now follows LeafFlightRoot");
    }

    private void ClearPlayerVelocity()
    {
        if (lockedPlayerRb == null && player != null)
            lockedPlayerRb = player.GetComponent<Rigidbody2D>();

        if (lockedPlayerRb == null)
            return;

        lockedPlayerRb.linearVelocity = Vector2.zero;
        lockedPlayerRb.angularVelocity = 0f;
    }

    private void SetPlayerRenderersEnabled(bool enabled)
    {
        SpriteRenderer[] renderers = playerVisualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = enabled;
    }
}
