using UnityEngine;

/// <summary>
/// ぶっ飛ばしを受けて適用する。単一責任=「与えられた力で自分を弾く」。
/// 敵に付ける。Dynamicな Rigidbody2D が必要。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Knockbackable : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>力(向き×強さ)を受けて弾かれる。</summary>
    public void ApplyKnockback(Vector2 force)
    {
        // 既存の速度を消してから与えると挙動が安定する
        // ※ Unity 2022以前は linearVelocity → velocity に置き換える
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}