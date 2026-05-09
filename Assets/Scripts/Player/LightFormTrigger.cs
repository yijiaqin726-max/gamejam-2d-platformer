using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class LightFormTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool onlyOnce = true;

    private bool hasTriggered;

    private void Reset()
    {
        // 方便新建触发器时自动设为 Trigger。
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (onlyOnce && hasTriggered)
            return;

        LightFormController lightFormController = null;

        // 优先用 Player Tag 判断；如果项目没配 tag 或 tag 不匹配，再 fallback 查组件。
        if (HasTag(other, playerTag))
            lightFormController = other.GetComponentInParent<LightFormController>();

        if (lightFormController == null)
            lightFormController = other.GetComponentInParent<LightFormController>();

        if (lightFormController == null)
            return;

        hasTriggered = true;
        lightFormController.EnterLightForm();
    }

    private static bool HasTag(Collider2D other, string tagName)
    {
        if (string.IsNullOrEmpty(tagName))
            return false;

        try
        {
            return other.CompareTag(tagName);
        }
        catch (UnityException)
        {
            return false;
        }
    }
}
