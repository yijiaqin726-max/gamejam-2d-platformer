using UnityEngine;

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

    private const float BurstVerticalControlMultiplier = 0.65f;

    private bool isFlying;
    private bool isBursting;
    private float burstElapsedTime;
    private Vector3 flightStartPos;

    private void OnEnable()
    {
        isFlying = false;
        isBursting = false;
        burstElapsedTime = 0f;
    }

    private void Update()
    {
        if (!isFlying)
            return;

        float currentForwardSpeed = normalForwardSpeed;
        float upwardSpeed = 0f;
        float verticalControlMultiplier = 1f;

        if (isBursting)
        {
            burstElapsedTime += Time.deltaTime;
            float burstT = burstDuration > 0f ? Mathf.Clamp01(burstElapsedTime / burstDuration) : 1f;
            float burstWeight = GetBurstWeight(burstT);

            currentForwardSpeed = Mathf.Lerp(normalForwardSpeed, burstForwardSpeed, burstWeight);
            upwardSpeed = burstUpwardSpeed * burstWeight;
            verticalControlMultiplier = BurstVerticalControlMultiplier;

            if (burstT >= 1f)
                isBursting = false;
        }

        float verticalInput = GetVerticalInput();
        Vector3 pos = transform.position;
        pos.x += currentForwardSpeed * Time.deltaTime;
        pos.y += (upwardSpeed + verticalInput * verticalSpeed * verticalControlMultiplier) * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    public void BeginFlight()
    {
        if (flightStartPoint != null)
        {
            flightStartPos = flightStartPoint.position;
            transform.position = flightStartPos;
        }

        if (leafVisualRoot != null)
            leafVisualRoot.SetActive(true);

        if (playerVisualRoot != null)
            playerVisualRoot.SetActive(false);

        isFlying = true;
        isBursting = true;
        burstElapsedTime = 0f;
        gameObject.SetActive(true);
    }

    public void ResetFlightToStart()
    {
        if (flightStartPoint != null)
            transform.position = flightStartPos;
    }

    public void EndFlight()
    {
        isFlying = false;
        isBursting = false;

        if (leafVisualRoot != null)
            leafVisualRoot.SetActive(false);

        if (playerVisualRoot != null)
            playerVisualRoot.SetActive(true);

        if (playerController != null)
            playerController.enabled = true;

        gameObject.SetActive(false);
    }

    private float GetVerticalInput()
    {
        float verticalInput = 0f;
        if (useWAndS)
        {
            if (Input.GetKey(KeyCode.W))
                verticalInput += 1f;
            if (Input.GetKey(KeyCode.S))
                verticalInput -= 1f;
        }

        if (useUpDownArrow)
            verticalInput += Input.GetAxisRaw("Vertical");

        return Mathf.Clamp(verticalInput, -1f, 1f);
    }

    private float GetBurstWeight(float burstT)
    {
        if (burstSpeedCurve != null && burstSpeedCurve.length > 0)
            return Mathf.Clamp01(burstSpeedCurve.Evaluate(burstT));

        return Mathf.SmoothStep(1f, 0f, burstT);
    }
}
