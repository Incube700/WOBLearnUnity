using UnityEngine;

/// <summary>
/// Обрабатывает попадание пули по цели и решает, был ли нанесён урон.
/// </summary>
public static class HitResolver
{
    /// <summary>
    /// Возвращает true, если урон нанесён и пулю следует уничтожить (использует угол по умолчанию).
    /// </summary>
    public static bool Resolve(Collider2D target, Vector2 velocity, Vector2 surfaceNormal, float damage)
    {
        return Resolve(target, velocity, surfaceNormal, damage, ArmorAngleResolver.DefaultCriticalAngle);
    }

    /// <summary>
    /// Возвращает true, если урон нанесён и пулю следует уничтожить (с заданным критическим углом).
    /// </summary>
    public static bool Resolve(Collider2D target, Vector2 velocity, Vector2 surfaceNormal, float damage, float criticalAngle)
    {
        float angle = MathAngles.ImpactAngle(velocity, surfaceNormal); // вычисляем угол удара
        bool canDamage = ArmorAngleResolver.CanPenetrate(angle, criticalAngle); // проверяем критический угол

        Damageable dmg = target.GetComponent<Damageable>();            // ищем компонент урона
        if (dmg != null && canDamage)                                  // есть цель и угол подходящий
        {
            dmg.ApplyDamage(damage);                                   // наносим урон
            return true;                                               // пулю нужно уничтожить
        }

        return false;                                                  // рикошет
    }
}
