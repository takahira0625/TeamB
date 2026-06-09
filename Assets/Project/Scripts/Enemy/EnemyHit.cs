using UnityEngine;

// プレイヤーに当たったら、この敵を消すスクリプト
public class EnemyHit : MonoBehaviour
{
    // 他のオブジェクトとぶつかった瞬間に呼ばれる関数
    void OnCollisionEnter2D(Collision2D collision)
    {
        // ぶつかった相手のタグが "Player" だったら
        if (collision.gameObject.CompareTag("Player"))
        {
            // この敵（自分自身）を消す
            Destroy(this.gameObject);
        }
    }
}