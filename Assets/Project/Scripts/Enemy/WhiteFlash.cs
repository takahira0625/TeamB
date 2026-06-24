using System.Collections;
using UnityEngine;

/// <summary>
/// 敵の白フラッシュ演出。単一責任=「呼ばれたら自分のスプライトを一瞬だけ白く寄せて元に戻す」だけ。
/// 画面全体ではなく、この敵スプライトのみを白くする。
/// 白の強さ(flashAmount)は0〜1で調整でき、スプライトの形を残したまま薄く光らせられる。
/// 発火するかどうか(段階3×先端のときだけ)の判断はSwordHitbox側が持ち、
/// クリティカル時にここのFlash()を直接呼ぶ。WhiteFlashは参照を持たず呼ばれて待つだけ。
/// </summary>
public class WhiteFlash : MonoBehaviour
{
    [Header("参照（未設定なら自動取得）")]
    [Tooltip("白くする対象のSpriteRenderer。未指定なら自身または子から取得")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("フラッシュ用シェーダー(Custom/SpriteFlash)。未指定ならShader.Findで探す")]
    [SerializeField] private Shader flashShader;

    [Header("発光")]
    [Tooltip("発光色（既定＝白）")]
    [SerializeField] private Color flashColor = Color.white;
    [Range(0f, 1f)]
    [Tooltip("白の強さ。0=変化なし / 1=完全に発光色。0.6なら薄く白く、形は見える")]
    [SerializeField] private float flashAmount = 0.6f;
    [Tooltip("白→元に戻るまでの時間(秒)")]
    [SerializeField] private float flashDuration = 0.08f;

    private Material originalMaterial;
    private Material flashMaterial;
    private Coroutine flashRoutine;

    private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

    /// <summary>発光中か。敵被弾点滅との競合調停で参照する(発光中は点滅側が色変更をスキップ)。</summary>
    public bool IsFlashing { get; private set; }

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (flashShader == null)
            flashShader = Shader.Find("Custom/SpriteFlash");

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.sharedMaterial;
            if (flashShader != null)
            {
                flashMaterial = new Material(flashShader);
                flashMaterial.SetColor(FlashColorId, flashColor);
            }
        }
    }

    private void OnDestroy()
    {
        if (flashMaterial != null)
            Destroy(flashMaterial);
    }

    /// <summary>
    /// 既定の強さ（flashAmount）で発光。SwordHitbox がクリティカル時に呼ぶ。
    /// </summary>
    public void Flash()
    {
        Flash(flashAmount);
    }

    /// <summary>
    /// 強さを指定して発光。EnemyHitFeedback が被弾時に 0.25 で呼ぶ。
    /// </summary>
    public void Flash(float amount)
    {
        if (spriteRenderer == null || flashMaterial == null) return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            Restore();
        }
        flashRoutine = StartCoroutine(FlashRoutine(amount));
    }

    private IEnumerator FlashRoutine(float amount)
    {
        IsFlashing = true;
        flashMaterial.SetFloat(FlashAmountId, amount);
        spriteRenderer.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        Restore();
        flashRoutine = null;
        IsFlashing = false;
    }

    private void Restore()
    {
        spriteRenderer.material = originalMaterial;
    }
}