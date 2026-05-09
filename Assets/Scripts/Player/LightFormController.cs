using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class LightFormController : MonoBehaviour
{
    [Header("Auto Enter")]
    [SerializeField] private bool enterOnStart = true;
    [SerializeField] private float enterOnStartDelay = 0.3f;
    [SerializeField] private bool enableDebugHotkey = true;

    [Header("Light Form")]
    [SerializeField] private GameObject lightOrbVisual;
    [SerializeField] private LightOrbFreeMoveController lightOrbMoveController;
    [SerializeField] private CircleCollider2D lightOrbCollider;
    [SerializeField] private float transformDelay = 0.5f;
    [SerializeField] private string transformTriggerName = "TransformToLight";

    [Header("Scripts To Disable")]
    [SerializeField] public MonoBehaviour[] movementScriptsToDisable;

    private Rigidbody2D cachedRigidbody;
    private SpriteRenderer[] cachedSpriteRenderers;
    private Animator cachedAnimator;
    private bool[] originalSpriteRendererEnabled;
    private bool originalLightOrbColliderEnabled;
    private bool originalLightOrbColliderIsTrigger;
    private Vector2 originalLightOrbColliderOffset;
    private bool isLightForm;
    private bool isTransforming;

    private void Awake()
    {
        // 本脚本负责切换形态；自由移动交给 LightOrbFreeMoveController。
        cachedRigidbody = GetComponent<Rigidbody2D>();
        cachedSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        cachedAnimator = GetComponent<Animator>();

        if (lightOrbMoveController == null)
            lightOrbMoveController = GetComponent<LightOrbFreeMoveController>();

        if (lightOrbCollider != null)
        {
            originalLightOrbColliderEnabled = lightOrbCollider.enabled;
            originalLightOrbColliderIsTrigger = lightOrbCollider.isTrigger;
            originalLightOrbColliderOffset = lightOrbCollider.offset;
        }

        CacheOriginalSpriteRendererState();

        if (lightOrbVisual != null)
            lightOrbVisual.SetActive(false);

        if (lightOrbMoveController != null)
            lightOrbMoveController.enabled = false;
    }

    private void Start()
    {
        if (enterOnStart)
            StartCoroutine(EnterOnStartRoutine());
    }

    private void Update()
    {
        if (!enableDebugHotkey)
            return;

        if (WasDebugHotkeyPressed())
            EnterLightForm();
    }

    public void EnterLightForm()
    {
        Debug.Log("[LightForm] EnterLightForm called");

        if (isLightForm || isTransforming)
            return;

        StartCoroutine(EnterLightFormRoutine());
    }

    public void ExitLightForm()
    {
        if (!isLightForm && !isTransforming)
            return;

        StopAllCoroutines();
        isTransforming = false;
        isLightForm = false;

        if (lightOrbMoveController != null)
            lightOrbMoveController.DisableLightOrbControl();

        RestoreSpriteRendererState();
        RestoreLightOrbColliderState();

        if (lightOrbVisual != null)
            lightOrbVisual.SetActive(false);

        SetMovementScriptsEnabled(true);
    }

    private IEnumerator EnterOnStartRoutine()
    {
        Debug.Log("[LightForm] Start auto enter");

        if (enterOnStartDelay > 0f)
            yield return new WaitForSeconds(enterOnStartDelay);

        EnterLightForm();
    }

    private IEnumerator EnterLightFormRoutine()
    {
        isTransforming = true;

        TrySetAnimatorTrigger(transformTriggerName);

        if (transformDelay > 0f)
            yield return new WaitForSeconds(transformDelay);

        DisableMovementScripts();
        ApplyLightFormRigidbodyState();
        ApplyLightOrbColliderState();
        SetNormalSpriteRenderersEnabled(false);
        ShowLightOrbVisual();

        if (lightOrbMoveController != null)
        {
            lightOrbMoveController.enabled = true;
            lightOrbMoveController.EnableLightOrbControl();
            Debug.Log("[LightForm] LightOrbMoveController enabled: " + lightOrbMoveController.enabled);
        }
        else
        {
            Debug.LogWarning("[LightForm] LightOrbMoveController is missing");
        }

        isLightForm = true;
        isTransforming = false;
    }

    private void DisableMovementScripts()
    {
        SetMovementScriptsEnabled(false);

        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || behaviour == this || behaviour == lightOrbMoveController)
                continue;

            string typeName = behaviour.GetType().Name;
            if (typeName == "PrototypePlayerController"
                || typeName == "PrototypePlayerControl"
                || typeName == "PlayerController"
                || typeName == "PlayerMovement")
            {
                behaviour.enabled = false;
                Debug.Log("[LightForm] Disabled movement script: " + typeName);
            }
        }
    }

    private void SetMovementScriptsEnabled(bool enabled)
    {
        if (movementScriptsToDisable == null)
            return;

        for (int i = 0; i < movementScriptsToDisable.Length; i++)
        {
            MonoBehaviour script = movementScriptsToDisable[i];
            if (script == null || script == this || script == lightOrbMoveController)
                continue;

            script.enabled = enabled;
            if (!enabled)
                Debug.Log("[LightForm] Disabled movement script: " + script.GetType().Name);
        }
    }

    private void ApplyLightFormRigidbodyState()
    {
        if (cachedRigidbody == null)
            return;

        cachedRigidbody.gravityScale = 0f;
        cachedRigidbody.linearVelocity = Vector2.zero;
        cachedRigidbody.angularVelocity = 0f;
        cachedRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        Debug.Log("[LightForm] Rigidbody gravity=" + cachedRigidbody.gravityScale + ", constraints=" + cachedRigidbody.constraints);
    }

    private void ApplyLightOrbColliderState()
    {
        if (lightOrbCollider == null)
            return;

        lightOrbCollider.enabled = true;
        lightOrbCollider.isTrigger = false;
        lightOrbCollider.offset = Vector2.zero;
    }

    private void RestoreLightOrbColliderState()
    {
        if (lightOrbCollider == null)
            return;

        lightOrbCollider.enabled = originalLightOrbColliderEnabled;
        lightOrbCollider.isTrigger = originalLightOrbColliderIsTrigger;
        lightOrbCollider.offset = originalLightOrbColliderOffset;
    }

    private void ShowLightOrbVisual()
    {
        if (lightOrbVisual == null)
        {
            Debug.LogWarning("[LightForm] LightOrbVisual is missing");
            return;
        }

        lightOrbVisual.SetActive(true);

        SpriteRenderer[] orbRenderers = lightOrbVisual.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < orbRenderers.Length; i++)
        {
            if (orbRenderers[i] != null)
                orbRenderers[i].enabled = true;
        }

        Debug.Log("[LightForm] LightOrbVisual activeSelf=" + lightOrbVisual.activeSelf + ", activeInHierarchy=" + lightOrbVisual.activeInHierarchy);
    }

    private void SetNormalSpriteRenderersEnabled(bool enabled)
    {
        if (cachedSpriteRenderers == null)
            return;

        for (int i = 0; i < cachedSpriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = cachedSpriteRenderers[i];
            if (spriteRenderer == null || IsLightOrbRenderer(spriteRenderer))
                continue;

            spriteRenderer.enabled = enabled;
        }
    }

    private bool IsLightOrbRenderer(SpriteRenderer spriteRenderer)
    {
        return lightOrbVisual != null && spriteRenderer.transform.IsChildOf(lightOrbVisual.transform);
    }

    private void TrySetAnimatorTrigger(string triggerName)
    {
        if (cachedAnimator == null || string.IsNullOrEmpty(triggerName))
            return;

        AnimatorControllerParameter[] parameters = cachedAnimator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == AnimatorControllerParameterType.Trigger && parameters[i].name == triggerName)
            {
                cachedAnimator.SetTrigger(triggerName);
                return;
            }
        }
    }

    private bool WasDebugHotkeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
            return keyboard.tKey.wasPressedThisFrame;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.T);
#else
        return false;
#endif
    }

    private void CacheOriginalSpriteRendererState()
    {
        if (cachedSpriteRenderers == null)
            return;

        originalSpriteRendererEnabled = new bool[cachedSpriteRenderers.Length];
        for (int i = 0; i < cachedSpriteRenderers.Length; i++)
        {
            if (cachedSpriteRenderers[i] != null)
                originalSpriteRendererEnabled[i] = cachedSpriteRenderers[i].enabled;
        }
    }

    private void RestoreSpriteRendererState()
    {
        if (cachedSpriteRenderers == null || originalSpriteRendererEnabled == null)
            return;

        for (int i = 0; i < cachedSpriteRenderers.Length; i++)
        {
            if (cachedSpriteRenderers[i] != null)
                cachedSpriteRenderers[i].enabled = originalSpriteRendererEnabled[i];
        }
    }
}
