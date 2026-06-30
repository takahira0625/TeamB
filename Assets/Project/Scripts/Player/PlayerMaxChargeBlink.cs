using UnityEngine;

/// <summary>
/// 最大ため中、プレイヤーを白く点滅させる。単一責任=「最大ためかを監視し、その間だけ白点滅を出す」。
/// 白くする処理はWhiteFlashに任せ、ここは『いつ光らせるか』だけを持つ。
/// プレイヤー本体に付ける。SwordChargeとWhiteFlashが必要。
/// </summary>
public class PlayerMaxChargeBlink : MonoBehaviour
{
    [Header("参照(未設定なら自動取得)")]
    [Tooltip("チャージ状態を見るSwordCharge。子のSwordPivotにある想定")]
    [SerializeField] private SwordCharge swordCharge;
    [Tooltip("プレイヤーを白くするWhiteFlash")]
    [SerializeField] private WhiteFlash whiteFlash;

    [Header("点滅")]
    [Tooltip("白く光らせる間隔(秒)。WhiteFlashのflashDurationより大きくすると点滅になる")]
    [SerializeField] private float blinkInterval = 0.16f;

    private float timer;

    private void Awake()
    {
        if (swordCharge == null)
        {
            swordCharge = GetComponentInChildren<SwordCharge>();
        }
        if (whiteFlash == null)
        {
            whiteFlash = GetComponent<WhiteFlash>();
        }
    }

    private void Update()
    {
        // 最大ため中でなければ点滅しない(タイマーもリセット)
        if (swordCharge == null || !swordCharge.IsMaxCharged)
        {
            timer = 0f;
            return;
        }

        // 最大ための間、一定間隔でWhiteFlashを1回ずつ呼んで点滅させる
        timer += Time.deltaTime;
        if (timer >= blinkInterval)
        {
            timer = 0f;
            if (whiteFlash != null)
            {
                whiteFlash.Flash();
            }
        }
    }
}
