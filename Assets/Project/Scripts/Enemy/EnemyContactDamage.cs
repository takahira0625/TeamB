using UnityEngine;

/// <summary>
/// 敵がプレイヤーに接触した際にダメージを与える。単一責任=「接触1回あたりのダメージ付与」。
/// OnCollisionEnter2D のみ使用。密着継続時のダメージはプレイヤー側の無敵時間が担当する。
/// </summary>
public class EnemyContactDamage : MonoBehaviour
{
    // 接触1回あたりのダメージ量（Inspectorで調整）
    [SerializeField] private int contactDamage = 10;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 撃破後にコンポーネントが無効化されていればダメージを与えない
        if (!enabled) return;

        if (!collision.gameObject.CompareTag("Player")) return;

        var health = collision.gameObject.GetComponent<Health>();
        if (health == null) return;

        health.TakeDamage(contactDamage);
    }
}
