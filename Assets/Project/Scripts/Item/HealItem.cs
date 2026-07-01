using UnityEngine;

/// <summary>
/// 回復アイテム。単一責任=「プレイヤーが触れたらHPを回復して自分を消す」。
/// トリガーCollider2Dで当たりを取る。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HealItem : MonoBehaviour
{
    [SerializeField] private int healAmount = 30;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        var health = other.GetComponentInParent<Health>();

        if (health == null)
        {
            return;
        }

        health.Heal(healAmount);
        Destroy(gameObject);
    }
}
