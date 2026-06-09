using UnityEngine;

// 敵を左にまっすぐ動かすスクリプト
public class EnemyMovement : MonoBehaviour
{
    // 移動の速さ（Inspectorで変えられる）
    public float moveSpeed = 2f;

    // ゲーム中、毎フレーム呼ばれる関数
    void Update()
    {
        // 毎フレーム、左へ動かす（-X方向）
        transform.Translate(-moveSpeed * Time.deltaTime, 0f, 0f);
    }
}