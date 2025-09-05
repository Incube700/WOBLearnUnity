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
}
