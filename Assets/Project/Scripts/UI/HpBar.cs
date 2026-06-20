using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 頭上HPバー表示。単一責任=「Health.OnHpChangedの通知をImage.fillAmountへ反映する」。
/// 追従は親子関係に任せ、位置追従コードは持たない。撃破演出・Destroy処理も持たない。
/// </summary>
public class HpBar : MonoBehaviour
{
    [Tooltip("HP変化の通知元。未設定なら親から自動取得")]
    [SerializeField] private Health health;
    [Tooltip("塗り量を変えるゲージ本体。未設定なら子から自動取得")]
    [SerializeField] private Image fillImage;

    private void Awake()
    {
        if (fillImage == null) fillImage = GetComponentInChildren<Image>();
        if (health == null) health = GetComponentInParent<Health>();
    }

    private void OnEnable()
    {
        if (health == null)
        {
            Debug.LogWarning($"[HpBar] {name} : Healthが見つからないため購読をスキップします。", this);
            return;
        }

        health.OnHpChanged += UpdateFill;

        // OnEnableはHealth.Awake()（CurrentHp=maxHpの初期化）より後に呼ばれるため、
        // ここではHealthの他メンバーを読まず「満タン」を表す比率1.0だけで初期表示する
        UpdateFill(1, 1);
    }

    private void OnDisable()
    {
        if (health == null) return;
        health.OnHpChanged -= UpdateFill;
    }

    private void UpdateFill(int current, int max)
    {
        if (max <= 0) return;
        if (fillImage == null) return;

        fillImage.fillAmount = (float)current / max;
    }
}
