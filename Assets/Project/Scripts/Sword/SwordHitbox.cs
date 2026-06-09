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
    private readonly HashSet<Knockbackable> hitThisSwing = new();
    private bool wasAttacking;

    /// <summary>ぶっ飛ばしの強さを設定する(チャージ段階に応じてSwordChargeが渡す)。</summary>
    public void SetKnockback(float force)
    {
        knockbackForce = force;
    }

    private void Awake()
    {
        if (swing == null) swing = GetComponentInParent<SwordSwing>();
        if (hitCollider == null) hitCollider = GetComponentInChildren<Collider2D>();
        if (attackOrigin == null) attackOrigin = transform;

        if (hitCollider != null) hitCollider.enabled = false;  // 通常時は判定オフ
    }

    private void Update()
    {
        bool attacking = swing != null && swing.IsAttacking;

        // 攻撃中だけ剣の判定を有効化する
        if (hitCollider != null) hitCollider.enabled = attacking;

        // 振り始めの瞬間にヒット済みリストをリセット(1振りで同じ敵は1回だけ)
        if (attacking && !wasAttacking) hitThisSwing.Clear();
        wasAttacking = attacking;
    }

    // 攻撃中、剣のトリガーに敵が触れたら呼ばれる
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (swing == null || !swing.IsAttacking) return;  // 念のため攻撃中以外は無視

        var target = other.GetComponentInParent<Knockbackable>();
        if (target == null || hitThisSwing.Contains(target)) return;
        hitThisSwing.Add(target);

        // 先端ヒットなら倍率をかける
        bool tipper = IsTipper(other);
        float force = knockbackForce * (tipper ? param.tipperKnockbackMul : 1f);

        // プレイヤーと反対方向(横)へ。必要なら上成分を足す。
        float sign = Mathf.Sign(other.transform.position.x - attackOrigin.position.x);
        if (sign == 0f) sign = 1f;
        Vector2 dir = new Vector2(sign, param.upwardBias).normalized;

        target.ApplyKnockback(dir * force);

        // ★確認用ログ。判定が合っているか確かめたら消してOK。
        //   ここが演出(ヒットストップ・白フラッシュ)を足す場所になる。
        if (tipper) Debug.Log("切先ヒット！(tipper)");
    }

    /// <summary>敵が剣のどこで当たったか。握りからの距離÷剣の長さがしきい値以上なら先端。</summary>
    private bool IsTipper(Collider2D enemy)
    {
        if (bladeTip == null) return false;            // 目印未設定なら常に通常ヒット

        Vector2 grip = transform.position;             // SwordPivotの原点=握り
        Vector2 blade = (Vector2)bladeTip.position - grip;
        float length = blade.magnitude;
        if (length < 0.0001f) return false;

        // 敵の中心を「握り→剣先」方向へ射影した距離を、剣の長さで割る
        float along = Vector2.Dot((Vector2)enemy.bounds.center - grip, blade.normalized);
        float ratio = along / length;
        return ratio >= param.tipperRatio;
    }
}