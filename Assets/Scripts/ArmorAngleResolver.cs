using UnityEngine;

/// <summary>
/// Решает, пробьёт ли пуля броню по углу попадания.
/// </summary>
public static class ArmorAngleResolver
{
    public const float DefaultCriticalAngle = 45f; // критический угол (°)

    /// <summary>
    /// Возвращает true, если угол меньше критического и можно наносить урон.
    /// </summary>
    public static bool CanPenetrate(float impactAngle, float criticalAngle = DefaultCriticalAngle)
    {
        return impactAngle < criticalAngle; // пробитие при меньшем угле
    }
}
