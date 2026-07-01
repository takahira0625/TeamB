using System.Collections;
using UnityEngine;

/// <summary>
/// プレイヤーがゴールの渦の中心へ螺旋を描いて吸い込まれる演出。単一責任=「呼ばれたら螺旋で中心へ寄せ、縮小・自転して消えるまで」を1回再生するだけ。
/// ゴール判定・シーン遷移・ゴールの見た目は一切持たない。AbsorbInto(中心, 完了コールバック)で起動する。
/// 2D前提(Rigidbody2D / Z軸回転 / XY平面の螺旋)。プレイヤー本体(PF_Player)に付ける。
/// </summary>
public class PlayerGoalAbsorb : MonoBehaviour
{
    [Header("演出パラメータ")]
    [Tooltip("吸い込み全体の時間(秒)")]
    [SerializeField] private float absorbDuration = 1.2f;
    [Tooltip("渦の中心を回る周回数")]
    [SerializeField] private float orbitTurns = 1.5f;
    [Tooltip("中心へ寄る速さ(0→1で半径を1→0に)。既定は線形")]
    [SerializeField] private AnimationCurve radiusCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Tooltip("縮み方のカーブ。既定は線形")]
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Tooltip("自身の回転総量(度)。0で自転なし")]
    [SerializeField] private float selfSpinDegrees = 540f;

    [Header("参照(未割り当てならAwakeで自動取得)")]
    [Tooltip("移動スクリプト。未割り当てなら同じオブジェクトから取得")]
    [SerializeField] private PlayerMovement playerMovement;
    [Tooltip("剣チャージ。子SwordPivotにある想定なのでInChildrenで取得")]
    [SerializeField] private SwordCharge swordCharge;
    [Tooltip("プレイヤーのRigidbody2D。未割り当てなら同じオブジェクトから取得")]
    [SerializeField] private Rigidbody2D rb;

    [Header("前面表示")]
    [Tooltip("前面に出す対象のSpriteRenderer。未割り当てなら同じオブジェクトから取得")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("吸い込み中に設定するSortingOrder。ゴールより大きくして後ろに回り込ませない")]
    [SerializeField] private int frontSortingOrder = 100;

    // 多重発火防止。演出中の再呼び出しは無視する。
    private bool isAbsorbing;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        if (swordCharge == null)
        {
            swordCharge = GetComponentInChildren<SwordCharge>();
        }
    }

    /// <summary>
    /// goalCenterを中心に螺旋で吸い込む＋localScaleを1→0＋自身を回転。完了でonComplete?.Invoke()。
    /// 演出中(isAbsorbing)の再呼び出しは即returnで無視する。
    /// </summary>
    public void AbsorbInto(Vector3 goalCenter, System.Action onComplete)
    {
        if (isAbsorbing)
        {
            return;
        }
        isAbsorbing = true;
        StartCoroutine(AbsorbRoutine(goalCenter, onComplete));
    }

    private IEnumerator AbsorbRoutine(Vector3 goalCenter, System.Action onComplete)
    {
        // 操作ロック。移動と剣チャージの両方を止める(剣入力は子SwordPivotのSwordChargeが独立に読むため)。
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        if (swordCharge != null)
        {
            swordCharge.enabled = false;
        }

        // 物理競合回避。速度を消してKinematicへ。演出後に戻す必要はない(この後破棄される想定)。
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // ゴールオブジェクトの後ろに回り込まないよう、演出中はプレイヤーを前面へ出す。
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = frontSortingOrder;
        }

        // 開始時に中心からのオフセットを保存(z成分は維持する)。
        Vector3 startPos = transform.position;
        float keepZ = startPos.z;
        Vector3 offset = startPos - goalCenter;
        float startRadius = offset.magnitude;
        float startAngle = Mathf.Atan2(offset.y, offset.x);
        Vector3 startScale = transform.localScale;

        float elapsed = 0f;
        while (elapsed < absorbDuration)
        {
            float t = Mathf.Clamp01(elapsed / absorbDuration);

            float angle = startAngle + orbitTurns * 2f * Mathf.PI * t;
            float radius = startRadius * (1f - radiusCurve.Evaluate(t));

            // 位置: 中心 + (cosθ, sinθ, 0)*半径。zは元のstartPos.zを維持。
            Vector3 pos = goalCenter + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            pos.z = keepZ;
            transform.position = pos;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, scaleCurve.Evaluate(t));
            transform.rotation = Quaternion.Euler(0f, 0f, selfSpinDegrees * t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 終了状態を確定(中心へ合わせ・z維持・スケール0) → 完了通知
        Vector3 endPos = goalCenter;
        endPos.z = keepZ;
        transform.position = endPos;
        transform.localScale = Vector3.zero;
        onComplete?.Invoke();
    }
}
