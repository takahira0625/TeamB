using UnityEngine;

/// <summary>
/// 敵がプレイヤーに接触した際にダメージを与える。単一責任=「接触1回あたりのダメージ付与」。
/// OnTriggerEnter2D のみ使用（プレイヤー×敵は押し合わないトリガー方式）。
/// 密着継続時のダメージはプレイヤー側の無敵時間が担当する。
/// </summary>
public class EnemyContactDamage : MonoBehaviour
{
    // 接触1回あたりのダメージ量（Inspectorで調整）
    [SerializeField] private int contactDamage = 10;

    // プレイヤーの当たり判定（ハートボックス）がこの敵のトリガーに入った瞬間に呼ばれる
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 撃破後にコンポーネントが無効化されていればダメージを与えない
        // ※物理コールバックは enabled を無視して呼ばれるため、この行で明示的に止める
        if (!enabled) return;

        if (!other.CompareTag("Player")) return;

        var health = other.GetComponentInParent<Health>();
        if (health == null) return;

        health.TakeDamage(contactDamage);
    }
}
