using System.Collections;
using UnityEngine;

/// <summary>
/// 剣の振りの動き。単一責任=「与えられたリーチで一度振る(回転＋表示)」。
/// 入力は見ない。Swing()で外から振らせる。チャージ判定はSwordChargeが担当。
/// 回転はKinematic Rigidbody2DのMoveRotationで物理に乗せる(速い振りでのすり抜け対策)。
/// ★このスクリプトは「握りの位置に置いた空オブジェクト(SwordPivot)」に付ける。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SwordSwing : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("調整値をまとめたSwordParamsアセット")]
    [SerializeField] private SwordParams param;
    [Tooltip("剣のスプライト。未指定なら子から自動取得する")]
    [SerializeField] private SpriteRenderer swordSprite;

    private bool isSwinging;
    private Vector3 baseScale;
    private Rigidbody2D rb;

    /// <summary>振り(攻撃判定が有効な区間)中ならtrue。SwordHitboxが参照する。</summary>
    public bool IsAttacking { get; private set; }

    /// <summary>振り〜構え戻しまで動作中ならtrue。SwordChargeが二重発動を防ぐのに使う。</summary>
    public bool IsSwinging => isSwinging;

    private void Awake()
    {
        // 未指定なら子の剣スプライトを自動で拾う
        if (swordSprite == null)
        {
            swordSprite = GetComponentInChildren<SpriteRenderer>();
        }
        baseScale = transform.localScale;   // リーチ1.0の基準サイズを記録
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SetAngle(param.startAngle);   // 最初は構えの角度で待機
        SetVisible(false);            // 通常時は剣を隠す
    }

    /// <summary>リーチ(剣の長さの倍率)を設定する。1.0で基準の長さ。</summary>
    public void SetReach(float lengthMul)
    {
        // 握りを起点に長さ方向(ローカルY)だけ伸ばす
        // ※剣が横長で伸び方がおかしいときは baseScale.y → baseScale.x 側に変える
        transform.localScale = new Vector3(baseScale.x, baseScale.y * lengthMul, baseScale.z);
    }

    /// <summary>外から振らせる。動作中は無視。</summary>
    public void Swing()
    {
        if (!isSwinging)
        {
            StartCoroutine(SwingRoutine());
        }
    }

    private IEnumerator SwingRoutine()
    {
        isSwinging = true;
        IsAttacking = true;                                        // 攻撃判定オン
        SetVisible(true);                                          // 振りの間だけ表示
        yield return Rotate(param.startAngle, param.endAngle, param.swingDuration);   // 振り(速い)
        IsAttacking = false;                                       // 攻撃判定オフ
        SetVisible(false);                                         // 振り終わりで隠す
        yield return Rotate(param.endAngle, param.startAngle, param.returnDuration);  // 戻し
        isSwinging = false;
    }

    private IEnumerator Rotate(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.fixedDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            // 物理で回す。フレーム間の回転を物理が補間するのですり抜けない。
            rb.MoveRotation(Mathf.Lerp(from, to, k));
            yield return new WaitForFixedUpdate();
        }
        rb.MoveRotation(to);
    }

    private void SetAngle(float z)
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, z);
    }

    private void SetVisible(bool visible)
    {
        if (swordSprite != null)
        {
            swordSprite.enabled = visible;
        }
    }
}