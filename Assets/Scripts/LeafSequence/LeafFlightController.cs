using UnityEngine;
using UnityEngine.InputSystem;

public sealed class LeafFlightController : MonoBehaviour
{
    [SerializeField] private float normalForwardSpeed = 4f;
    [SerializeField] private float burstForwardSpeed = 12f;
    [SerializeField] private float burstDuration = 0.8f;
    [SerializeField] private float burstUpwardSpeed = 2f;
    [SerializeField] private float verticalSpeed = 6f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 6f;
    [SerializeField] private Transform flightStartPoint;
    [SerializeField] private GameObject leafVisualRoot;
    [SerializeField] private GameObject playerVisualRoot;
    [SerializeField] private PrototypePlayerController playerController;
    [SerializeField] private bool useWAndS = true;
    [SerializeField] private bool useUpDownArrow = true;

    private bool isFlying = false;
    private float burstTimer = 0f;

    private void Start()
    {
        isFlying = false;

        if (leafVisualRoot != null)
            leafVisualRoot.SetActive(false);
    }

    private void Update()
    {
        if (!isFlying)
            return;

        float vertical = GetVerticalInput();

        float currentForwardSpeed = normalForwardSpeed;
        if (burstTimer < burstDuration)
        {
            float t = burstTimer / Mathf.Max(0.01f, burstDuration);
            float curve = 1f - Mathf.SmoothStep(0f, 1f, t);
            currentForwardSpeed = Mathf.Lerp(normalForwardSpeed, burstForwardSpeed, curve);
            burstTimer += Time.deltaTime;
        }

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

        isFlying = true;
        burstTimer = 0f;

        if (flightStartPoint != null)
            transform.position = flightStartPoint.position;

        if (leafVisualRoot != null)
            leafVisualRoot.SetActive(true);

        if (playerController != null)
            playerController.enabled = false;

        if (playerVisualRoot != null)
            SetPlayerRenderersEnabled(false);

        Debug.Log("LeafFlightRoot position after BeginFlight: " + transform.position);
    }

    public void ResetFlightToStart()
    {
        if (flightStartPoint != null)
            transform.position = flightStartPoint.position;
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

        if (keyboard != null)
        {
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
        }

        return Mathf.Clamp(vertical, -1f, 1f);
    }

    private void SetPlayerRenderersEnabled(bool enabled)
    {
        SpriteRenderer[] renderers = playerVisualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = enabled;
    }
}
