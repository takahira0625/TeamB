using UnityEngine;

/// <summary>
/// カメラ関連の調整パラメータ(筋肉)をプログラム(骨)から分離して持つScriptableObject。
/// Projectで右クリック → Create → Params → CameraParams で作り、
/// CameraShake / cameramovement / LeftEdgeBlocker に同じアセットを割り当てる。
/// 再生中でもInspectorから調整できる。
/// </summary>
[CreateAssetMenu(fileName = "CameraParams", menuName = "Params/CameraParams")]
public class CameraParams : ScriptableObject
{
    [Header("自動横スクロール")]
    [Tooltip("カメラを右へ流す速さ(cameramovement)")]
    public float scrollSpeed = 2.0f;

    [Header("カメラ揺れ (ヒット時)")]
    [Tooltip("段階ごとの揺れ幅。配列の添字=チャージ段階(0始まり)")]
    public float[] amplitudeByLevel = { 0.05f, 0.1f, 0.2f };
    [Tooltip("段階が配列範囲外/取れない時に使う基準の揺れ幅")]
    public float defaultAmplitude = 0.1f;
    [Tooltip("1回の揺れの長さ(秒)")]
    public float shakeDuration = 0.15f;
    [Tooltip("減衰の効き具合。大きいほど早く収まる(1で線形)")]
    public float shakeDamping = 1.0f;
    [Tooltip("揺れの細かさ。大きいほど小刻みに震える")]
    public float shakeFrequency = 25f;

    [Header("左端ブロック (プレイヤーの左移動制限)")]
    [Tooltip("プレイヤーの中心から、見た目の左端までの距離")]
    public float leftOffsetFromCenter = 0.1f;
    [Tooltip("画面左端から少し余白を空けたい場合だけ使う")]
    public float edgePadding = 0.0f;
}
