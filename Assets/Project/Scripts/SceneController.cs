using UnityEngine;
using UnityEngine.SceneManagement;  // シーン遷移を使うために追加

// ボタンが押されたらゲームのシーンに戻すスクリプト
public class SceneController : MonoBehaviour
{
    // ボタンから呼び出すための関数
    public void LoadGameScene()
    {
        // ゲーム本編のシーンを読み込む
        SceneManager.LoadScene("Stage1Scene");
    }
}
