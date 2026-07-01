using UnityEngine;

/// <summary>
/// HP関連の調整パラメータ(筋肉)をプログラム(骨)から分離して持つScriptableObject。
/// Projectで右クリック → Create → Params → HealthParams で作り、Health に割り当てる。
/// ※Healthはプレイヤー・敵で共用し、個体ごとに最大HPが違うため、
///   用途別に複数アセットを作って割り当てる(例: PlayerHealth=100 / EnemyHealth=100 / MetalHealth=1)。
/// 再生中でもInspectorから調整できる。
/// </summary>
[CreateAssetMenu(fileName = "HealthParams", menuName = "Params/HealthParams")]
public class HealthParams : ScriptableObject
{
    [Header("HP")]
    [Tooltip("最大HP")]
    public int maxHp = 100;
}
