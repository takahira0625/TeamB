using UnityEngine;

/// <summary>
/// チャージ中のプレイヤー本体アニメーション制御。
/// 単一責任=「SwordChargeの状態を読み、チャージ段階に応じた本体スプライトを再生する。
/// 攻撃(振り)の瞬間はそのチャージ画像のまま停止し、攻撃が終わったら待機(Animator)に戻す」。
///
/// 待機アニメは既存のAnimatorが担当しているので、チャージ中だけAnimatorを止めて
/// 本体スプライトを直接上書きする(止めないとAnimatorが毎フレーム上書きしてしまうため)。
/// 画像や速さはInspectorで調整する(調整値=筋肉、ロジック=骨 の分離)。
///
/// ★このスクリプトはプレイヤー本体(SpriteRenderer と 待機用Animator を持つオブジェクト)に付ける。
/// </summary>
public class PlayerChargeAnimator : MonoBehaviour
{
    /// <summary>1段階分のチャージ画像。intro(最初の1枚)→loop(繰り返す数枚)の構成。</summary>
    [System.Serializable]
    public class ChargeStageSprites
    {
        [Tooltip("最初に1回だけ表示する画像(例: チャージ1小)")]
        public Sprite intro;
        [Tooltip("introの後にこの順で繰り返し表示する画像(例: チャージ2小, チャージ3小)")]
        public Sprite[] loop;
    }

    [Header("参照(未指定なら自動取得)")]
    [Tooltip("待機アニメを再生しているAnimator。チャージ中は止めて本体スプライトを上書きする")]
    [SerializeField] private Animator idleAnimator;
    [Tooltip("チャージ画像を表示する本体のSpriteRenderer")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [Tooltip("チャージ状態(段階)を持つSwordCharge。子のSwordPivotにある想定")]
    [SerializeField] private SwordCharge swordCharge;
    [Tooltip("攻撃(振り)中かを見るSwordSwing")]
    [SerializeField] private SwordSwing swordSwing;

    [Header("チャージ画像 (段階0=弱, 1=中, 2=最大 の順)")]
    [Tooltip("SwordParamsのstagesと同じ数・同じ並び順にする")]
    [SerializeField] private ChargeStageSprites[] stages;

    [Header("調整値")]
    [Tooltip("調整値をまとめたPlayerParamsアセット（再生速度・intro表示時間）")]
    [SerializeField] private PlayerParams param;

    // 今チャージアニメを再生中か(=Animatorを止めて上書きしている状態か)
    private bool isPlayingCharge;
    // 再生中の段階。段階が変わったらintroから出し直すために覚えておく
    private int currentStage = -1;
    // intro表示中か / ループの何枚目か / 次に切り替える時刻
    private bool inIntro;
    private int loopIndex;
    private float nextFrameTime;

    private void Awake()
    {
        // Inspector未指定なら自動配線(本体=このオブジェクト、剣の状態=子のSwordPivot)
        if (idleAnimator == null) idleAnimator = GetComponent<Animator>();
        if (bodyRenderer == null) bodyRenderer = GetComponent<SpriteRenderer>();
        if (swordCharge == null) swordCharge = GetComponentInChildren<SwordCharge>();
        if (swordSwing == null) swordSwing = GetComponentInChildren<SwordSwing>();
    }

    private void Update()
    {
        // (1) チャージ中: 段階に応じたアニメを再生する
        if (swordCharge != null && swordCharge.IsCharging)
        {
            PlayChargeForLevel(swordCharge.CurrentLevel);
            return;
        }

        // (2) 攻撃(振り)中: 何も更新しない＝直前のチャージ画像のまま停止(フリーズ)
        if (swordSwing != null && swordSwing.IsSwinging)
        {
            return;
        }

        // (3) それ以外(通常時): 待機アニメに戻す
        ReturnToIdle();
    }

    /// <summary>指定段階のチャージアニメを進める。</summary>
    private void PlayChargeForLevel(int level)
    {
        // 段階データ/調整値が無い/範囲外なら何もしない(配線ミス対策)
        if (param == null || stages == null || level < 0 || level >= stages.Length)
        {
            return;
        }

        // チャージ開始 or 段階が上がった瞬間: Animatorを止めてintroから出し直す
        if (!isPlayingCharge || currentStage != level)
        {
            isPlayingCharge = true;
            currentStage = level;
            if (idleAnimator != null) idleAnimator.enabled = false; // 待機アニメの上書きを止める
            StartIntro(level);
            return;
        }

        // 切り替え時刻が来たら次のコマへ
        if (Time.time >= nextFrameTime)
        {
            AdvanceFrame(level);
        }
    }

    /// <summary>introを表示してループ開始の準備をする。</summary>
    private void StartIntro(int level)
    {
        ChargeStageSprites data = stages[level];
        inIntro = true;
        loopIndex = 0;
        if (data.intro != null && bodyRenderer != null)
        {
            bodyRenderer.sprite = data.intro;
        }
        nextFrameTime = Time.time + param.chargeAnimIntroDuration;
    }

    /// <summary>ループ画像を1コマ進める(introの次は必ずloopの先頭から)。</summary>
    private void AdvanceFrame(int level)
    {
        ChargeStageSprites data = stages[level];
        if (data.loop == null || data.loop.Length == 0)
        {
            return; // ループ画像が無ければintroのまま止めておく
        }

        if (inIntro)
        {
            inIntro = false;
            loopIndex = 0;            // intro→ループ先頭
        }
        else
        {
            loopIndex = (loopIndex + 1) % data.loop.Length; // 末尾まで行ったら先頭へ
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.sprite = data.loop[loopIndex];
        }

        // 0以下が入っても固まらないよう保険
        float interval = param.chargeAnimFramesPerSecond > 0f ? 1f / param.chargeAnimFramesPerSecond : 0.1f;
        nextFrameTime = Time.time + interval;
    }

    /// <summary>待機アニメ(Animator)を再開する。</summary>
    private void ReturnToIdle()
    {
        if (!isPlayingCharge)
        {
            return; // すでに待機中なら何もしない
        }
        isPlayingCharge = false;
        currentStage = -1;
        if (idleAnimator != null) idleAnimator.enabled = true; // 待機アニメ再開
    }
}
