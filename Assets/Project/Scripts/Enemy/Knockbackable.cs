using System.Collections;
using UnityEngine;

/// <summary>
/// ぶっ飛ばしを受けて適用する。単一責任=「与えられた力で自分を弾く」。
/// 敵に付ける。Dynamicな Rigidbody2D が必要。
/// 弾かれている間は一定時間（スタン）EnemyMovementを止め、
/// 前進が物理の勢いを打ち消して「弾かれ感」が濁るのを防ぐ。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Knockbackable : MonoBehaviour
{
    // 調整値をまとめたEnemyParamsアセット（スタン時間）
    [SerializeField] private EnemyParams param;

    // スタン中に前進を止める相手。未設定なら同じオブジェクトから自動取得
    [SerializeField] private EnemyMovement enemyMovement;

    // 撃破済みか判定するためのHP。未設定なら同じオブジェクトから自動取得
    [SerializeField] private Health health;

    private Rigidbody2D rb;

    // 進行中のスタンコルーチン。連続ヒット時にタイマーをリセットするため保持する
    private Coroutine stunCoroutine;

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

    /// <summary>力(向き×強さ)を受けて弾かれ、スタンを開始する。</summary>
    public void ApplyKnockback(Vector2 force)
    {
        // 既存の速度を消してから与えると挙動が安定する
        // ※ Unity 2022以前は linearVelocity → velocity に置き換える
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);

        // 弾かれている間だけ前進を止める
        StartStun();
    }

    /// <summary>前進を止め、stunDuration後に再開するスタンを開始する。</summary>
    private void StartStun()
    {
        if (enemyMovement == null || param == null) return;

        // 連続ヒット：進行中のスタンを止めてからタイマーを引き直す
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        enemyMovement.enabled = false;
        stunCoroutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        yield return new WaitForSeconds(param.stunDuration);

        // 撃破済み(HP0)なら前進を復活させない。死んだ敵が動き出すのを防ぐ
        if (health != null && health.IsDead)
        {
            stunCoroutine = null;
            yield break;
        }

        enemyMovement.enabled = true;
        stunCoroutine = null;
    }
}
