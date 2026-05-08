using UnityEngine;

/// <summary>
/// 控制叶子飞行形态
/// 自动向右飞，玩家上下控制躲避乌鸦
/// </summary>
public sealed class LeafFlightController : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 4f;
    [SerializeField] private float verticalSpeed = 5f;
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 3f;
    [SerializeField] private Transform flightStartPoint;
    [SerializeField] private GameObject leafVisualRoot;
    [SerializeField] private GameObject playerVisualRoot;
    [SerializeField] private MonoBehaviour playerController;
    [SerializeField] private bool useWAndS = true;
    [SerializeField] private bool useUpDownArrow = true;

    private bool isFlying;
    private Vector3 flightStartPos;

    private void OnEnable()
    {
        isFlying = false;
    }

    private void Update()
    {
        if (!isFlying)
            return;

        // 自动向右飞
        transform.Translate(Vector3.right * forwardSpeed * Time.deltaTime, Space.World);

        // 玩家上下控制
        float verticalInput = 0f;
        if (useWAndS)
        {
            if (Input.GetKey(KeyCode.W))
                verticalInput += 1f;
            if (Input.GetKey(KeyCode.S))
                verticalInput -= 1f;
        }
        if (useUpDownArrow)
        {
            verticalInput += Input.GetAxisRaw("Vertical");
        }

        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.y += verticalInput * verticalSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }
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

        if (leafVisualRoot != null)
            leafVisualRoot.SetActive(false);

        if (playerVisualRoot != null)
            playerVisualRoot.SetActive(true);

        if (playerController != null)
            playerController.enabled = true;

        gameObject.SetActive(false);
    }
}
