using System.Collections;
using UnityEngine;

public class PlayerInvincibility : MonoBehaviour
{
    [SerializeField]
    private Health health;

    [SerializeField]
    private SpriteRenderer targetSpriteRenderer;

    // 調整値をまとめたPlayerParamsアセット（無敵時間・点滅間隔）
    [SerializeField]
    private PlayerParams param;

    public bool IsInvincible { get; private set; }

    private int previousHp;
    private Coroutine invincibleCoroutine;

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            previousHp = health.CurrentHp;
            health.OnHpChanged += HandleHpChanged;
        }
    }

    private void Start()
    {
        if (health != null)
        {
            previousHp = health.CurrentHp;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHpChanged -= HandleHpChanged;
        }

        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
            invincibleCoroutine = null;
        }

        IsInvincible = false;

        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.enabled = true;
        }
    }

    private void HandleHpChanged(int currentHp, int maxHp)
    {
        bool isDamaged = currentHp < previousHp;

        previousHp = currentHp;

        if (!isDamaged)
        {
            return;
        }

        if (currentHp <= 0)
        {
            return;
        }

        StartInvincibility();
    }

    private void StartInvincibility()
    {
        if (param == null)
        {
            return;
        }

        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
        }

        invincibleCoroutine = StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        IsInvincible = true;

        float elapsedTime = 0f;

        while (elapsedTime < param.invincibleDuration)
        {
            if (targetSpriteRenderer != null)
            {
                targetSpriteRenderer.enabled = !targetSpriteRenderer.enabled;
            }

            yield return new WaitForSeconds(param.invincibleBlinkInterval);

            elapsedTime += param.invincibleBlinkInterval;
        }

        IsInvincible = false;

        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.enabled = true;
        }

        invincibleCoroutine = null;
    }
}