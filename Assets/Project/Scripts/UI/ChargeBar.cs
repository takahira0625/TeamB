using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// チャージ中だけ頭上に表示し、段階の進捗(0〜1)と現在段階に応じた色をゲージに反映する。単一責任=「表示のみ」。
/// チャージ判定・攻撃発動はSwordCharge側の責務であり、このスクリプトは関与しない。
/// </summary>
public class ChargeBar : MonoBehaviour
{
    [Tooltip("チャージ状態の通知元。SwordPivotなど別階層に付くため明示割り当てが必要")]
    [SerializeField] private SwordCharge source;
    [Tooltip("塗り量を変えるゲージ本体。未設定なら子から自動取得")]
    [SerializeField] private Image fillImage;
    [Tooltip("段階色などをまとめたPlayerParamsアセット")]
    [SerializeField] private PlayerParams param;

    private void Awake()
    {
        if (fillImage == null) fillImage = GetComponentInChildren<Image>();

        if (source == null)
        {
            Debug.LogWarning($"[ChargeBar] {name} : source(SwordCharge)が未設定です。Inspectorで割り当ててください。", this);
        }
        if (fillImage == null)
        {
            Debug.LogWarning($"[ChargeBar] {name} : fillImageが見つかりません。子にImageを配置してください。", this);
            return;
        }

        // このスクリプトが付くオブジェクト自身をSetActive(false)するとUpdateが二度と呼ばれなくなるため、
        // 表示/非表示はfillImage.enabledの切り替えで行う
        fillImage.enabled = false;
    }

    private void Update()
    {
        if (source == null || fillImage == null) return;

        bool charging = source.IsCharging;
        fillImage.enabled = charging;

        if (charging)
        {
            fillImage.fillAmount = source.ChargeProgress01;
            fillImage.color = GetStageColor(source.CurrentLevel);
        }
    }

    private Color GetStageColor(int level)
    {
        if (param == null || param.chargeStageColors == null || param.chargeStageColors.Length == 0) return Color.white;

        int index = Mathf.Clamp(level, 0, param.chargeStageColors.Length - 1);
        return param.chargeStageColors[index];
    }
}
