using UnityEngine;

// 敵を左にまっすぐ動かすスクリプト
public class EnemyMovement : MonoBehaviour
{
    // 調整値をまとめたEnemyParamsアセット（移動速度）
    [SerializeField] private EnemyParams param;

    // ゲーム中、毎フレーム呼ばれる関数
    void Update()
    {
        if (param == null)
        {
            return;
        }

        // 毎フレーム、左へ動かす（-X方向）
        transform.Translate(-param.moveSpeed * Time.deltaTime, 0f, 0f);
    }
}