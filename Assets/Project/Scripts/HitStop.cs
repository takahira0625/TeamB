using System.Collections;
using UnityEngine;

/// <summary>
/// ヒットの瞬間、チャージ段階×先端の当たり方に応じた長さだけ Time.timeScale を下げ、必ず1に戻す。
/// 単一責任=「手応えに応じた時間停止と確実な復帰」。敵は一切見ない。
/// SwordHitbox.OnHit を購読して起動する独立演出スクリプト(剣4分割には含めない)。
/// Time.timeScale はグローバルなので、シーンに1つだけ置くこと。
/// </summary>
public class HitStop : MonoBehaviour
{
    [Tooltip("ヒット通知の購読相手。未指定ならシーンから自動取得")]
    [SerializeField] private SwordHitbox hitbox;
    [Tooltip("停止フレーム数・倍率の読み出し元。剣スクリプトと同じSwordParamsを割り当てる")]
    [SerializeField] private SwordParams param;

    /// <summary>ヒットストップ中かどうか。多重起動の防止に使う。</summary>
    public bool IsStopping { get; private set; }

    private void Awake()
    {
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

        // 保険：停止中に無効化されても時間を必ず元に戻す(戻し忘れ事故の防止)
        Time.timeScale = 1f;
        IsStopping = false;
    }

    /// <summary>ヒット通知(段階, 先端可否)を受け、停止の長さを決めて時間を止める。</summary>
    private void HandleHit(int level, bool tipper)
    {
        if (param == null)
        {
            return;
        }

        // 多重起動の防止：停止中に来た新しいヒットは無視する(停止はごく短いため)
        if (IsStopping)
        {
            return;
        }

        // 段階別の値があればそれを、無ければ基本値を使う
        bool hasPerStage = param.stopFramesPerStage != null
                           && level >= 0
                           && level < param.stopFramesPerStage.Length;
        int frames = hasPerStage ? param.stopFramesPerStage[level] : param.baseStopFrames;

        // 先端ヒットは倍率をかけて長く止める(受け側を大きく見せる非対称)
        float totalFrames = frames * (tipper ? param.tipperStopMul : 1f);

        // フレーム→実時間に換算(60fps固定)
        float seconds = totalFrames / 60f;
        if (seconds <= 0f)
        {
            return;
        }

        StartCoroutine(HitStopRoutine(seconds));
    }

    private IEnumerator HitStopRoutine(float seconds)
    {
        IsStopping = true;
        try
        {
            Time.timeScale = 0f;                              // 0で完全停止(小さい値にすればスロー)
            yield return new WaitForSecondsRealtime(seconds); // ★実時間で待つ(timeScale=0でも進む)
        }
        finally
        {
            Time.timeScale = 1f;                              // どう抜けても必ず戻す
            IsStopping = false;
        }
    }
}
