using UnityEngine;

/// <summary>
/// ぶっ飛ばしを受けて適用する。単一責任=「与えられた力で自分を弾き、着地まで前進を止める」。
/// 敵に付ける。Dynamicな Rigidbody2D が必要。
/// 弾かれている間はEnemyMovementを止め、地面に着いたら前進を再開する。
/// 前進(transform移動)が物理の勢いを打ち消して放物線が濁るのを防ぐため。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Knockbackable : MonoBehaviour
{
    // 着地を判定する地面のレイヤー(Ground)。Inspectorで設定する
    [SerializeField] private LayerMask groundLayer;

    // 着地で前進を再開する相手。未設定なら同じオブジェクトから自動取得
    [SerializeField] private EnemyMovement enemyMovement;

    // 撃破済みか判定するためのHP。未設定なら同じオブジェクトから自動取得
    [SerializeField] private Health health;

    private Rigidbody2D rb;

    // 弾かれて空中にいる間はtrue。着地でfalseに戻す
    private bool isKnockedBack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Inspectorで未設定なら同じオブジェクトから探す
        if (enemyMovement == null)
        {
            enemyMovement = GetComponent<EnemyMovement>();
        }

        if (health == null)
        {
            health = GetComponent<Health>();
        }
    }

    /// <summary>力(向き×強さ)を受けて弾かれ、着地するまで前進を止める。</summary>
    public void ApplyKnockback(Vector2 force)
    {
        // 既存の速度を消してから与えると挙動が安定する
        // ※ Unity 2022以前は linearVelocity → velocity に置き換える
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);

        // 弾かれている間は前進を止める(着地まで)
        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }
        isKnockedBack = true;
    }

    // 何かに触れた瞬間に呼ばれる。地面に着いたら前進を再開する
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 弾かれ中でなければ無視(普通に地面を歩いているときなど)
        if (!isKnockedBack)
        {
            return;
        }

        // 触れた相手が地面レイヤーかを調べる
        // (1 << レイヤー番号) でそのレイヤーだけ1のビットを作り、groundLayerと重なれば地面
        int otherLayer = collision.gameObject.layer;
        bool isGround = (groundLayer.value & (1 << otherLayer)) != 0;
        if (!isGround)
        {
            return;
        }

        // 着地した
        isKnockedBack = false;

        // 撃破済み(HP0)なら前進を復活させない。死んだ敵が動き出すのを防ぐ
        if (health != null && health.IsDead)
        {
            return;
        }

        if (enemyMovement != null)
        {
            enemyMovement.enabled = true;
        }
    }
}
