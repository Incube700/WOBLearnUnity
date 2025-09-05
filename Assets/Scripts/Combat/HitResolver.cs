using UnityEngine;

/// <summary>
/// Статический класс для обработки логики попадания пули.
/// </summary>
public static class HitResolver
{
    /// <summary>
    /// Обрабатывает столкновение пули с объектом.
    /// </summary>
    /// <returns>Возвращает true, если цель была пробита и урон нанесён.</returns>
    public static bool Resolve(Collider2D hitCollider, Vector2 bulletVelocity, Vector2 surfaceNormal, float damage, float criticalAngle)
    {
        // Пытаемся найти на объекте компонент, который может принять урон
        if (!hitCollider.TryGetComponent<IDamageable>(out var damageable))
        {
            return false; // Объект не может получить урон, значит, будет рикошет
        }

        float impactAngle = MathAngles.ImpactAngle(bulletVelocity, surfaceNormal);

        if (ArmorAngleResolver.CanPenetrate(impactAngle, criticalAngle))
        {
            damageable.TakeDamage(damage);
            return true; // Броня пробита
        }
        return false; // Рикошет
    }
}