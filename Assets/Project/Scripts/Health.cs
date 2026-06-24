using System;
using UnityEngine;

/// <summary>
/// HP管理コンポーネント。単一責任=「HPの保持・増減・死亡通知」。
/// 死亡時の処理（消滅・ゲームオーバーなど）は持たず、OnDiedで外部へ委譲する。
/// PF_Enemy・PF_Player 両プレハブで共用する。
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHp = 100;

    public int CurrentHp { get; private set; }
    public int MaxHp => maxHp;
    public bool IsDead => CurrentHp <= 0;

    /// <summary>HPが変化したとき発火する。引数:(現在HP, 最大HP)</summary>
    public event Action<int, int> OnHpChanged;

    /// <summary>HPが0になった瞬間に1度だけ発火する。多段ヒットで再発火しない。</summary>
    public event Action OnDied;

    private void Awake()
    {
        CurrentHp = maxHp;
    }

    /// <summary>ダメージを受ける。HPは0未満にならない。死亡時はOnDiedを1度だけ発火する。</summary>
    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHp = Mathf.Clamp(CurrentHp - amount, 0, maxHp);
        Debug.Log($"[Health] {gameObject.name} HP: {CurrentHp}/{maxHp}");
        OnHpChanged?.Invoke(CurrentHp, maxHp);

        if (IsDead)
        {
            Debug.Log($"[Health] {gameObject.name} 死亡");
            OnDied?.Invoke();
        }
    }

    /// <summary>HPを回復する。maxHpを超えない。死亡後は呼び出しを無視する。</summary>
    public void Heal(int amount)
    {
        if (IsDead) return;

        CurrentHp = Mathf.Clamp(CurrentHp + amount, 0, maxHp);
        OnHpChanged?.Invoke(CurrentHp, maxHp);
    }
}
