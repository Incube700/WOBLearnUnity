using UnityEngine;

/// <summary>
/// Решает, пробьёт ли пуля броню по углу попадания.
/// </summary>
public static class ArmorAngleResolver
{
    public const float DefaultCriticalAngle = 45f;          // допустимый угол пробития (°)

    /// <summary>
    /// Возвращает true, если угол меньше критического и пуля пробивает броню.
    /// </summary>
    public static bool CanPenetrate(float impactAngle, float criticalAngle = DefaultCriticalAngle)
    {
        return impactAngle < criticalAngle;                 // проникаем при меньшем угле
    }
}
