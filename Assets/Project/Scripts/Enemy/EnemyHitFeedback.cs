using UnityEngine;

/// <summary>
/// 敵が被弾（HP減少）したとき、WhiteFlash に薄い白を依頼するだけ。
/// 発光の実体は WhiteFlash。スプライト・マテリアルには直接触らない。
/// 単一責任 = 「被弾検知 → WhiteFlash への委譲のみ」
/// </summary>
public class EnemyHitFeedback : MonoBehaviour
{
    [Tooltip("HP変化の通知元。未設定なら自身から自動取得")]
    [SerializeField] private Health health;

    [Tooltip("発光の実体。未設定なら自身から自動取得")]
    [SerializeField] private WhiteFlash whiteFlash;

    [Tooltip("被弾時の白の薄さ（0=変化なし, 1=完全に白）")]
    [Range(0f, 1f)]
    [SerializeField] private float hitFlashAmount = 0.25f;

    private int _previousHp;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogWarning($"[EnemyHitFeedback] {name} : Health が見つかりません。", this);
            return;
        }

        if (whiteFlash == null) whiteFlash = GetComponent<WhiteFlash>();
        if (whiteFlash == null)
        {
            Debug.LogWarning($"[EnemyHitFeedback] {name} : WhiteFlash が見つかりません。", this);
            return;
        }

        _previousHp = health.CurrentHp;
    }

    private void OnEnable()
    {
        if (health != null) health.OnHpChanged += HandleHpChanged;
    }

    private void OnDisable()
    {
        if (health != null) health.OnHpChanged -= HandleHpChanged;
    }

    private void HandleHpChanged(int currentHp, int maxHp)
    {
        // 回復（currentHp >= _previousHp）では発光しない
        if (currentHp < _previousHp && !whiteFlash.IsFlashing)
        {
            whiteFlash.Flash(hitFlashAmount);
        }
        _previousHp = currentHp;
    }

    private void OnDestroy()
    {
        if (health != null) health.OnHpChanged -= HandleHpChanged;
    }
}