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

            // 3. プレイヤーの吸い込み演出を取得（rootでも子コライダーでも拾えるようParentから探す）
            PlayerGoalAbsorb absorb = collision.GetComponentInParent<PlayerGoalAbsorb>();

            // 4. ゴールしたら頭上の体力バーを外す（吸い込み演出中も表示しない）。
            //    体力バーはプレイヤーの子(PF_HpBar)にあるので本体を起点に探して非表示にする。
            Transform playerRoot = absorb != null
                ? absorb.transform
                : (collision.attachedRigidbody != null ? collision.attachedRigidbody.transform : collision.transform);
            HpBar hpBar = playerRoot.GetComponentInChildren<HpBar>(true);
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(false);
            }

            // 5. 渦の中心へ吸い込み、完了後にフェードアウト→シーン遷移へ繋ぐ。
            //    ※ここでは timeScale を触らない。吸い込み(PlayerGoalAbsorb)は Time.deltaTime 駆動なので、
            //      先に timeScale=0 にすると吸い込みが止まって完了通知(onComplete)が来ず固まるため。
            if (absorb != null)
            {
                absorb.AbsorbInto(transform.position, OnAbsorbComplete);
            }
            else
            {
                // 演出が見つからない場合はそのままフェードへ（保険）
                OnAbsorbComplete();
            }
        }
    }

    // 吸い込み完了後：世界を止めてフェードアウトし、暗転しきったらシーン遷移する
    private void OnAbsorbComplete()
    {
        // フェード中に敵接触などで死んでゴールが無効化されるのを防ぐため、ここで世界を止める。
        // フェード(ScreenFader)は unscaledDeltaTime 駆動なので timeScale=0 でも進む。
        Time.timeScale = 0f;

        // フェードアウト完了でシーン遷移。フェードが無ければそのまま遷移する（保険）。
        if (screenFader != null)
        {
            screenFader.FadeOut(LoadGoalScene);
        }
        else
        {
            LoadGoalScene();
        }
    }

    // シーン遷移（SceneController.LoadGoal が冒頭で timeScale を1へ戻す）
    private void LoadGoalScene()
    {
        if (sceneController != null)
        {
            sceneController.LoadGoal();
        }
    }
}
