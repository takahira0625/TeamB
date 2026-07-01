using UnityEngine;

/// <summary>
/// 白フラッシュ演出の調整パラメータ(筋肉)をプログラム(骨)から分離して持つScriptableObject。
/// Projectで右クリック → Create → Params → FlashParams で作り、WhiteFlash に割り当てる。
/// WhiteFlashは敵・プレイヤー両方に付くため、用途別に複数アセットを作れる
/// (例: EnemyCritFlash / PlayerBlinkFlash)。SwordParamsと同じ流儀。
/// 再生中でもInspectorから調整できる。
/// ※フラッシュ用シェーダーはアセット参照なのでWhiteFlash側のSerializeFieldに残す。
/// </summary>
[CreateAssetMenu(fileName = "FlashParams", menuName = "Params/FlashParams")]
public class FlashParams : ScriptableObject
{
    [Header("発光")]
    [Tooltip("発光色(既定＝白)")]
    public Color flashColor = Color.white;
    [Range(0f, 1f)]
    [Tooltip("白の強さ。0=変化なし / 1=完全に発光色。0.6なら薄く白く、形は見える")]
    public float flashAmount = 0.6f;
    [Tooltip("白→元に戻るまでの時間(秒)")]
    public float flashDuration = 0.08f;
}
