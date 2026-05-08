using UnityEngine;
using UnityEngine.InputSystem;

public sealed class LeafFlightController : MonoBehaviour
{
    [SerializeField] private float normalForwardSpeed = 4f;
    [SerializeField] private float burstForwardSpeed = 12f;
    [SerializeField] private float burstDuration = 0.8f;
    [SerializeField] private float burstUpwardSpeed = 2f;
    [SerializeField] private AnimationCurve burstSpeedCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private float verticalSpeed = 5f;
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 3f;
    [SerializeField] private Transform flightStartPoint;
    [SerializeField] private GameObject leafVisualRoot;
    [SerializeField] private GameObject playerVisualRoot;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private bool useWAndS = true;
    [SerializeField] private bool useUpDownArrow = true;
    [SerializeField] private bool startDisabled = true;

    private bool isFlying;
    private float burstTimer;
    private Vector3 flightStartPos;

    private void Start()
    {
        if (startDisabled)
        {
            isFlying = false;
            enabled = false;
        }
    }

    private void Update()
    {
        if (!isFlying)
            return;

        float vertical = GetVerticalInput();
        float currentForwardSpeed = GetCurrentForwardSpeed();

        Vector3 pos = transform.position;
        pos.x += currentForwardSpeed * Time.deltaTime;
        pos.y += vertical * verticalSpeed * Time.deltaTime;

        if (burstTimer < burstDuration)
            pos.y += burstUpwardSpeed * Time.deltaTime;

        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    public void BeginFlight()
    {
        Debug.Log("Leaf flight begin");

        gameObject.SetActive(true);
        enabled = true;
        isFlying = true;
        burstTimer = 0f;

        if (flightStartPoint != null)
        {
            flightStartPos = flightStartPoint.position;
            transform.position = flightStartPos;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (playerController != null)
            playerController.enabled = false;

        if (leafVisualRoot != null)
            leafVisualRoot.SetActive(true);

        if (playerVisualRoot != null)
            SetPlayerRenderersEnabled(false);
    }

    public void ResetFlightToStart()
    {
        if (flightStartPoint != null)
            transform.position = flightStartPos;
    }

    public void EndFlight()
    {
        isFlying = false;
        burstTimer = 0f;

        if (leafVisualRoot != null)
            leafVisualRoot.SetActive(false);

        if (playerVisualRoot != null)
            SetPlayerRenderersEnabled(true);

        if (playerController != null)
            playerController.enabled = true;

        gameObject.SetActive(false);
    }

    private float GetVerticalInput()
    {
        float vertical = 0f;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return vertical;

        if (useWAndS)
        {
            if (keyboard.wKey.isPressed)
                vertical += 1f;
            if (keyboard.sKey.isPressed)
                vertical -= 1f;
        }

        if (useUpDownArrow)
        {
            if (keyboard.upArrowKey.isPressed)
                vertical += 1f;
            if (keyboard.downArrowKey.isPressed)
                vertical -= 1f;
        }

        return Mathf.Clamp(vertical, -1f, 1f);
    }

    private float GetCurrentForwardSpeed()
    {
        float currentForwardSpeed = normalForwardSpeed;
        if (burstTimer < burstDuration)
        {
            float t = burstDuration > 0f ? Mathf.Clamp01(burstTimer / burstDuration) : 1f;
            float curveValue = burstSpeedCurve != null ? burstSpeedCurve.Evaluate(t) : 1f - t;
            currentForwardSpeed = Mathf.Lerp(normalForwardSpeed, burstForwardSpeed, Mathf.Clamp01(curveValue));
            burstTimer += Time.deltaTime;
        }

        return currentForwardSpeed;
    }

    private void SetPlayerRenderersEnabled(bool enabled)
    {
        SpriteRenderer[] renderers = playerVisualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = enabled;
    }
}
