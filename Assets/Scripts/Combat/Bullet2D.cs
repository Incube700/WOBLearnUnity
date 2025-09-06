using UnityEngine;

/// <summary>
/// Простая пуля с детерминированными рикошетами.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Bullet2D : MonoBehaviour
{
    [Header("Кинематика")]
    [SerializeField] private float speedMps = 100f;     // м/с
    [SerializeField] private float minSpeedMps = 10f;   // страховка
    [SerializeField] private LayerMask hitMask;         // слои для Raycast

    [Header("Снаряд")]
    [SerializeField] private ShellType2D shellType = ShellType2D.AP; // тип
    [SerializeField] private float caliberMM = 75f;     // калибр
    [SerializeField] private float damage = 160f;       // урон
    [SerializeField] private float penAt100m = 120f;    // пробитие
    [SerializeField] private float penLossPer100m = 15f; // потеря пробития

    [Header("Рикошеты")]
    [SerializeField] private int maxRicochets = 3;      // лимит рикошетов
    private const float RICOCHET_SPEED_KEEP = 1.0f;     // скорость не теряем

    [Header("Жизненный цикл")]
    [SerializeField] private float maxLifeTime = 8f;    // сек

    private Vector2 velocity;          // текущая скорость
    private int ricochetCount = 0;     // сколько рикошетов было
    private float lifeTimer = 0f;      // таймер жизни
    private float distanceTraveled = 0f; // пройденная дистанция
    private Vector2 prevPos;           // предыдущая позиция для линий

    private void Awake()
    {
        velocity = transform.up.normalized * speedMps; // начальная скорость вдоль up
        prevPos = transform.position; // стартовая точка траектории
    }

    private void FixedUpdate()
    {
        lifeTimer += Time.fixedDeltaTime; // обновляем таймер
        if (lifeTimer > maxLifeTime) { Destroy(gameObject); return; } // уничтожаем по таймеру

        Vector2 curPos = transform.position; // текущая позиция
        Vector2 step = velocity * Time.fixedDeltaTime; // смещение

        RaycastHit2D hit = Physics2D.Raycast(curPos, step.normalized, step.magnitude, hitMask); // ищем столкновение

        if (hit.collider != null)
        {
            Vector2 hitPoint = hit.point - step.normalized * 0.01f; // чуть отступаем
            Debug.DrawLine(curPos, hitPoint, Color.green, 2f); // зелёный путь до контакта
            DrawContactPoint(hitPoint); // красный крест в точке контакта
            transform.position = hitPoint; // перемещаем пулю в место удара

            Armor2D armor = hit.collider.GetComponentInParent<Armor2D>(); // пытаемся найти броню
            float armorMM = 30f; // дефолт для стен
            if (armor != null)
            {
                armorMM = armor.GetNominalArmorMM(hitPoint, velocity); // получаем номинал и сектор
            }

            ShellStats2D shell = new ShellStats2D
            {
                type = shellType,
                caliberMM = caliberMM,
                damage = damage,
                penAt100m = penAt100m,
                penLossPer100m = penLossPer100m,
                maxRicochets = maxRicochets,
                ricochetSpeedKeep = RICOCHET_SPEED_KEEP
            };

            ImpactInput2D inp = new ImpactInput2D
            {
                bulletVelocity = velocity,
                hitNormal = hit.normal,
                distanceTraveledM = distanceTraveled,
                armorNominalMM = armorMM,
                shell = shell,
                ricochetCountSoFar = ricochetCount
            };

            ImpactResult2D res = PenetrationSolver2D.Solve(inp); // расчёт пробития

            Debug.Log(
                $"[Bullet] Target={hit.collider.name} | Shell={shell.type} cal={shell.caliberMM}mm | " +
                $"Angle={res.angle:F1}°, NormAngle={res.anglePrime:F1}° | Thr={res.ricochetThreshold:F1}° | " +
                $"Over3x={res.overmatch3x} Over2xRelax={res.overmatch2xRelax} | " +
                $"ArmorNom={armorMM:F1}mm Eff={res.effectiveArmor:F1}mm | " +
                $"Pen@{distanceTraveled:F1}m={res.penetrationAtDistance:F1}mm | " +
                $"RicCnt={ricochetCount} | " +
                (res.ricochet ? "RICOCHET" : (res.penetrated ? $"PEN Dmg={res.damageApplied:F1}" : "NO PEN"))
            );

            if (armor != null) armor.ReportHitResult(res); // сообщаем броне исход

            var counter = hit.collider.GetComponentInParent<DebugHitCounter>();
            if (counter != null) counter.RegisterImpact(res); // учитываем статистику

            if (res.ricochet)
            {
                ricochetCount++; // увеличиваем счётчик
                if (ricochetCount > maxRicochets) { Destroy(gameObject); return; } // превышен лимит

                velocity = res.newVelocity; // отражённая скорость

                if (velocity.magnitude < minSpeedMps) { Destroy(gameObject); return; } // слишком медленно

                transform.position = hitPoint + res.newVelocity.normalized * 0.02f; // выводим пулю из коллайдера
                prevPos = transform.position; // начинаем новый сегмент
            }
            else
            {
                if (res.penetrated && res.damageApplied > 0f)
                {
                    var hp = hit.collider.GetComponentInParent<Hitpoints2D>();
                    if (hp != null) hp.ApplyDamage(res.damageApplied); // наносим урон
                }

                Destroy(gameObject); // пуля больше не нужна
                return;
            }
        }
        else
        {
            Vector2 nextPos = curPos + step; // новая позиция
            Debug.DrawLine(curPos, nextPos, Color.green, 2f); // рисуем путь без столкновения
            transform.position = nextPos; // перемещаем пулю
        }

        distanceTraveled += step.magnitude; // накапливаем дистанцию
        prevPos = transform.position; // обновляем предыдущую позицию

        if (velocity.sqrMagnitude > 1e-6f)
            transform.up = velocity.normalized; // ориентируем вдоль скорости
    }

    private void DrawContactPoint(Vector2 point)
    {
        float s = 0.1f; // размер креста
        Debug.DrawLine(point + Vector2.left * s, point + Vector2.right * s, Color.red, 2f); // горизонтальная линия
        Debug.DrawLine(point + Vector2.up * s, point + Vector2.down * s, Color.red, 2f); // вертикальная линия
    }
}
