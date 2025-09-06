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
    
    /// <summary>
    /// Расширенная версия обработки столкновения с учетом направления попадания и типа брони.
    /// </summary>
    /// <returns>Возвращает true, если цель была пробита и урон нанесён.</returns>
    public static bool ResolveAdvanced(Collider2D hitCollider, Vector2 bulletVelocity, Vector2 surfaceNormal, float damage, float criticalAngle, Transform shooterTransform)
    {
        // Пытаемся найти на объекте компонент, который может принять урон
        if (!hitCollider.TryGetComponent<IDamageable>(out var damageable))
        {
            return false; // Объект не может получить урон, значит, будет рикошет
        }
        
        // Определяем направление попадания относительно объекта
        Vector2 hitDirection = (hitCollider.transform.position - shooterTransform.position).normalized;
        
        // Определяем, попадание в башню или в корпус
        bool isTurretHit = false;
        if (hitCollider.gameObject.CompareTag("Turret"))
        {
            isTurretHit = true;
        }
        
        float impactAngle = MathAngles.ImpactAngle(bulletVelocity, surfaceNormal);
        
        if (ArmorAngleResolver.CanPenetrate(impactAngle, criticalAngle))
        {
            // Используем расширенный метод нанесения урона с учетом направления и угла попадания
            damageable.TakeDamageAdvanced(damage, hitDirection, isTurretHit, impactAngle);
            return true; // Броня пробита
        }
        return false; // Рикошет
    }
}