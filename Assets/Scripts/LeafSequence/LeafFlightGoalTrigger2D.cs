using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 飞行段终点 Trigger
/// 叶子碰到后结束飞行并切下一关
/// </summary>
public sealed class LeafFlightGoalTrigger2D : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private float delayBeforeLoad = 0.2f;
    [SerializeField] private CrowLaneSpawner crowLaneSpawner;
    [SerializeField] private LeafFlightController leafFlightController;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered)
            return;

        LeafFlightController leafFlight = collision.GetComponentInParent<LeafFlightController>();
        if (leafFlight == null)
            return;

        hasTriggered = true;
        StartCoroutine(CompleteLeafSequence());
    }

    private System.Collections.IEnumerator CompleteLeafSequence()
    {
        // 停止乌鸦生成
        if (crowLaneSpawner != null)
            crowLaneSpawner.StopSpawning();

        // 结束飞行
        if (leafFlightController != null)
            leafFlightController.EndFlight();

        yield return new WaitForSeconds(delayBeforeLoad);

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("LeafFlightGoalTrigger2D: nextSceneName is empty");
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
