using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 16f;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 移动
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // 跑步动画
        anim.SetFloat("speed", Mathf.Abs(moveInput));

        // 跳跃
        if (Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.Play("jump");
        }

        // 落地切回待机
        if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            if (Mathf.Abs(moveInput) > 0.1f)
                anim.Play("run");
            else
                anim.Play("idle");
        }

        // 翻转
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }
}