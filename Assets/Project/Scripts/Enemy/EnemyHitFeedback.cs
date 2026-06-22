using System.Collections;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 敵が被弾(HP減少)したときにSpriteRendererの色を短時間点滅させる演出専用コンポーネント。
/// 単一責任=「被弾点滅の表示のみ」。撃破・ノックバック・接触ダメージの処理は一切行わない。
/// </summary>
public class EnemyHitFeedback : MonoBehaviour
{
    [Tooltip("HP変化の通知元。未設定なら自身から自動取得")]
    [SerializeField] private Health health;
    [Tooltip("点滅対象。未設定なら子から自動取得")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("点滅の合計時間(秒)")]
    [SerializeField] private float blinkDuration = 0.2f;
    [Tooltip("色の切り替え間隔(秒)")]
    [SerializeField] private float blinkInterval = 0.05f;
    [Tooltip("被弾時に乗せる色")]
    [SerializeField] private Color flashColor = Color.white;

    // WhiteFlashが未実装でもコンパイルできるよう、型参照ではなく文字列GetComponent+リフレクションで疎結合に確認する。
    // WhiteFlash.csが存在しない/付いていない場合はnullのままで、点滅抑制ロジックは常にfalseを返す。
    private Component _whiteFlash;
    private PropertyInfo _isFlashingProperty;

    private int _previousHp;
    private Color _originalColor;
    private Coroutine _blinkCoroutine;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (health == null)
        {
            Debug.LogWarning($"[EnemyHitFeedback] {name} : Healthが見つかりません。", this);
            return;
        }

        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[EnemyHitFeedback] {name} : SpriteRendererが見つかりません。", this);
            return;
        }

        _whiteFlash = GetComponent("WhiteFlash");
        _isFlashingProperty = _whiteFlash?.GetType().GetProperty("IsFlashing");

        _previousHp = health.CurrentHp;
    }

    private void OnEnable()
    {
        if (health == null || spriteRenderer == null) return;
        health.OnHpChanged += HandleHpChanged;
    }

    private void OnDisable()
    {
        if (health == null) return;
        health.OnHpChanged -= HandleHpChanged;
    }

    private void HandleHpChanged(int currentHp, int maxHp)
    {
        // 回復(currentHp >= _previousHp)では点滅させない
        if (currentHp < _previousHp)
        {
            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                spriteRenderer.color = _originalColor;
            }
            _blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }

        _previousHp = currentHp;
    }

    private IEnumerator BlinkCoroutine()
    {
        _originalColor = spriteRenderer.color;
        float elapsed = 0f;
        bool isFlash = true;
        while (elapsed < blinkDuration)
        {
            // WhiteFlashが発光中は色変更を抑制(競合調停)
            if (!IsWhiteFlashing())
            {
                spriteRenderer.color = isFlash ? flashColor : _originalColor;
            }
            isFlash = !isFlash;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        spriteRenderer.color = _originalColor;
        _blinkCoroutine = null;
    }

    private bool IsWhiteFlashing()
    {
        if (_whiteFlash == null || _isFlashingProperty == null) return false;
        return (bool)_isFlashingProperty.GetValue(_whiteFlash);
    }

    private void OnDestroy()
    {
        if (health != null) health.OnHpChanged -= HandleHpChanged;
        StopAllCoroutines();
        if (spriteRenderer != null) spriteRenderer.color = _originalColor;
    }
}
