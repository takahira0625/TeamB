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

    // 元のマテリアル(Awakeでキャッシュ)。Flash後に必ずここへ戻す。
    private Material originalMaterial;
    // このコンポーネント専用に生成するフラッシュ用マテリアル。OnDestroyで破棄する。
    private Material flashMaterial;

    // 走行中のFlashコルーチン。多重呼び出し時に止めて引き直すため保持する。
    private Coroutine flashRoutine;

    // シェーダープロパティ名のID(毎回文字列照合しないようキャッシュ)
    private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

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
            // 元のマテリアルを覚えておく(Flash後に必ず戻すため)。
            // sharedMaterialで読むのは、materialで読むとUnityが複製を作ってバッチングが切れるため。
            originalMaterial = spriteRenderer.sharedMaterial;

            // フラッシュ用マテリアルを自分専用に1つ作る。スプライトのテクスチャは
            // SpriteRendererが_MainTexへ自動で渡すので、ここでは色と強さだけ設定する。
            if (flashShader != null)
            {
                flashMaterial = new Material(flashShader);
                flashMaterial.SetColor(FlashColorId, flashColor);
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

    /// <summary>
    /// 即座に白発光を開始する。多重呼び出しは走行中コルーチンを止めて引き直す。
    /// SwordHitboxがクリティカル(段階3×先端)時に直接呼ぶ。
    /// </summary>
    public void Flash()
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
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        IsFlashing = true;

        // フラッシュ用マテリアルへ差し替え、白の強さを設定する
        flashMaterial.SetFloat(FlashAmountId, flashAmount);
        spriteRenderer.material = flashMaterial;

        yield return new WaitForSeconds(flashDuration);

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
