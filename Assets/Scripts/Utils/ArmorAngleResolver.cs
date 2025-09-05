using UnityEngine;

/// <summary>
/// Решает, пробьёт ли пуля броню по углу попадания.
/// </summary>
public static class ArmorAngleResolver
{
    public const float DefaultCriticalAngle = 45f;          // минимальный угол для урона

    /// <summary>
    /// Возвращает true, если угол больше критического и можно наносить урон.
    /// </summary>
    public static bool CanPenetrate(float impactAngle, float criticalAngle = DefaultCriticalAngle)
    {
        return impactAngle > criticalAngle;                 // true, если угол превышает порог
    }
}
