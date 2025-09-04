using UnityEngine;

/// <summary>
/// Вспомогательные функции для работы с углами.
/// </summary>
public static class MathAngles
{
    /// <summary>
    /// Возвращает угол между направлением пули и нормалью поверхности.
    /// </summary>
    public static float ImpactAngle(Vector2 bulletVelocity, Vector2 surfaceNormal)
    {
        return Vector2.Angle(-bulletVelocity.normalized, surfaceNormal.normalized); // угол атаки
    }
}
