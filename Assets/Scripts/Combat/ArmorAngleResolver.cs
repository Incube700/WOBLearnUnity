using UnityEngine;

/// <summary>
/// Статический класс для расчета пробития брони в зависимости от угла попадания.
/// </summary>
public static class ArmorAngleResolver
{
    /// <summary>
    /// Стандартный критический угол пробития (в градусах).
    /// Если угол попадания меньше этого значения, то броня пробивается.
    /// </summary>
    public const float DefaultCriticalAngle = 45f;
    
    /// <summary>
    /// Проверяет, может ли снаряд пробить броню под данным углом.
    /// </summary>
    /// <param name="impactAngle">Угол попадания в градусах (0 = прямое попадание, 90 = по касательной)</param>
    /// <param name="criticalAngle">Критический угол пробития брони в градусах</param>
    /// <returns>true, если броня пробита; false, если произошел рикошет</returns>
    public static bool CanPenetrate(float impactAngle, float criticalAngle)
    {
        return impactAngle <= criticalAngle;
    }
    
    /// <summary>
    /// Рассчитывает модификатор урона в зависимости от угла попадания.
    /// Чем ближе к прямому попаданию, тем больше урона.
    /// </summary>
    /// <param name="impactAngle">Угол попадания в градусах</param>
    /// <returns>Множитель урона от 0.5 до 1.5</returns>
    public static float GetDamageModifier(float impactAngle)
    {
        // Нормализуем угол от 0 до 90 градусов
        float normalizedAngle = Mathf.Clamp(impactAngle, 0f, 90f) / 90f;
        
        // Инвертируем: меньший угол = больший урон
        float invertedFactor = 1f - normalizedAngle;
        
        // Рассчитываем множитель от 0.5 до 1.5
        return 0.5f + invertedFactor;
    }
    
    /// <summary>
    /// Рассчитывает эффективную толщину брони в зависимости от угла попадания.
    /// </summary>
    /// <param name="baseArmor">Базовая толщина брони</param>
    /// <param name="impactAngle">Угол попадания в градусах</param>
    /// <returns>Эффективная толщина брони</returns>
    public static float GetEffectiveArmor(float baseArmor, float impactAngle)
    {
        // Нормализуем угол от 0 до 90 градусов
        float normalizedAngle = Mathf.Clamp(impactAngle, 0f, 90f);
        
        // Рассчитываем эффективную толщину по формуле: базовая / cos(угол)
        // При угле 0 градусов эффективная = базовая
        // При приближении к 90 градусам эффективная стремится к бесконечности
        float angleRad = normalizedAngle * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angleRad);
        
        // Избегаем деления на очень маленькие числа
        if (cosAngle < 0.1f) cosAngle = 0.1f;
        
        return baseArmor / cosAngle;
    }
}