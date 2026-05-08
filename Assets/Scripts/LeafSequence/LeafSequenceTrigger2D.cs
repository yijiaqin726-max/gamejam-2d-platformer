using UnityEngine;

/// <summary>
/// 触发落叶变身飞行段的入口
/// 挂在木桩 Trigger 上，检测玩家进入后启动整个流程
/// </summary>
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

        hasTriggered = true;
        StartCoroutine(PlayLeafSequence());
    }

    private System.Collections.IEnumerator PlayLeafSequence()
    {
        // 禁用玩家控制
        if (playerController != null)
            playerController.enabled = false;

        // 树叶飘落
        if (guideLeaf != null)
            yield return StartCoroutine(guideLeaf.PlayFall());

        // 延迟后进入叶子形态
        yield return new WaitForSeconds(delayBeforeLeafMode);

        // 隐藏玩家视觉
        if (playerVisualRoot != null)
            playerVisualRoot.SetActive(false);

        // 启用叶子飞行控制
        if (leafFlightController != null)
            leafFlightController.BeginFlight();

        // 启动乌鸦生成
        if (crowLaneSpawner != null)
            crowLaneSpawner.StartSpawning();
    }
}
