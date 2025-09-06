using UnityEngine;

/// <summary>
/// Тип снаряда в двумерной системе пробития.
/// </summary>
public enum ShellType2D { AP, APCR, HEAT }

/// <summary>
/// Настройки снаряда.
/// </summary>
public struct ShellStats2D
{
    public ShellType2D type;        // тип снаряда
    public float caliberMM;         // калибр, мм
    public float damage;            // базовый урон
    public float penAt100m;         // пробитие на 100 м
    public float penLossPer100m;    // потеря пробития на 100 м

    public int maxRicochets;        // максимум рикошетов
    public float ricochetSpeedKeep; // множитель скорости при рикошете
}

/// <summary>
/// Данные для расчёта взаимодействия.
/// </summary>
public struct ImpactInput2D
{
    public Vector2 bulletVelocity;  // мировая скорость пули
    public Vector2 hitNormal;       // нормаль контакта
    public float distanceTraveledM; // пройденная дистанция
    public float armorNominalMM;    // номинальная толщина
    public ShellStats2D shell;      // параметры снаряда
    public int ricochetCountSoFar;  // количество рикошетов до этого
}

/// <summary>
/// Результат расчёта.
/// </summary>
public struct ImpactResult2D
{
    public bool ricochet;            // был ли рикошет
    public bool penetrated;          // пробитие
    public float damageApplied;      // нанесённый урон
    public Vector2 newVelocity;      // новая скорость после рикошета
    public float angle;              // исходный угол встречи
    public float anglePrime;         // после нормализации
    public float ricochetThreshold;  // порог рикошета
    public float effectiveArmor;     // приведённая толщина
    public float penetrationAtDistance; // пробитие на дистанции
    public bool overmatch3x;         // 3х-оверматч
    public bool overmatch2xRelax;    // 2х-оверматч
}

/// <summary>
/// Решает пробитие без рандома.
/// </summary>
public static class PenetrationSolver2D
{
    public static ImpactResult2D Solve(ImpactInput2D inp)
    {
        ImpactResult2D r = new ImpactResult2D
        {
            ricochet = false,
            penetrated = false,
            damageApplied = 0f,
            newVelocity = inp.bulletVelocity,

            angle = 0f,
            anglePrime = 0f,
            ricochetThreshold = 0f,
            effectiveArmor = 0f,
            penetrationAtDistance = 0f,
            overmatch3x = false,
            overmatch2xRelax = false
        };

        Vector2 v = inp.bulletVelocity; // текущая скорость
        if (v.sqrMagnitude < 1e-6f) return r; // нулевая скорость — мгновенный выход

        float angle = Vector2.Angle(-v, inp.hitNormal); // угол встречи 0..180
        r.angle = angle;

        float normDeg = GetNormalizationDegrees(inp.shell.type); // нормализация
        float ricochetThreshold = GetRicochetThreshold(inp.shell.type); // базовый порог
        r.ricochetThreshold = ricochetThreshold;

        float armor = Mathf.Max(1f, inp.armorNominalMM); // защита от деления на ноль
        float caliber = Mathf.Max(1f, inp.shell.caliberMM); // калибр
        bool noRicochetByOvermatch = caliber >= (3f * armor); // 3х-оверматч
        bool relaxThreshold = !noRicochetByOvermatch && caliber >= (2f * armor); // 2х-оверматч

        r.overmatch3x = noRicochetByOvermatch; // пишем флаги
        r.overmatch2xRelax = relaxThreshold;

        if (relaxThreshold)
        {
            if (inp.shell.type == ShellType2D.HEAT) ricochetThreshold -= 5f;   // 85 -> 80
            else                                    ricochetThreshold -= 10f;  // APCR:70->60, AP:45->35
            ricochetThreshold = Mathf.Clamp(ricochetThreshold, 20f, 89f);     // защита от крайностей
        }

        float anglePrime = Mathf.Max(0f, angle - normDeg); // учитываем нормализацию
        r.anglePrime = anglePrime;

        if (!noRicochetByOvermatch && anglePrime > ricochetThreshold)
        {
            r.ricochet = true; // рикошет
            Vector2 nV = Vector2.Reflect(v, inp.hitNormal); // отражаем скорость
            r.newVelocity = nV * Mathf.Max(1f, inp.shell.ricochetSpeedKeep); // скорость без потерь
            return r; // дальнейших расчётов нет
        }

        float angleRad = anglePrime * Mathf.Deg2Rad; // перевели в радианы
        float cosA = Mathf.Max(0.017452f, Mathf.Cos(angleRad)); // cos(89°) минимально
        float effectiveMM = armor / cosA; // приведённая толщина
        r.effectiveArmor = effectiveMM;

        float pen = ComputePenetrationAtDistance(inp.shell.penAt100m, inp.shell.penLossPer100m, inp.distanceTraveledM); // пробитие
        r.penetrationAtDistance = pen;

        if (pen >= effectiveMM)
        {
            r.penetrated = true; // пробили
            float ricMult = Mathf.Pow(0.8f, Mathf.Max(0, inp.ricochetCountSoFar)); // -20% урона за каждый рикошет
            float dmg = Mathf.Max(1f, inp.shell.damage * ricMult); // не даём урону стать отрицательным
            r.damageApplied = dmg; // записываем урон
        }
        else
        {
            r.penetrated = false; // непробитие
            r.damageApplied = 0f; // урона нет
        }

        return r;
    }

    private static float ComputePenetrationAtDistance(float p100, float lossPer100, float distM)
    {
        float loss = lossPer100 * (distM / 100f); // линейная потеря
        float p = Mathf.Max(0f, p100 - loss); // пробитие не опускаем ниже нуля
        return p;
    }

    private static float GetNormalizationDegrees(ShellType2D t)
    {
        switch (t)
        {
            case ShellType2D.AP:   return 5f;  // ББ
            case ShellType2D.APCR: return 2f;  // подкалиберный
            case ShellType2D.HEAT: return 0f;  // кумулятивный
            default: return 0f;
        }
    }

    private static float GetRicochetThreshold(ShellType2D t)
    {
        switch (t)
        {
            case ShellType2D.AP:   return 45f; // ББ до 45°
            case ShellType2D.APCR: return 70f; // подкалибер
            case ShellType2D.HEAT: return 85f; // кумулятив
            default: return 70f;
        }
    }
}
