using UnityEngine;

public class PushableStone2D : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = 4f;
        rb.gravityScale = 2.4f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearDamping = 0.3f;
        rb.angularDamping = 0.3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }
}
