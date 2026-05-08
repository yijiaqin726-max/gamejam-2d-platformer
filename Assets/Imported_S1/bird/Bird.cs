using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // 向左飞
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        // 飞出很远才消失，绝对不会刚生成就消失
        if (transform.position.x < 800f)
        {
            Destroy(gameObject);
        }
    }
}