using System.Collections;
using UnityEngine;

/// <summary>
/// 敵の撃破演出。単一責任=「Health.OnDied通知を受けて、見た目を撃破絵に差し替えてから消滅させる」。
/// HPの増減・死亡判定そのものはHealthが担当するため、ここでは触らない。
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyDeath : MonoBehaviour
{
    [Header("参照（未設定なら自動取得）")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private EnemyContactDamage enemyContactDamage;
    [SerializeField] private Health health;
    // 移動アニメ用のAnimator。撃破時に止めないと毎フレーム歩行絵でSpriteを上書きし、撃破絵が表示されない
    [SerializeField] private Animator animator;

    [Header("撃破演出")]
    // 仮素材差し替え箇所：岡本納品前は色違いスプライト等の仮素材で代用可。未設定なら差し替えをスキップする
    [SerializeField] private Sprite deathSprite;
    [Tooltip("撃破絵を表示してから消滅するまでの秒数（目安0.1〜0.3）")]
    [SerializeField] private float displayDuration = 0.2f;
    [Tooltip("撃破時に自分の位置へInstantiateするエフェクト（任意）")]
    [SerializeField] private GameObject deathEffect;
    // 仮素材差し替え箇所：平岡納品前は仮音で代用可。未設定なら再生をスキップする
    [SerializeField] private AudioClip deathSe;

    // OnDiedの多重発火・DeathSequenceの多重起動を防ぐガード
    private bool isDying;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (enemyMovement == null) enemyMovement = GetComponent<EnemyMovement>();
        if (enemyContactDamage == null) enemyContactDamage = GetComponent<EnemyContactDamage>();
        if (health == null) health = GetComponent<Health>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        health.OnDied += OnEnemyDied;
    }

    private void OnDestroy()
    {
        health.OnDied -= OnEnemyDied;
    }

    private void OnEnemyDied()
    {
        if (isDying) return;
        isDying = true;

        // 差し替え表示中は「死体」：動かない・触れてもダメージを与えない状態にする
        // ※EnemyContactDamage側にも enabled を見るガードがあるため、ここで止めれば物理コールバックも無害化される
        if (enemyMovement != null) enemyMovement.enabled = false;
        if (enemyContactDamage != null) enemyContactDamage.enabled = false;
        // Animatorを止めないと歩行アニメがSpriteを上書きし続け、下のdeathSprite差し替えが見えない
        if (animator != null) animator.enabled = false;

        // OnDiedはTakeDamage（剣の当たり判定の流れ）の中で呼ばれるため、その場で即Destroyしない。
        // 差し替え→表示待ち→Destroyはコルーチンに乗せて次フレーム以降に回す。
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (spriteRenderer != null && deathSprite != null)
        {
            spriteRenderer.sprite = deathSprite;
        }

        if (deathEffect != null)
        {
            var effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, displayDuration);
        }

        if (deathSe != null)
        {
            // 自身はこの後Destroyされるため、PlayClipAtPointで再生専用の一時オブジェクトに任せて鳴らし切る
            AudioSource.PlayClipAtPoint(deathSe, transform.position);
        }

        yield return new WaitForSeconds(displayDuration);
        Destroy(gameObject);
    }
}
