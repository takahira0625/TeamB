using System.Collections;
using UnityEngine;

public class GameOverController2 : MonoBehaviour
{
    [SerializeField] private Health health;

    [SerializeField] private SceneController sceneController;

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private SwordCharge swordCharge;

    [SerializeField] private float delaySeconds = 0.7f;

    // 二重遷移の保険（OnDiedは1回だけだが念のため）
    private bool isGameOver;

    private void Awake()
    {
        // 別オブジェクトに付ける想定なので基本はInspector配線。保険として自動取得も試みる
        if (health == null)
        {
            health = GetComponent<Health>();
        }
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        if (swordCharge == null)
        {
            swordCharge = GetComponentInChildren<SwordCharge>();
        }

        // プレイヤーの死亡通知を購読（死亡処理の起点）
        if (health != null)
        {
            health.OnDied += HandleDied;
        }
    }

    private void OnDestroy()
    {
        // リトライの再読み込みで破棄済み参照を呼ばないよう必ず解除
        if (health != null)
        {
            health.OnDied -= HandleDied;
        }
    }

    // Health.OnDied から呼ばれる死亡処理の本体
    private void HandleDied()
    {
        if (isGameOver)
        {
            return;
        }
        isGameOver = true;

        // プレイヤーの操作を止める
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        if (swordCharge != null)
        {
            swordCharge.enabled = false;
        }

        // 敵の動きを止める
        EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.enabled = false;
        }

        StartCoroutine(GoToGameOver());
    }

    private IEnumerator GoToGameOver()
    {
        // 実時間で待つ。万一ヒットストップでtimeScaleが0付近でも待ち時間が伸びない
        yield return new WaitForSecondsRealtime(delaySeconds);

        // timeScaleのリセットはSceneControllerが冒頭で行う（戻し忘れを構造的に防ぐ）
        if (sceneController != null)
        {
            sceneController.LoadGameOver();
        }
    }
}