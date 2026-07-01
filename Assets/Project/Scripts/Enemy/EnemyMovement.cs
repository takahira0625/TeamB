using UnityEngine;

// 敵を左にまっすぐ動かすスクリプト。
// 一度カメラに映って「起動」したら、以降は常に前進する
// （画面外に吹っ飛ばされても着地後ちゃんと戻ってくるように）。
public class EnemyMovement : MonoBehaviour
{
    // 調整値をまとめたEnemyParamsアセット（移動速度）
    [SerializeField] private EnemyParams param;

    // 画面内判定に使うカメラ。未指定ならメインカメラを使う
    [SerializeField] private Camera targetCamera;

    // 一度でもカメラに映ったらtrue。以降は画面外でも動く
    private bool activated;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    // ゲーム中、毎フレーム呼ばれる関数
    void Update()
    {
        if (param == null)
        {
            return;
        }

        // まだ起動していない敵は、カメラに映るまで待機する
        // （手前の敵がプレイヤーの見える前から歩いてこないように）
        if (!activated)
        {
            if (!IsInView())
            {
                return;
            }
            activated = true;   // 画面に入ったら起動。以降は画面外でも動き続ける
        }

        // 毎フレーム、左へ動かす（-X方向）
        transform.Translate(-param.moveSpeed * Time.deltaTime, 0f, 0f);
    }

    // 敵がカメラの横方向の表示範囲にいるか（横スクロール前提なのでXだけ見る）
    private bool IsInView()
    {
        // カメラが取れないときは起動扱い（保険）
        if (targetCamera == null)
        {
            return true;
        }

        // ビューポート座標(0〜1が画面内)のXで判定する
        Vector3 viewportPos = targetCamera.WorldToViewportPoint(transform.position);
        return viewportPos.x >= 0f && viewportPos.x <= 1f;
    }
}
