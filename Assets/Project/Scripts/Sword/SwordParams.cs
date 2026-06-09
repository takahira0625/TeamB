using UnityEngine;

/// <summary>
/// 剣の調整パラメータ(筋肉)をプログラム(骨)から分離して持つScriptableObject。
/// Projectで右クリック → Create → Kissaki → SwordParams で作り、
/// SwordSwing / SwordCharge / SwordHitbox に同じアセットを割り当てる。
/// 再生中でもInspectorから調整できる。
/// </summary>
[CreateAssetMenu(fileName = "SwordParams", menuName = "Params/SwordParams")]
public class SwordParams : ScriptableObject
{
    [System.Serializable]
    public class ChargeStage
    {
        [Tooltip("この段階に到達するのに必要な累積チャージ時間(秒)。先頭(弱)は0")]
        public float chargeTime = 0f;
        [Tooltip("リーチ(剣の長さの倍率)。1.0で基準")]
        public float reach = 1f;
        [Tooltip("ぶっ飛ばしの強さ")]
        public float knockback = 6f;
    }

    [Header("振り")]
    [Tooltip("振り始め(構え)の角度")]
    public float startAngle = 60f;
    [Tooltip("振り終わりの角度")]
    public float endAngle = -120f;
    [Tooltip("振りにかける時間(秒)。短いほど鋭い")]
    public float swingDuration = 0.12f;
    [Tooltip("構えに戻すのにかける時間(秒)")]
    public float returnDuration = 0.18f;

    [Header("チャージ段階 (弱 → 中 → 最大)")]
    public ChargeStage[] stages =
    {
        new ChargeStage { chargeTime = 0f,   reach = 1.0f, knockback = 6f },
        new ChargeStage { chargeTime = 0.35f, reach = 1.4f, knockback = 11f },
        new ChargeStage { chargeTime = 0.8f, reach = 1.9f, knockback = 18f },
    };

    [Header("ぶっ飛ばし")]
    [Tooltip("上方向の成分(0で真横。重力ありなら少し上げると弧を描く)")]
    public float upwardBias = 1f;

    [Header("先端補正 (tipper)")]
    [Range(0f, 1f)]
    [Tooltip("握り=0, 剣先=1。この比率以上で当たると先端ヒット")]
    public float tipperRatio = 0.8f;
    [Tooltip("先端ヒット時のぶっ飛ばし倍率")]
    public float tipperKnockbackMul = 1.8f;
}