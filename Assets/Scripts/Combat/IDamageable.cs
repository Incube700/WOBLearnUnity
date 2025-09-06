using UnityEngine;                                      // подключаем базовые типы Unity

/// <summary>
/// Интерфейс для всех объектов, которые могут получать урон.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Применяет урон к объекту.
    /// </summary>
    /// <param name="damage">Количество нанесённого урона.</param>
    void TakeDamage(float damage);                        // вызывается при получении урона
    
    /// <summary>
    /// Расширенный метод получения урона с учетом направления попадания и брони.
    /// </summary>
    /// <param name="damage">Базовый урон</param>
    /// <param name="hitDirection">Направление попадания</param>
    /// <param name="isTurretHit">Попадание в башню</param>
    /// <param name="impactAngle">Угол попадания снаряда</param>
    void TakeDamageAdvanced(float damage, Vector2 hitDirection, bool isTurretHit, float impactAngle);
}
