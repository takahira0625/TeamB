using UnityEngine;
using UnityEngine.InputSystem;

// プレイヤーをA/Dキーで左右に動かすスクリプト
public class PlayerMovement : MonoBehaviour
{
    // 調整値をまとめたPlayerParamsアセット（移動速度・チャージ中の倍率）
    [SerializeField] private PlayerParams param;

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
        if (param == null)
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

        // 今の移動速度を決める。チャージ中はゆっくり（倍率をかける）
        float speed = param.moveSpeed;
        if (swordCharge != null && swordCharge.IsCharging)
        {
            speed = param.moveSpeed * param.chargeMoveMultiplier;
        }

        // 入力に応じて左右に動かす
        transform.Translate(move * speed * Time.deltaTime, 0f, 0f);
    }
}