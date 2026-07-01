using UnityEngine;

/// <summary>
/// 撃破時にアイテムを落とす。単一責任=「Health.OnDied通知でドロップを1つ生成する」。
/// </summary>
[RequireComponent(typeof(Health))]
public class ItemDropper : MonoBehaviour
{
    [SerializeField] private GameObject dropPrefab;

    [SerializeField] private Vector2 dropOffset = Vector2.zero;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (health != null)
        {
            health.OnDied += HandleDied;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDied -= HandleDied;
        }
    }

    private void HandleDied()
    {
        if (dropPrefab == null)
        {
            return;
        }

        Vector2 pos = (Vector2)transform.position + dropOffset;
        Instantiate(dropPrefab, pos, Quaternion.identity);
    }
}
