using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingLeaf : MonoBehaviour
{
    [Header("参数")]
    public float fallSpeed = 2.5f;
    public float groundY = 0f;         // 改成你地图地面的Y坐标
    public float triggerDistance = 6f;  // 放大触发距离，更容易触发
    public float spawnHeight = 8f;

    private bool startFall = false;
    private bool landDone = false;
    private GameObject player;

    void Start()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        player = GameObject.FindGameObjectWithTag("Player");
        landDone = false;
        startFall = false;
        Debug.Log("叶子脚本初始化完成");
    }

    void Update()
    {
        if (landDone) return;

        if (!startFall && player != null)
        {
            float dis = Vector2.Distance(transform.position, player.transform.position);
            if (dis < triggerDistance)
            {
                // 瞬移到玩家头顶
                transform.position = new Vector3(
                    player.transform.position.x, 
                    player.transform.position.y + spawnHeight, 
                    0
                );
                GetComponent<SpriteRenderer>().enabled = true;
                startFall = true;
                Debug.Log("✅ 触发成功！叶子已瞬移到头顶");
            }
        }

        // 开始下落（用直接赋值位置的方式，不会被任何东西挡住）
        if (startFall && transform.position.y > groundY)
        {
            transform.position = new Vector3(
                transform.position.x, 
                transform.position.y - fallSpeed * Time.deltaTime, 
                0
            );
        }

        // 落地
        if (startFall && transform.position.y <= groundY)
        {
            landDone = true;
            Debug.Log("✅ 叶子落地了！");
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
                Debug.Log("✅ 玩家触发变身！");
            }
            Destroy(gameObject);
        }
    }
}