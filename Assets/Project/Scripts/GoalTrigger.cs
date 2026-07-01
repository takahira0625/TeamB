using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public SceneController sceneController;

    public ScreenFader screenFader;

    private bool isGoalReached = false;

    void Start() { }
    void Update() { }

    // プレイヤーがゴールのセンサーに触れた瞬間に動く処理
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. すでに一度ゴールしているなら、これ以降の処理はしない（多重遷移の防止）
        if (isGoalReached) return;

        // 2. ぶつかってきたオブジェクトのタグが「Player」かどうかを判定
        if (collision.CompareTag("Player"))
        {
            // ゴールフラグをtrueにしてロックする
            isGoalReached = true;
            Time.timeScale = 0f;

            // フェードアウト
            if (screenFader != null)
            {
                screenFader.FadeOut(() =>
                {
                    // 3. 配線されたSceneControllerの「LoadGoal」メソッドを呼び出してシーンを切り替える
                    if (sceneController != null)
                    {
                        sceneController.LoadGoal();
                    }
                });
            }

            else
            {
                if (sceneController != null)
                {
                    sceneController.LoadGoal();
                }
            }
        }
    }
}