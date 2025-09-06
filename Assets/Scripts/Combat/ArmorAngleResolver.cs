using UnityEngine;

/// <summary>
/// Статический класс для расчёта пробития брони по углу попадания.
/// </summary>
public static class ArmorAngleResolver
{
    /// <summary>
    /// Критический угол пробития в градусах.
    /// </summary>
    public const float DefaultCriticalAngle = 45f;

    /// <summary>
    /// Возвращает true, если снаряд пробивает броню.
    /// </summary>
    /// <param name="impactAngle">Угол между направлением пули и нормалью поверхности (°)</param>
    /// <param name="criticalAngle">Максимально допустимый угол для пробития (°)</param>
    /// <returns>True — броня пробита; false — рикошет.</returns>
    public static bool CanPenetrate(float impactAngle, float criticalAngle)
    {
        return impactAngle <= criticalAngle; // острый угол до порога — есть пробитие
    }
}

