using UnityEngine;
using UnityEngine.InputSystem;

// プレイヤーをA/Dキーで左右に動かすスクリプト
public class PlayerMovement : MonoBehaviour
{
    // 移動の速さ（publicにするとInspectorで変えられる）
    public float moveSpeed = 5f;

    // ゲーム中、毎フレーム呼ばれる関数
    void Update()
    {
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