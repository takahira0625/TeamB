using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 剣の当たり判定(トリガー方式)。単一責任=「攻撃中に剣へ触れた敵にヒットを与える」。
/// 剣のトリガーCollider2Dを攻撃中だけ有効化し、触れた敵をぶっ飛ばす。
/// 振りの動きはSwordSwing、ぶっ飛ばしの適用はKnockbackableが担当する。
/// ★Rigidbody2D と同じオブジェクト(SwordPivot)に付けること(触れた通知がここに届く)。
/// </summary>
public class SwordHitbox : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("調整値をまとめたSwordParamsアセット")]
    [SerializeField] private SwordParams param;
    [Tooltip("攻撃中かどうかの判定元。未指定なら自身または親から取得")]
    [SerializeField] private SwordSwing swing;
    [Tooltip("剣のトリガーCollider。未指定なら子から自動取得")]
    [SerializeField] private Collider2D hitCollider;
    [Tooltip("ぶっ飛ばし方向の基準(プレイヤー)。未指定なら自分")]
    [SerializeField] private Transform attackOrigin;
    [Tooltip("剣の先端の目印。握り(このオブジェクトの原点)から先端までで比率を測る")]
    [SerializeField] private Transform bladeTip;

    private float knockbackForce;   // チャージ段階に応じてSwordChargeが設定する実行値
    private int chargeStage;        // チャージ段階。SwordChargeがSetChargeStageで設定する
    private readonly HashSet<Knockbackable> hitThisSwing = new();
    private bool wasAttacking;

    /// <summary>ぶっ飛ばしの強さを設定する(チャージ段階に応じてSwordChargeが渡す)。</summary>
    public void SetKnockback(float force)
    {
        knockbackForce = force;
    }

    /// <summary>チャージ段階を設定する(SwordChargeが振る直前に渡す)。</summary>
    public void SetChargeStage(int stage)
    {
        chargeStage = stage;
    }

    /// <summary>現在のチャージ段階(0〜)。白フラッシュ等の演出コンポーネントが参照する。</summary>
    public int CurrentChargeStage => chargeStage;

    /// <summary>
    /// ヒットの瞬間に1回だけ発火する手応えイベント。引数=(チャージ段階, 先端tipper可否)。
    /// ヒットストップ・カメラ揺れ・段階別SEがこれを購読する(宣言・発火の正本はこのクラス)。
    /// ※白フラッシュは購読せず、SwordHitboxから対象敵を直接呼ぶ別経路。
    /// 購読側はOnDestroyで必ず購読解除(-=)すること。
    /// </summary>
    public event System.Action<int, bool> OnHit;

    private void Awake()
    {
        if (swing == null)
        {
            swing = GetComponentInParent<SwordSwing>();
        }
        if (hitCollider == null)
        {
            hitCollider = GetComponentInChildren<Collider2D>();
        }
        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }

        // 通常時は判定オフ
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }
    }

    private void Update()
    {
        bool attacking = swing != null && swing.IsAttacking;

        // 攻撃中だけ剣の判定を有効化する
        if (hitCollider != null)
        {
            hitCollider.enabled = attacking;
        }

        // 振り始めの瞬間にヒット済みリストをリセット(1振りで同じ敵は1回だけ)
        if (attacking && !wasAttacking)
        {
            hitThisSwing.Clear();
        }
        wasAttacking = attacking;
    }

    // 攻撃中、剣のトリガーに敵が触れたら呼ばれる
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 念のため攻撃中以外は無視
        if (swing == null || !swing.IsAttacking)
        {
            return;
        }

        // 1振りで同じ敵を二重にヒットさせない
        var target = other.GetComponentInParent<Knockbackable>();
        if (target == null || hitThisSwing.Contains(target))
        {
            return;
        }
        hitThisSwing.Add(target);

        // 先端ヒットなら倍率をかける(ノックバックとダメージは独立した倍率)
        bool tipper = IsTipper(other);
        float force = knockbackForce * (tipper ? param.tipperKnockbackMul : 1f);

        // プレイヤーと反対方向(横)へ。必要なら上成分を足す。
        float sign = Mathf.Sign(other.transform.position.x - attackOrigin.position.x);
        if (sign == 0f)
        {
            sign = 1f;
        }
        Vector2 dir = new Vector2(sign, param.upwardBias).normalized;

        target.ApplyKnockback(dir * force);

        // ダメージ付与(チャージ段階 + tipper補正)
        var health = target.GetComponent<Health>();
        if (health != null)
        {
            int baseDamage = param.stages[chargeStage].damage;
            int damage = tipper ? Mathf.RoundToInt(baseDamage * param.tipperDamageMul) : baseDamage;
            health.TakeDamage(damage);
        }

        // 手応えイベントを1回だけ発火する(全ヒット共通)。
        // ヒットストップ・カメラ揺れ・段階別SE がこれを購読する。
        // ※白フラッシュは購読せず、段階×先端を見てSwordHitboxから対象敵を直接呼ぶ別経路。
        OnHit?.Invoke(chargeStage, tipper);

        if (tipper)
        {
            string hpLog = health != null ? $" → {health.gameObject.name} HP: {health.CurrentHp}/{health.MaxHp}" : "";
            Debug.Log($"切先ヒット！(tipper){hpLog}");
        }
    }

    /// <summary>敵が剣のどこで当たったか。握りからの距離÷剣の長さがしきい値以上なら先端。</summary>
    private bool IsTipper(Collider2D enemy)
    {
        // 目印未設定なら常に通常ヒット
        if (bladeTip == null)
        {
            return false;
        }

        Vector2 grip = transform.position;             // SwordPivotの原点=握り
        Vector2 blade = (Vector2)bladeTip.position - grip;
        float length = blade.magnitude;
        if (length < 0.0001f)
        {
            return false;
        }

        // 敵の中心を「握り→剣先」方向へ射影した距離を、剣の長さで割る
        float along = Vector2.Dot((Vector2)enemy.bounds.center - grip, blade.normalized);
        float ratio = along / length;
        return ratio >= param.tipperRatio;
    }
}