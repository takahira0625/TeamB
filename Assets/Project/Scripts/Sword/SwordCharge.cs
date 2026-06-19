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
    /// 現在のチャージ段階内での進捗(0〜1)。チャージバーをなめらかに伸ばすための読み取り専用値。
    /// 非チャージ中は0、最大段階まで溜まっていれば1を返す。値は変化させず計算して返すだけ。
    /// </summary>
    public float ChargeProgress01
    {
        get
        {
            // チャージしていない、またはparam未設定なら進捗なし
            if (!isCharging || param == null || param.stages.Length == 0)
            {
                return 0f;
            }

            // 最大段階まで溜まっていれば、これ以上の区間はないので1
            int maxIndex = param.stages.Length - 1;
            if (CurrentLevel >= maxIndex)
            {
                return 1f;
            }

            // 現在段階の到達時間〜次段階の到達時間を 0〜1 に正規化して返す
            float current = param.stages[CurrentLevel].chargeTime;
            float next = param.stages[CurrentLevel + 1].chargeTime;
            return Mathf.InverseLerp(current, next, chargeTimer);
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
        // 振り(構え戻し含む)が終わるまでは新しいチャージを始めない
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

        // 段階を1回だけ判定して使い回す
        int level = EvaluateLevel(chargeTimer);
        var stage = param.stages[level];

        // 段階に応じて各担当へ値を渡してから振らせる
        if (swing != null)
        {
            swing.SetReach(stage.reach);
        }
        if (hitbox != null)
        {
            hitbox.SetKnockback(stage.knockback);
            hitbox.SetChargeStage(level);   // ← stage と必ず同じ段階になる
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