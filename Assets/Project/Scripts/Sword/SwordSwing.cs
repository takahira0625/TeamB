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

    [Header("処理落ち対策")]
    [Tooltip("1フレームで進める物理時間の上限(秒)。重いフレームでも振りが1コマに圧縮されず弧が見えるようにする。小さいほど安定するが、重い時に一瞬スローになる")]
    [SerializeField] private float maxFrameStep = 0.03f;

    private bool isSwinging;
    private Vector3 baseScale;
    private Rigidbody2D rb;

    // 今向いている角度。チャージで振りかぶった角度から振り下ろすために使う
    private float currentAngle;

    /// <summary>振り(攻撃判定が有効な区間)中ならtrue。SwordHitboxが参照する。</summary>
    public bool IsAttacking { get; private set; }

    /// <summary>振り〜構え戻しまで動作中ならtrue。SwordChargeが二重発動を防ぐのに使う。</summary>
    public bool IsSwinging => isSwinging;

    private void Awake()
    {
        if (swordSprite == null)
        {
            swordSprite = GetComponentInChildren<SpriteRenderer>();
        }

        baseScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();

        if (Time.maximumDeltaTime > maxFrameStep)
        {
            Time.maximumDeltaTime = maxFrameStep;
        }
    }

    private void Start()
    {
        SetAngle(param.startAngle);
        SetVisible(false);
    }

    /// <summary>リーチ(剣の長さの倍率)を設定する。1.0で基準の長さ。</summary>
    public void SetReach(float lengthMul)
    {
        transform.localScale = new Vector3(baseScale.x, baseScale.y * lengthMul, baseScale.z);
    }

    /// <summary>
    /// チャージ中の構え（振りかぶり）を見せる。SwordChargeが毎フレーム呼ぶ。
    /// t01: チャージの進み具合(0〜1)。0で通常の構え、1で最大の振りかぶり。
    /// </summary>
    public void ShowChargePose(float t01)
    {
        if (isSwinging)
        {
            return;
        }

        if (param == null)
        {
            return;
        }

        SetVisible(true);

        float angle = Mathf.Lerp(
            param.startAngle,
            param.chargeAngle,
            Mathf.Clamp01(t01)
        );

        SetAngle(angle);
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

        IsAttacking = true;
        SetVisible(true);

        // チャージで振りかぶった角度から、そのまま振り下ろす
        yield return Rotate(currentAngle, param.endAngle, param.swingDuration);

        IsAttacking = false;
        SetVisible(false);

        yield return Rotate(param.endAngle, param.startAngle, param.returnDuration);

        SetAngle(param.startAngle);
        isSwinging = false;
    }

    private IEnumerator Rotate(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.fixedDeltaTime;
            float k = Mathf.Clamp01(t / duration);

            float angle = Mathf.Lerp(from, to, k);
            currentAngle = angle;

            rb.MoveRotation(angle);

            yield return new WaitForFixedUpdate();
        }

        currentAngle = to;
        rb.MoveRotation(to);
    }

    private void SetAngle(float z)
    {
        currentAngle = z;
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