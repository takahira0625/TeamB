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

    // 今向いている角度。チャージで振りかぶった角度からそのまま振り下ろすために記録する
    private float currentAngle;

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

        // 初回の振りなどでフレームが大きく処理落ちすると、物理(FixedUpdate)が遅れを
        // まとめて取り戻すため、振りの複数ステップが1コマに圧縮されて「弧でなく振り払い」に
        // 見える。1フレームで進める物理時間に上限を設けることで、重いフレームでも
        // 振りが複数コマに分かれて弧として見えるようにする。
        // ※Time.maximumDeltaTimeはゲーム全体に効くグローバル設定。代償として、極端に重い
        //   フレームではゲーム全体が一瞬だけスローになる(単発ならほぼ気付かない)。
        if (Time.maximumDeltaTime > maxFrameStep)
        {
            Time.maximumDeltaTime = maxFrameStep;
        }
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

    /// <summary>
    /// チャージ中の構え(振りかぶり)を見せる。SwordChargeが毎フレーム呼ぶ。
    /// t01: チャージの進み具合(0〜1)。0で通常の構え、1で最大の振りかぶり。
    /// </summary>
    public void ShowChargePose(float t01)
    {
        // 振り中は振り側の表示を優先(触らない)
        if (isSwinging)
        {
            return;
        }

        SetVisible(true);   // チャージ中は剣を見せる

        // 構え角度 → 振りかぶり角度 へ、溜めの進みに応じて少し持ち上げる
        float angle = Mathf.Lerp(param.startAngle, param.chargeAngle, Mathf.Clamp01(t01));
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
        IsAttacking = true;                                        // 攻撃判定オン
        SetVisible(true);                                          // 振りの間だけ表示
        // 振り出しは「今の角度」から。チャージで振りかぶっていればその角度から振り下ろす
        yield return Rotate(currentAngle, param.endAngle, param.swingDuration);       // 振り(速い)
        IsAttacking = false;                                       // 攻撃判定オフ
        SetVisible(false);                                         // 振り終わりで隠す
        yield return Rotate(param.endAngle, param.startAngle, param.returnDuration);  // 戻し
        SetAngle(param.startAngle);                                // 構えの角度に戻し切る
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
            float angle = Mathf.Lerp(from, to, k);
            currentAngle = angle;   // 今の角度を覚える(振り下ろしの起点に使う)
            rb.MoveRotation(angle);
            yield return new WaitForFixedUpdate();
        }
        currentAngle = to;
        rb.MoveRotation(to);
    }

    private void SetAngle(float z)
    {
        currentAngle = z;   // 今の角度を覚える
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