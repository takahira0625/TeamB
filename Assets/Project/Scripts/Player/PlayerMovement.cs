using UnityEngine;
using UnityEngine.InputSystem;

// プレイヤーをA/Dキーで左右に動かすスクリプト
public class PlayerMovement : MonoBehaviour
{
    // 移動の速さ（publicにするとInspectorで変えられる）
    public float moveSpeed = 5f;

    // チャージ中かどうかを見るための剣のチャージ担当。
    // 未指定なら子オブジェクト(SwordPivot)から自動で取得する。
    [SerializeField] private SwordCharge swordCharge;

    void Awake()
    {
        // Inspectorで指定されていなければ、子から探して自動配線する
        if (swordCharge == null)
        {
            swordCharge = GetComponentInChildren<SwordCharge>();
        }
    }

    // ゲーム中、毎フレーム呼ばれる関数
    void Update()
    {
        // チャージ中は移動を完全ロック（確定ルール）。何も動かさずに抜ける
        if (swordCharge != null && swordCharge.IsCharging)
        {
            return;
        }

        // A=-1、D=+1 を受け取る
        float move = 0f;
        if (Keyboard.current.aKey.isPressed)
        {
            move = -1f;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            move = 1f;
        }

        // 入力に応じて左右に動かす
        transform.Translate(move * moveSpeed * Time.deltaTime, 0f, 0f);
    }
}