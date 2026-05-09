using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider2D))]
public sealed class EndingSequenceTrigger : MonoBehaviour
{
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private CanvasGroup endingCanvasGroup;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private MonoBehaviour[] controllersToDisable;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float quitDelay = 20f;
    [SerializeField] private bool onlyOnce = true;

    private bool hasTriggered;

    private void Awake()
    {
        // 结束语面板默认隐藏，触发后再显示并淡入。
        if (endingPanel != null)
        {
            PrepareCanvasGroup();
            endingPanel.SetActive(false);
        }

        BoxCollider2D triggerCollider = GetComponent<BoxCollider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (onlyOnce && hasTriggered)
        {
            return;
        }

        if (!IsPlayerOrLightOrb(other))
        {
            return;
        }

        hasTriggered = true;
        StartCoroutine(PlayEndingSequence(other));
    }

    private IEnumerator PlayEndingSequence(Collider2D other)
    {
        Debug.Log("Ending sequence triggered");

        DisableConfiguredControllers();
        StopPlayerRigidbody(other);

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
            yield return FadeInEndingPanel();
        }
        else
        {
            Debug.LogWarning("EndingPanel is not assigned on EndingSequenceTrigger.");
        }

        // 不暂停 Time.timeScale，保证 20 秒计时继续走。
        yield return new WaitForSeconds(Mathf.Max(0f, quitDelay));

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PrepareCanvasGroup()
    {
        if (endingPanel == null)
        {
            return;
        }

        if (endingCanvasGroup == null)
        {
            endingCanvasGroup = endingPanel.GetComponent<CanvasGroup>();
        }

        if (endingCanvasGroup == null)
        {
            endingCanvasGroup = endingPanel.AddComponent<CanvasGroup>();
        }

        endingCanvasGroup.alpha = 0f;
        endingCanvasGroup.blocksRaycasts = true;
        endingCanvasGroup.interactable = true;
    }

    private IEnumerator FadeInEndingPanel()
    {
        PrepareCanvasGroup();
        if (endingCanvasGroup == null)
        {
            yield break;
        }

        endingCanvasGroup.alpha = 0f;
        endingCanvasGroup.blocksRaycasts = true;
        endingCanvasGroup.interactable = true;

        float duration = Mathf.Max(0.01f, fadeInDuration);
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            endingCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        endingCanvasGroup.alpha = 1f;
    }

    private bool IsPlayerOrLightOrb(Collider2D other)
    {
        if (other == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
        {
            return true;
        }

        Transform otherTransform = other.transform;
        if (!string.IsNullOrEmpty(playerTag) && otherTransform.root != null && otherTransform.root.CompareTag(playerTag))
        {
            return true;
        }

        // Tag 没设对时兜底：玩家或光点对象通常会带 Rigidbody2D 或控制脚本。
        return other.GetComponentInParent<Rigidbody2D>() != null
            || other.GetComponentInParent<PrototypePlayerController>() != null
            || other.GetComponentInParent<LightOrbFreeMoveController>() != null;
    }

    private void DisableConfiguredControllers()
    {
        if (controllersToDisable == null)
        {
            return;
        }

        for (int i = 0; i < controllersToDisable.Length; i++)
        {
            MonoBehaviour controller = controllersToDisable[i];
            if (controller != null)
            {
                controller.enabled = false;
            }
        }
    }

    private void StopPlayerRigidbody(Collider2D other)
    {
        Rigidbody2D targetRigidbody = playerRigidbody;
        if (targetRigidbody == null && other != null)
        {
            targetRigidbody = other.GetComponentInParent<Rigidbody2D>();
        }

        if (targetRigidbody == null)
        {
            return;
        }

        targetRigidbody.linearVelocity = Vector2.zero;
        targetRigidbody.angularVelocity = 0f;
    }
}
