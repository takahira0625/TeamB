using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public SceneController sceneController;

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

            // 5. 渦の中心へ吸い込み、演出完了後にシーン遷移(=武田のフェードアウト)へ繋ぐ
            if (absorb != null)
            {
                absorb.AbsorbInto(transform.position, OnAbsorbComplete);
            }
            else
            {
                // 演出が見つからない場合は従来どおり即遷移（保険）
                OnAbsorbComplete();
            }
        }
    }

    // 吸い込み演出の完了通知を受けてシーンを切り替える（onCompleteの接続先）
    private void OnAbsorbComplete()
    {
        if (sceneController != null)
        {
            sceneController.LoadGoal();
        }
    }
}