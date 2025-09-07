using UnityEngine;

/// <summary>
/// Маркер для коллайдеров, которые ДОЛЖНЫ получать урон.
/// Повесьте на коллайдер корпуса. Башне этот компонент не добавляйте.
/// </summary>
public class DamageReceiver2D : MonoBehaviour
{
    [Tooltip("Если не задано, будет взят ближайший Hitpoints2D у родителей.")]
    public Hitpoints2D targetHP;

    /// <summary>
    /// Применяет урон к назначенной цели.
    /// </summary>
    public void ApplyDamage(float dmg)
    {
        if (dmg <= 0f) return;
        var hp = targetHP != null ? targetHP : GetComponentInParent<Hitpoints2D>();
        if (hp != null)
        {
            hp.ApplyDamage(dmg);
            return;
        }
        // Совместимость: если проект ещё использует старый Health, пробуем его
        var legacy = GetComponentInParent<Health>();
        if (legacy != null)
        {
            legacy.TakeDamage(dmg);
        }
    }

    private void Reset()
    {
        if (targetHP == null)
            targetHP = GetComponentInParent<Hitpoints2D>();
    }
}
