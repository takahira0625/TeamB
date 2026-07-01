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

    private float knockbackForce;
    private int chargeStage;
    private readonly HashSet<Knockbackable> hitThisSwing = new();
    private bool wasAttacking;

    public void SetKnockback(float force)
    {
        knockbackForce = force;
    }

    public void SetChargeStage(int stage)
    {
        chargeStage = stage;
    }

    public int CurrentChargeStage => chargeStage;

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

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }
    }

    private void Update()
    {
        bool attacking = swing != null && swing.IsAttacking;

        if (hitCollider != null)
        {
            hitCollider.enabled = attacking;
        }

        if (attacking && !wasAttacking)
        {
            hitThisSwing.Clear();
        }

        wasAttacking = attacking;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (swing == null || !swing.IsAttacking)
        {
            return;
        }

        var target = other.GetComponentInParent<Knockbackable>();

        if (target == null || hitThisSwing.Contains(target))
        {
            return;
        }

        hitThisSwing.Add(target);

        bool tipper = IsTipper(other);

        // クリティカル＝最大チャージ段階 × 先端ヒット
        // メタル装甲判定と白フラッシュで共用する
        bool isCrit = tipper && chargeStage == param.stages.Length - 1;

        float force = knockbackForce * (tipper ? param.tipperKnockbackMul : 1f);

        float sign = Mathf.Sign(other.transform.position.x - attackOrigin.position.x);
        if (sign == 0f)
        {
            sign = 1f;
        }

        Vector2 dir = new Vector2(sign, param.upwardBias).normalized;

        // メタル装甲でもノックバックは入る
        target.ApplyKnockback(dir * force);

        // メタル装甲：クリティカル以外はダメージを弾く
        var armor = target.GetComponent<MetalArmor>();
        bool damageBlocked = armor != null && armor.ShouldBlock(isCrit);

        var health = target.GetComponent<Health>();

        if (health != null && !damageBlocked)
        {
            int baseDamage = param.stages[chargeStage].damage;
            int damage = tipper ? Mathf.RoundToInt(baseDamage * param.tipperDamageMul) : baseDamage;
            health.TakeDamage(damage);
        }

        OnHit?.Invoke(chargeStage, tipper);

        if (isCrit)
        {
            target.GetComponent<WhiteFlash>()?.Flash();
        }
    }

    private bool IsTipper(Collider2D enemy)
    {
        if (bladeTip == null)
        {
            return false;
        }

        Vector2 grip = transform.position;
        Vector2 blade = (Vector2)bladeTip.position - grip;
        float length = blade.magnitude;

        if (length < 0.0001f)
        {
            return false;
        }

        float along = Vector2.Dot((Vector2)enemy.bounds.center - grip, blade.normalized);
        float ratio = along / length;

        return ratio >= param.tipperRatio;
    }
}