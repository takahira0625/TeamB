using UnityEngine;

/// <summary>
/// チャージ入力と段階判定。単一責任=「押し続けて溜め、離した段階に応じて攻撃を構成し振らせる」。
/// 段階ごとにリーチ(SwordSwing)とぶっ飛ばし(SwordHitbox)を設定してから振らせる。
/// SwordSwing・SwordHitboxと同じオブジェクト(SwordPivot)に付ける想定。
/// </summary>
public class SwordCharge : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("調整値をまとめたSwordParamsアセット")]
    [SerializeField] private SwordParams param;
    [Tooltip("未指定なら同じオブジェクトから取得")]
    [SerializeField] private SwordSwing swing;
    [SerializeField] private SwordHitbox hitbox;

    private bool isCharging;
    private float chargeTimer;

    /// <summary>チャージ中ならtrue。移動側がこれを見て移動をロックできる。</summary>
    public bool IsCharging => isCharging;
    /// <summary>現在のチャージ段階(0〜)。チャージUI等が参照できる。</summary>
    public int CurrentLevel { get; private set; }

    /// <summary>
    /// 押し始めから最大チャージまでを0〜1で返す連続ゲージ用の値。
    /// 段階でリセットせず、最後の段階の到達時間を満タン(1)とみなす。
    /// </summary>
    public float ChargeProgress01
    {
        get
        {
            if (!isCharging || param == null || param.stages.Length == 0)
            {
                return 0f;
            }
            float maxTime = param.stages[param.stages.Length - 1].chargeTime;
            if (maxTime <= 0f)
            {
                return 1f;
            }
            return Mathf.Clamp01(chargeTimer / maxTime);
        }
    }

    /// <summary>最大段階まで溜まっているか。最大ため演出(プレイヤー点滅など)が参照する。</summary>
    public bool IsMaxCharged
    {
        get
        {
            if (!isCharging || param == null || param.stages.Length == 0)
            {
                return false;
            }
            return CurrentLevel >= param.stages.Length - 1;
        }
    }

    private void Awake()
    {
        if (swing == null)
        {
            swing = GetComponent<SwordSwing>();
        }
        if (hitbox == null)
        {
            hitbox = GetComponent<SwordHitbox>();
        }
    }

    private void Update()
    {
        if (swing != null && swing.IsSwinging)
        {
            isCharging = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeTimer = 0f;
        }

        if (isCharging && Input.GetMouseButton(0))
        {
            chargeTimer += Time.deltaTime;
            CurrentLevel = EvaluateLevel(chargeTimer);
        }

        if (isCharging && Input.GetMouseButtonUp(0))
        {
            Release();
        }
    }

    private void Release()
    {
        isCharging = false;

        int level = EvaluateLevel(chargeTimer);
        var stage = param.stages[level];

        if (swing != null)
        {
            swing.SetReach(stage.reach);
        }
        if (hitbox != null)
        {
            hitbox.SetKnockback(stage.knockback);
            hitbox.SetChargeStage(level);
        }
        if (swing != null)
        {
            swing.Swing();
        }
    }

    /// <summary>溜め時間から段階を決める。chargeTimeは到達に必要な累積秒。</summary>
    private int EvaluateLevel(float t)
    {
        int level = 0;
        for (int i = 0; i < param.stages.Length; i++)
        {
            if (t >= param.stages[i].chargeTime)
            {
                level = i;
            }
        }
        return level;
    }
}