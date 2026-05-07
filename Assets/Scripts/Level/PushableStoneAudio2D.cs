using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PushableStoneAudio2D : MonoBehaviour
{
    [SerializeField] private AudioClip stoneImpactSfx;
    [SerializeField] private AudioClip stonePushLoopSfx;
    [SerializeField] private float impactVolume = 0.85f;
    [SerializeField] private float pushVolume = 0.35f;
    [SerializeField] private float minImpactVelocity = 2.5f;
    [SerializeField] private float impactCooldown = 0.4f;
    [SerializeField] private float minPushSpeed = 0.25f;

    private Rigidbody2D body;
    private AudioSource audioSource;
    private float lastImpactTime;
    private bool isTouchingGround;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        lastImpactTime = -impactCooldown;
        isTouchingGround = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.isTrigger)
        {
            return;
        }

        float impactMagnitude = collision.relativeVelocity.magnitude;
        if (impactMagnitude >= minImpactVelocity && Time.time - lastImpactTime >= impactCooldown)
        {
            if (audioSource != null && stoneImpactSfx != null)
            {
                audioSource.PlayOneShot(stoneImpactSfx, impactVolume);
            }
            lastImpactTime = Time.time;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.isTrigger)
        {
            isTouchingGround = true;
        }
    }

    private void Update()
    {
        if (isTouchingGround && Mathf.Abs(body.linearVelocity.x) > minPushSpeed)
        {
            if (audioSource != null && stonePushLoopSfx != null && !audioSource.isPlaying)
            {
                audioSource.clip = stonePushLoopSfx;
                audioSource.loop = true;
                audioSource.volume = pushVolume;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == stonePushLoopSfx)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
        }

        isTouchingGround = false;
    }
}
