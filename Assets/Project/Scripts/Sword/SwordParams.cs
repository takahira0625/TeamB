using UnityEngine;

/// <summary>
/// 剣の調整パラメータ(筋肉)をプログラム(骨)から分離して持つScriptableObject。
/// Projectで右クリック → Create → Params → SwordParams で作り、
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
        [Tooltip("この段階で与えるダメージ量")]
        public int damage = 10;
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

    [Header("構え（チャージ中の振りかぶり）")]
    [Tooltip("チャージ最大時に剣を持ち上げる角度。startAngleより少し大きくすると振りかぶって見える")]
    public float chargeAngle = 80f;

    [Header("チャージ段階 (弱 → 中 → 最大)")]
    public ChargeStage[] stages =
    {
        new ChargeStage { chargeTime = 0f,   reach = 1.0f, knockback = 6f,  damage = 10 },
        new ChargeStage { chargeTime = 0.7f, reach = 1.4f, knockback = 11f, damage = 20 },
        new ChargeStage { chargeTime = 1.6f, reach = 1.9f, knockback = 18f, damage = 35 },
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
    [Tooltip("先端ヒット時のダメージ倍率(ノックバック倍率とは独立)")]
    public float tipperDamageMul = 1.5f;

    [Header("ヒットストップ")]
    [Tooltip("段階別が無い場合に使う基本停止フレーム数(60fps換算)")]
    public int baseStopFrames = 3;
    [Tooltip("段階ごとの停止フレーム数。stagesと添字を合わせる(弱 → 中 → 最大)")]
    public int[] stopFramesPerStage = { 2, 5, 10 };
    [Tooltip("先端ヒット時に停止フレームへかける倍率(目安 1.5〜2.0)")]
    public float tipperStopMul = 1.8f;
}