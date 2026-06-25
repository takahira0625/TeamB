using UnityEngine;

/// <summary>
/// 無敵中だけ「PlayerHurtbox 層 × Enemy 層」の物理当たりを切る薄いアダプタ。
/// 単一責任 =「無敵すり抜けの物理切り替えだけ」。
/// 無敵かどうかの判断・点滅は PlayerInvincibility の責務で、ここは IsInvincible を読むだけ。
/// </summary>
public class PlayerInvincibilityPassthrough : MonoBehaviour
{
    [Tooltip("無敵状態の通知元。未設定なら同じGameObjectから自動取得")]
    [SerializeField] private PlayerInvincibility invincibility;

    // 切り替える対象レイヤー名（被弾判定用ハートボックスと敵）
    private const string PlayerHurtboxLayerName = "PlayerHurtbox";
    private const string EnemyLayerName = "Enemy";

    private int playerHurtboxLayer = -1;
    private int enemyLayer = -1;

    // 直前の無敵状態。変化した瞬間だけ切り替えるためのエッジ検出用
    private bool wasInvincible;

    private void Awake()
    {
        if (invincibility == null)
        {
            invincibility = GetComponent<PlayerInvincibility>();
        }
        if (invincibility == null)
        {
            Debug.LogWarning($"[PlayerInvincibilityPassthrough] {name} : PlayerInvincibility が見つかりません。", this);
        }

        // レイヤー番号を取得（レイヤー未作成だと -1 が返る）
        playerHurtboxLayer = LayerMask.NameToLayer(PlayerHurtboxLayerName);
        enemyLayer = LayerMask.NameToLayer(EnemyLayerName);

        if (playerHurtboxLayer < 0 || enemyLayer < 0)
        {
            Debug.LogWarning(
                $"[PlayerInvincibilityPassthrough] {name} : レイヤー '{PlayerHurtboxLayerName}' または '{EnemyLayerName}' が未作成です。",
                this);
        }
    }

    private void Update()
    {
        if (invincibility == null)
        {
            return;
        }

        bool isInvincible = invincibility.IsInvincible;

        // 値が変化した瞬間だけ切り替える（毎フレーム IgnoreLayerCollision を呼ばない）
        if (isInvincible != wasInvincible)
        {
            SetPassthrough(isInvincible);
            wasInvincible = isInvincible;
        }
    }

    // ignore=true：無敵中＝当たりを切る（すり抜け） / false：通常に戻す
    private void SetPassthrough(bool ignore)
    {
        if (playerHurtboxLayer < 0 || enemyLayer < 0)
        {
            return;
        }

        Physics2D.IgnoreLayerCollision(playerHurtboxLayer, enemyLayer, ignore);
    }

    private void OnDisable()
    {
        // IgnoreLayerCollision はグローバルな物理設定。
        // 無効化・破棄（リトライのシーン再読み込み含む）で必ず通常（当たりON）に戻す。
        SetPassthrough(false);
        wasInvincible = false;
    }
}
