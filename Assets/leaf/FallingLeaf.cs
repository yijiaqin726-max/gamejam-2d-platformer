using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingLeaf : MonoBehaviour
{
    [Header("参数")]
    public float fallSpeed = 2.5f;
    public float groundY = -16f;
    public float triggerDistance = 4f;
    public float spawnHeight = 8f;

    private bool startFall = false;
    private bool landDone = false;
    private GameObject player;

    void Start()
    {
        // 一开始先把叶子隐藏，不动
        GetComponent<SpriteRenderer>().enabled = false;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (landDone) return;

        // 玩家靠近 → 在玩家头顶生成叶子并开始下落
        if (!startFall && player != null)
        {
            float dis = Vector2.Distance(transform.position, player.transform.position);
            if (dis < triggerDistance)
            {
                // 把叶子挪到玩家头顶上方
                transform.position = new Vector3(player.transform.position.x, 
                                                player.transform.position.y + spawnHeight, 
                                                0);
                // 显示叶子
                GetComponent<SpriteRenderer>().enabled = true;

                startFall = true;
            }
        }

        // 开始下落
        if (startFall && transform.position.y > groundY)
        {
            transform.Translate(0, -fallSpeed * Time.deltaTime, 0);
        }

        // 落地停止
        if (startFall && transform.position.y <= groundY)
        {
            landDone = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (landDone && other.CompareTag("Player"))
        {
            PrototypePlayerController pc = other.GetComponent<PrototypePlayerController>();
            if (pc != null)
            {
                pc.TurnToLeafAndFly();
            }
            Destroy(gameObject);
        }
    }
}