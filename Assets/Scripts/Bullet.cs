
using UnityEngine;
using System.Runtime.CompilerServices;

/// <summary>
/// Полёт пули с рикошетами и расчётом угла брони.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 25f;             // скорость полёта (м/с)
    [SerializeField] private float damage = 1f;             // урон за попадание
    [SerializeField] private int maxRicochets = 3;          // сколько раз можно рикошетить
    [SerializeField, Range(0.1f, 1f)] private float ricochetDamping = 1f; // потеря скорости при рикошете (1 — без потерь)
    [SerializeField] private LayerMask ricochetMask = ~0;   // какие слои допускают рикошет (по умолчанию — все)
    [SerializeField] private float criticalAngle = ArmorAngleResolver.DefaultCriticalAngle; // порог пробития (°)
    [SerializeField, Range(0f, 0.5f)] private float minExitNormalFraction = 0.12f; // минимальная доля скорости от поверхности
    [SerializeField, Range(0.001f, 0.05f)] private float separation = 0.01f; // смещение от поверхности после удара

    private Rigidbody2D rb;                                 // кэш тела
    private int ricochetCount;                              // число рикошетов
    private Vector2 velocity;                               // текущая скорость (ручное управление)

    private readonly RaycastHit2D[] hits = new RaycastHit2D[8]; // буфер попаданий на шаге
    private ContactFilter2D castFilter;                         // фильтр для Cast

    [Header("Отладка")]
    [SerializeField] private bool debugDrawReflection = true;   // рисовать ли луч отражения
    [SerializeField] private float debugRayLength = 1.2f;       // длина луча
    [SerializeField] private float debugRayDuration = 0.2f;     // время жизни луча
    [SerializeField] private Color debugReflectionColor = new Color(0f, 1f, 1f, 1f); // цвет отражения

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();                   // получаем Rigidbody2D
        rb.gravityScale = 0f;                               // топ‑даун — без гравитации
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // точные столкновения
        rb.freezeRotation = true;                           // пуля не кувыркается от столкновений
        rb.isKinematic = true;                              // управляем вручную (через Cast + MovePosition)
        // Убираем трение и упругость, чтобы не было скольжения и «прыжка» физикой
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            var mat = new PhysicsMaterial2D("BulletRuntime") { friction = 0f, bounciness = 0f };
            col.sharedMaterial = mat;
        }

        // Подготовка фильтра: учитываем все слои, игнорируем триггеры
        castFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = Physics2D.DefaultRaycastLayers,
            useTriggers = false
        };
    }

    public void Launch(Vector2 direction)
    {
        velocity = direction.normalized * speed;               // задаём скорость пули
        if (velocity.sqrMagnitude > 0.000001f)
            transform.up = velocity.normalized;                // ориентируем визуал по полёту
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        if (velocity.sqrMagnitude < 0.000001f)
        {
            Destroy(gameObject);
            return;
        }

        float remaining = velocity.magnitude * dt;
        Vector2 dir = velocity.normalized;

        // Страховка от зацикливания в одном кадре
        const int MaxCollisionsPerStep = 3;
        for (int i = 0; i < MaxCollisionsPerStep && remaining > 0f; i++)
        {
            int hitCount = rb.Cast(dir, castFilter, hits, remaining);
            if (hitCount <= 0)
            {
                // свободный полёт на остаток
                rb.MovePosition(rb.position + dir * remaining);
                break;
            }

            // Берём самый близкий хит
            int closestIndex = 0;
            float closestDist = hits[0].distance;
            for (int h = 1; h < hitCount; h++)
            {
                if (hits[h].distance < closestDist)
                {
                    closestDist = hits[h].distance;
                    closestIndex = h;
                }
            }

            var hit = hits[closestIndex];
            Vector2 hitPoint = rb.position + dir * hit.distance;
            Vector2 normal = hit.normal;
            Collider2D other = hit.collider;

            // Перемещаемся к точке удара с небольшим выходом из контакта
            Vector2 postHitPos = hitPoint + normal * separation;
            rb.MovePosition(postHitPos);

            // Сначала пробуем нанести урон
            bool damaged = HitResolver.Resolve(other, velocity, normal, damage, criticalAngle);
            if (damaged)
            {
                Destroy(gameObject);
                return;
            }

            // Проверяем лимит рикошетов
            if (ricochetCount >= maxRicochets)
            {
                Destroy(gameObject);
                return;
            }

            // Разрешён ли рикошет по слою
            int hitLayer = other.gameObject.layer;
            if ((ricochetMask.value & (1 << hitLayer)) == 0)
            {
                Destroy(gameObject);
                return;
            }

            // Считаем отражение
            Vector2 reflected = Vector2.Reflect(velocity, normal);
            if (debugDrawReflection)
            {
                Debug.DrawLine(hitPoint, hitPoint + reflected.normalized * debugRayLength, debugReflectionColor, debugRayDuration);
            }
            float mag = reflected.magnitude;
            if (mag > 0.0001f)
            {
                Vector2 rdir = reflected / mag;
                float n = Vector2.Dot(rdir, -normal);
                float minN = minExitNormalFraction;
                if (n < minN)
                {
                    rdir = (rdir + (-normal) * (minN - n)).normalized;
                    reflected = rdir * mag;
                }
            }
            reflected *= ricochetDamping;

            velocity = reflected;
            dir = velocity.normalized;
            transform.up = dir;

            ricochetCount++;

            // Сколько пути осталось после удара (+ небольшой выход из контакта уже учли MovePosition)
            remaining -= hit.distance;
            remaining = Mathf.Max(0f, remaining);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ApproximatelyZero(float v) => Mathf.Abs(v) < 0.000001f;
}
