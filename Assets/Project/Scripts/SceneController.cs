using UnityEngine;
using UnityEngine.SceneManagement;  // シーン遷移を使うために追加

// ボタンが押されたらゲームのシーンに戻すスクリプト
public class SceneController : MonoBehaviour
{
    // ボタンから呼び出すための関数
    public void LoadGameScene()
    {
        // ゲーム本編のシーンを読み込む
        Time.timeScale = 1;
        SceneManager.LoadScene("Stage1Scene");
    }

    public void LoadTitle()
    {
        // タイトルシーンを読み込む
        Time.timeScale = 1;
        SceneManager.LoadScene("TitleScene");
    }

    public void LoadGoal()
    {
        // ゴールシーンを読み込む
        Time.timeScale = 1;
        SceneManager.LoadScene("GoalScene");
    }

    public void LoadGameOver()
    {
        // ゲームオーバーシーンを読み込む
        Time.timeScale = 1;
        SceneManager.LoadScene("GameOverScene");
    }
}
