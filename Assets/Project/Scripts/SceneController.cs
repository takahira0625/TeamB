using UnityEngine;
using UnityEngine.SceneManagement;  // シーン遷移を使うために追加

// ボタンが押されたらゲームのシーンに戻すスクリプト
public class SceneController : MonoBehaviour
{
    // ゲーム開始時に一度だけ呼び出される関数
    void Start()
    {

    }

    // ボタンから呼び出すための関数
    public void LoadGameScene()
    {
        // ゲーム本編のシーンを読み込む
        SceneManager.LoadScene("Stage1Scene");
    }
}
