using UnityEngine;

/// <summary>
/// プレイヤー関連の調整パラメータ(筋肉)をプログラム(骨)から分離して持つScriptableObject。
/// Projectで右クリック → Create → Params → PlayerParams で作り、
/// PlayerMovement / PlayerInvincibility / PlayerMaxChargeBlink / PlayerChargeAnimator /
/// ChargeBar / GameOverController に同じアセットを割り当てる。
/// 再生中でもInspectorから調整できる。
/// ※画像・音などのアセット参照はプレハブ側のSerializeFieldに残す(ここには数値だけ)。
/// </summary>
[CreateAssetMenu(fileName = "PlayerParams", menuName = "Params/PlayerParams")]
public class PlayerParams : ScriptableObject
{
    [Header("移動")]
    [Tooltip("移動の速さ")]
    public float moveSpeed = 5f;
    [Range(0f, 1f)]
    [Tooltip("チャージ中の移動速度の倍率(0=動けない、1=通常と同じ)。ゆっくり歩かせる")]
    public float chargeMoveMultiplier = 0.2f;

    [Header("被弾後の無敵")]
    [Tooltip("被弾後に無敵でいる時間(秒)")]
    public float invincibleDuration = 1.5f;
    [Tooltip("無敵中にSpriteRendererをON/OFFする間隔(秒)")]
    public float invincibleBlinkInterval = 0.1f;

    [Header("最大ため点滅")]
    [Tooltip("最大ため中に白く光らせる間隔(秒)。WhiteFlashのflashDurationより大きくすると点滅になる")]
    public float maxChargeBlinkInterval = 0.16f;

    [Header("チャージ本体アニメ")]
    [Tooltip("ループ画像を1秒あたり何枚切り替えるか")]
    public float chargeAnimFramesPerSecond = 8f;
    [Tooltip("intro画像を表示しておく秒数")]
    public float chargeAnimIntroDuration = 0.1f;

    [Header("チャージゲージUI")]
    [Tooltip("CurrentLevel(段階)ごとの色。要素数が段階数より少ない場合は末尾の色を使い回す")]
    public Color[] chargeStageColors = { Color.white, Color.yellow, Color.red };

    [Header("ゲームオーバー遷移")]
    [Tooltip("死亡してからゲームオーバー画面へ移るまでの待ち時間(秒)")]
    public float gameOverDelaySeconds = 0.7f;
}
