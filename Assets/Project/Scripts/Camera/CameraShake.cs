using UnityEngine;

/// <summary>
/// ヒットの瞬間、チャージ段階に応じた幅でカメラを短く揺らして減衰させるだけ。
/// 単一責任=「手応えに応じたカメラの揺れ」。入力・当たり判定・スクロールには関与しない。
/// SwordHitbox.OnHit を購読して起動する独立演出スクリプト(剣4分割には含めない)。
///
/// ★親リグ方式で使うこと。空オブジェクト CameraRig の子に Main Camera を入れ、
///   スクロール(cameramovement)は親、このスクリプトは子の localPosition だけを揺らす。
///   こうするとワールド位置を書く自動スクロールと喧嘩しない。
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("参照")]
    [Tooltip("ヒット通知の発火元。未指定ならシーンから自動取得")]
    [SerializeField] private SwordHitbox hitbox;

    [Header("揺れ幅(チャージ段階別)")]
    [Tooltip("段階ごとの揺れ幅。配列の添字=チャージ段階(0始まり)")]
    [SerializeField] private float[] amplitudeByLevel = { 0.05f, 0.1f, 0.2f };
    [Tooltip("段階が配列範囲外/取れない時に使う基準の揺れ幅")]
    [SerializeField] private float defaultAmplitude = 0.1f;

    [Header("揺れの質")]
    [Tooltip("1回の揺れの長さ(秒)")]
    [SerializeField] private float duration = 0.15f;
    [Tooltip("減衰の効き具合。大きいほど早く収まる(1で線形)")]
    [SerializeField] private float damping = 1.0f;
    [Tooltip("揺れの細かさ。大きいほど小刻みに震える")]
    [SerializeField] private float frequency = 25f;

    private Vector3 baseLocalPosition;  // 揺れの基準にする最初のローカル位置(z=-10等を保持)
    private float currentAmplitude;     // 今回の揺れの初期振幅
    private float elapsed;              // 揺れ開始からの経過時間
    private bool isShaking;

    // PerlinNoiseのxとyで別の波形にするためのオフセット(同じ値だと斜め一直線に揺れてしまう)
    private const float NoiseSeedX = 0f;
    private const float NoiseSeedY = 100f;

    private void Awake()
    {
        // 揺れは基準位置からのオフセットで重ねる。最初のローカル位置を覚えておく。
        baseLocalPosition = transform.localPosition;

        // Inspectorで未指定ならシーンから探す(Unity 6の新API)
        if (hitbox == null)
        {
            hitbox = FindFirstObjectByType<SwordHitbox>();
        }
    }

    private void OnEnable()
    {
        // ヒットの瞬間を購読する
        if (hitbox != null)
        {
            hitbox.OnHit += HandleHit;
        }
    }

    private void OnDisable()
    {
        // 購読は必ず解除する(リトライのシーン再読み込みで破棄済み参照を呼ばないため)
        if (hitbox != null)
        {
            hitbox.OnHit -= HandleHit;
        }

        // 保険：揺れ中に無効化されても位置を基準へ戻す
        transform.localPosition = baseLocalPosition;
        isShaking = false;
    }

    /// <summary>ヒット通知(段階, 先端可否)を受け、段階に応じた幅で揺れを開始する。</summary>
    private void HandleHit(int level, bool tipper)
    {
        Shake(level);
    }

    /// <summary>チャージ段階を指定して揺れを開始する。amplitudeByLevel から揺れ幅を引く。</summary>
    public void Shake(int level)
    {
        // 段階に対応する揺れ幅を取る。範囲外なら基準幅でフォールバック。
        bool hasPerLevel = amplitudeByLevel != null
                           && level >= 0
                           && level < amplitudeByLevel.Length;
        currentAmplitude = hasPerLevel ? amplitudeByLevel[level] : defaultAmplitude;

        elapsed = 0f;
        isShaking = true;
    }

    private void LateUpdate()
    {
        if (!isShaking)
        {
            return;
        }

        elapsed += Time.deltaTime;

        // 終了：基準位置へ戻して終わる
        if (elapsed >= duration || duration <= 0f)
        {
            transform.localPosition = baseLocalPosition;
            isShaking = false;
            return;
        }

        // 経過に応じて振幅を減衰させる(1→0)。dampingで効き具合を変える。
        float fade = 1f - (elapsed / duration);
        fade = Mathf.Pow(fade, damping);
        float amplitude = currentAmplitude * fade;

        // PerlinNoiseで-1〜1の揺れを作る(滑らかにブレる)。xとyは別シードで独立させる。
        float time = elapsed * frequency;
        float offsetX = (Mathf.PerlinNoise(NoiseSeedX, time) * 2f) - 1f;
        float offsetY = (Mathf.PerlinNoise(NoiseSeedY, time) * 2f) - 1f;

        // 基準位置にオフセットを重ねる。Zは触らない(カメラ距離を保つ)。
        transform.localPosition = baseLocalPosition + new Vector3(offsetX * amplitude, offsetY * amplitude, 0f);
    }
}
