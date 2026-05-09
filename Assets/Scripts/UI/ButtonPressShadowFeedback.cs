using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public sealed class ButtonPressShadowFeedback : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private Color normalShadowColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color hoverShadowColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private Color pressedShadowColor = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private Vector2 normalShadowDistance = new Vector2(2f, -2f);
    [SerializeField] private Vector2 hoverShadowDistance = new Vector2(3f, -3f);
    [SerializeField] private Vector2 pressedShadowDistance = new Vector2(5f, -5f);
    [SerializeField] private Vector2 pressedOffset = new Vector2(0f, -2f);
    [SerializeField] private float hoverScale = 1.04f;
    [SerializeField] private float pressedScale = 0.98f;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float hoverVolume = 0.6f;
    [SerializeField] private float clickVolume = 0.8f;

    private RectTransform rectTransform;
    private Shadow shadow;
    private AudioSource audioSource;
    private Vector3 normalLocalPosition;
    private Vector3 normalLocalScale;
    private bool initialized;
    private bool isHovering;
    private bool isPressed;

    private void Awake()
    {
        Initialize();
        ApplyNormalState();
    }

    private void OnEnable()
    {
        Initialize();
        ApplyNormalState();
    }

    private void OnDisable()
    {
        if (initialized)
        {
            ApplyNormalState();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Initialize();
        isPressed = true;
        PlaySound(clickSound, clickVolume);
        ApplyCurrentState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        ApplyCurrentState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Initialize();
        isHovering = true;
        PlaySound(hoverSound, hoverVolume);
        ApplyCurrentState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        isPressed = false;
        ApplyCurrentState();
    }

    private void Initialize()
    {
        if (initialized)
        {
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        shadow = GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = gameObject.AddComponent<Shadow>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        normalLocalPosition = rectTransform.localPosition;
        normalLocalScale = rectTransform.localScale;
        initialized = true;
    }

    private void ApplyNormalState()
    {
        if (!initialized)
        {
            return;
        }

        shadow.effectColor = normalShadowColor;
        shadow.effectDistance = normalShadowDistance;
        rectTransform.localPosition = normalLocalPosition;
        rectTransform.localScale = normalLocalScale;
    }

    private void ApplyHoverState()
    {
        if (!initialized)
        {
            return;
        }

        shadow.effectColor = hoverShadowColor;
        shadow.effectDistance = hoverShadowDistance;
        rectTransform.localPosition = normalLocalPosition;
        rectTransform.localScale = normalLocalScale * Mathf.Max(0.01f, hoverScale);
    }

    private void ApplyPressedState()
    {
        if (!initialized)
        {
            return;
        }

        shadow.effectColor = pressedShadowColor;
        shadow.effectDistance = pressedShadowDistance;
        rectTransform.localPosition = normalLocalPosition + new Vector3(pressedOffset.x, pressedOffset.y, 0f);
        rectTransform.localScale = normalLocalScale * Mathf.Max(0.01f, pressedScale);
    }

    private void ApplyCurrentState()
    {
        if (isPressed)
        {
            ApplyPressedState();
            return;
        }

        if (isHovering)
        {
            ApplyHoverState();
            return;
        }

        ApplyNormalState();
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        // UI 音效只播放一次，不影响 Button 自己的 OnClick 逻辑。
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
