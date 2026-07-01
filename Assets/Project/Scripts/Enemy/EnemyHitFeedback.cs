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

    [Tooltip("被弾時のフラッシュの強さ（0=変化なし, 1=完全にhitFlashColor）")]
    [Range(0f, 1f)]
    [SerializeField] private float hitFlashAmount = 0.25f;

    [Tooltip("通常被弾の色。先端クリティカルの白と見分けるため暗めにする")]
    [SerializeField] private Color hitFlashColor = new Color(0.15f, 0.15f, 0.15f, 1f);

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
    }

    private void Start()
    {
        // 直前HPの初期化は Start() で行う。Awake だと Health.Awake(CurrentHp=maxHp) との
        // 実行順が保証されず、EnemyHitFeedback.Awake が先に走ると _previousHp が 0 のままになり、
        // 満タンからの初撃が発光しないため。全 Awake 完了後の Start なら CurrentHp が確定している。
        if (health != null) _previousHp = health.CurrentHp;
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
            whiteFlash.Flash(hitFlashAmount, hitFlashColor);
        }
        _previousHp = currentHp;
    }

    private void OnDestroy()
    {
        if (health != null) health.OnHpChanged -= HandleHpChanged;
    }
}