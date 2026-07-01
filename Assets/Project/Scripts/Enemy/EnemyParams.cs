using UnityEngine;

/// <summary>
/// 敵関連の調整パラメータ(筋肉)をプログラム(骨)から分離して持つScriptableObject。
/// Projectで右クリック → Create → Params → EnemyParams で作り、
/// EnemyMovement / EnemyContactDamage / Knockbackable / EnemyDeath に同じアセットを割り当てる。
/// 敵の種類ごとに別アセットを作れる(通常敵・メタル敵など。SwordParamsと同じ流儀)。
/// ※撃破スプライト・SE・エフェクトなどのアセット参照はプレハブ側のSerializeFieldに残す。
/// ※HP(maxHp)は敵/プレイヤー共用のHealthが持つため、ここには入れない。
/// </summary>
[CreateAssetMenu(fileName = "EnemyParams", menuName = "Params/EnemyParams")]
public class EnemyParams : ScriptableObject
{
    [Header("移動")]
    [Tooltip("左へ進む速さ")]
    public float moveSpeed = 2f;

    [Header("接触ダメージ")]
    [Tooltip("プレイヤーへの接触1回あたりのダメージ量")]
    public int contactDamage = 10;

    [Header("ぶっ飛ばし後のスタン")]
    [Tooltip("弾かれてから前進を再開するまでの秒数(手応えの調整軸。目安0.2〜0.4)")]
    public float stunDuration = 0.3f;

    [Header("撃破演出")]
    [Tooltip("撃破絵を表示してから消滅するまでの秒数(目安0.1〜0.3)")]
    public float deathDisplayDuration = 0.2f;
}
