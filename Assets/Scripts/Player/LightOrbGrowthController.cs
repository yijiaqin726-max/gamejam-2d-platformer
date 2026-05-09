using System.Collections;
using UnityEngine;

public sealed class LightOrbGrowthController : MonoBehaviour
{
    [SerializeField] private Transform lightOrbVisual;
    [SerializeField] private float scaleIncreasePerOrb = 0.08f;
    [SerializeField] private float maxScaleMultiplier = 1.8f;
    [SerializeField] private float scalePopDuration = 0.15f;

    private Vector3 initialScale = Vector3.one;
    private float currentScaleMultiplier = 1f;
    private Coroutine scaleRoutine;

    private void Awake()
    {
        // 如果 Inspector 没拖，尝试按当前约定名自动找到 LightOrbVisual。
        if (lightOrbVisual == null)
        {
            Transform found = transform.Find("LightOrbVisual");
            if (found != null)
                lightOrbVisual = found;
        }

        if (lightOrbVisual != null)
            initialScale = lightOrbVisual.localScale;
    }

    public void GrowOnce()
    {
        if (lightOrbVisual == null)
        {
            Debug.LogWarning("[LightOrbGrowth] LightOrbVisual is missing");
            return;
        }

        currentScaleMultiplier = Mathf.Min(
            currentScaleMultiplier + Mathf.Max(0f, scaleIncreasePerOrb),
            Mathf.Max(1f, maxScaleMultiplier));

        Vector3 targetScale = initialScale * currentScaleMultiplier;

        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScalePopRoutine(targetScale));
    }

    private IEnumerator ScalePopRoutine(Vector3 targetScale)
    {
        float duration = Mathf.Max(0.01f, scalePopDuration);
        float halfDuration = duration * 0.5f;
        Vector3 startScale = lightOrbVisual.localScale;
        Vector3 popScale = targetScale * 1.08f;

        // 先快速鼓一下，再回到目标大小，只改视觉 localScale，不碰玩家根物体/Collider/Rigidbody。
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            lightOrbVisual.localScale = Vector3.Lerp(startScale, popScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            lightOrbVisual.localScale = Vector3.Lerp(popScale, targetScale, t);
            yield return null;
        }

        lightOrbVisual.localScale = targetScale;
        scaleRoutine = null;
    }
}
