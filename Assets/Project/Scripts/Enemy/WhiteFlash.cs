using System.Collections;
using UnityEngine;

/// <summary>
/// 敵の白フラッシュ演出。単一責任=「呼ばれたら自分のスプライトを一瞬だけ白く寄せて元に戻す」だけ。
/// 画面全体ではなく、この敵スプライトのみを白くする。
/// 発光の色・強さ・時間は FlashParams(SO) を既定値として使う（敵用/プレイヤー用で別アセット可）。
/// 発火の判断はSwordHitbox側が持ち、クリティカル時にFlash()を直接呼ぶ。
/// 通常被弾(EnemyHitFeedback)は Flash(amount, color) で暗い色を指定し、白(クリティカル)と見分ける。
/// </summary>
public class WhiteFlash : MonoBehaviour
{
    [Header("参照（未設定なら自動取得）")]
    [Tooltip("白くする対象のSpriteRenderer。未指定なら自身または子から取得")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("フラッシュ用シェーダー(Custom/SpriteFlash)。未指定ならShader.Findで探す")]
    [SerializeField] private Shader flashShader;
    [Tooltip("発光色・強さ・時間をまとめたFlashParamsアセット（敵用/プレイヤー用で別アセット可）")]
    [SerializeField] private FlashParams param;

    private Material originalMaterial;
    private Material flashMaterial;
    private Coroutine flashRoutine;

    private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

    // 未割り当て時の保険（FlashParamsが無くても最低限光る/戻る）
    private const float DefaultDuration = 0.08f;

    /// <summary>発光中か。敵被弾点滅との競合調停で参照する(発光中は点滅側が色変更をスキップ)。</summary>
    public bool IsFlashing { get; private set; }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        if (flashShader == null)
        {
            flashShader = Shader.Find("Custom/SpriteFlash");
        }

        if (spriteRenderer != null)
        {
            // 元のマテリアルを覚えておく(Flash後に必ず戻すため)。sharedMaterialで読むのはバッチングを切らないため。
            originalMaterial = spriteRenderer.sharedMaterial;
            if (flashShader != null)
            {
                flashMaterial = new Material(flashShader);
                if (param != null)
                {
                    flashMaterial.SetColor(FlashColorId, param.flashColor);
                }
            }
        }
    }

    private void OnDestroy()
    {
        // 自分で生成したマテリアルは自分で破棄する(リーク防止)
        if (flashMaterial != null)
        {
            Destroy(flashMaterial);
        }
    }

    /// <summary>既定(FlashParams)の色・強さで発光。SwordHitboxがクリティカル時に呼ぶ。</summary>
    public void Flash()
    {
        if (param == null)
        {
            return;
        }
        Flash(param.flashAmount, param.flashColor);
    }

    /// <summary>強さを指定して発光。色は既定(FlashParams)。</summary>
    public void Flash(float amount)
    {
        if (param == null)
        {
            return;
        }
        Flash(amount, param.flashColor);
    }

    /// <summary>
    /// 強さと色を指定して発光。EnemyHitFeedbackが通常被弾を暗い色で呼び、
    /// 先端クリティカル(白)と見分けられるようにするために使う。
    /// </summary>
    public void Flash(float amount, Color color)
    {
        if (spriteRenderer == null || flashMaterial == null)
        {
            return;
        }

        // すでに光っている最中なら止めて引き直す(常に元のマテリアルへ戻し切る)
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            Restore();
        }

        // 呼び出しごとに色を設定する(前回の色が残らないように)
        flashMaterial.SetColor(FlashColorId, color);
        flashRoutine = StartCoroutine(FlashRoutine(amount));
    }

    private IEnumerator FlashRoutine(float amount)
    {
        IsFlashing = true;

        flashMaterial.SetFloat(FlashAmountId, amount);
        spriteRenderer.material = flashMaterial;

        float duration = param != null ? param.flashDuration : DefaultDuration;
        yield return new WaitForSeconds(duration);

        Restore();
        flashRoutine = null;
        IsFlashing = false;
    }

    // 元のマテリアルへ戻す
    private void Restore()
    {
        spriteRenderer.material = originalMaterial;
    }
}
