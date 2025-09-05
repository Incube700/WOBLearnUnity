using UnityEngine;

/// <summary>
/// Решает, пробьёт ли пуля броню по углу попадания.
/// </summary>
public static class ArmorAngleResolver
{
    public const float DefaultCriticalAngle = 45f; // критический угол (°)

    /// <summary>
    /// Возвращает true, если угол больше критического и можно наносить урон.
    /// </summary>
    public static bool CanPenetrate(float impactAngle, float criticalAngle = DefaultCriticalAngle)
    {
<<<<<<< HEAD:Assets/Scripts/ArmorAngleResolver.cs
        return impactAngle < criticalAngle; // пробитие при меньшем угле
=======
        return impactAngle > criticalAngle;                 // true, если угол превышает порог
>>>>>>> 359937279a0e59718d3806e865f6e200d4fd2864:Assets/Scripts/Utils/ArmorAngleResolver.cs
    }
}
