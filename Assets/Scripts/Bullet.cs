
using UnityEngine;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();                   // получаем Rigidbody2D
        rb.gravityScale = 0f;                               // топ‑даун — без гравитации
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // точные столкновения
        rb.freezeRotation = true;                           // пуля не кувыркается от столкновений
        // Убираем трение и упругость, чтобы не было скольжения и «прыжка» физикой
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            var mat = new PhysicsMaterial2D("BulletRuntime") { friction = 0f, bounciness = 0f };
            col.sharedMaterial = mat;
        }
    }

    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;               // задаём скорость пули
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 normal = collision.contacts[0].normal;      // нормаль поверхности
        Vector2 velocity = rb.linearVelocity;                           // текущая скорость

        bool hit = HitResolver.Resolve(collision.collider, velocity, normal, damage, criticalAngle); // пробуем нанести урон
        if (hit)                                           // цель поражена
        {
            Destroy(gameObject);                            // уничтожаем пулю
            return;                                         // дальше не обрабатываем
        }

        // Семантика: разрешаем РОВНО maxRicochets отражений.
        // Если лимит уже исчерпан — уничтожаем пулю на этом столкновении.
        if (ricochetCount >= maxRicochets)                 // достигли лимита отражений ранее
        {
            Destroy(gameObject);                            // уничтожаем пулю без отражения
            return;                                         // прекращаем метод
        }

        // Если поверхность не в маске рикошета — просто уничтожаем пулю
        int hitLayer = collision.collider.gameObject.layer;
        if ((ricochetMask.value & (1 << hitLayer)) == 0)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 reflected = Vector2.Reflect(velocity, normal); // вычисляем отражённую скорость
        // Гарантируем, что есть минимальная составляющая от поверхности (не скользим вдоль стены)
        float mag = reflected.magnitude;
        if (mag > 0.0001f)
        {
            Vector2 dir = reflected / mag;
            float n = Vector2.Dot(dir, -normal); // компонента «от поверхности»
            float minN = minExitNormalFraction;
            if (n < minN)
            {
                dir = (dir + (-normal) * (minN - n)).normalized;
                reflected = dir * mag;
            }
        }
        reflected *= ricochetDamping;                        // теряем часть скорости при рикошете
        rb.linearVelocity = reflected;                               // задаём новую скорость
        transform.up = reflected.normalized;                // поворачиваем визуал пули

        // Небольшое смещение от поверхности, чтобы выйти из контакта и не «залипать»
        rb.position = rb.position + normal * separation;

        ricochetCount++;                                    // отражение состоялось — увеличиваем счётчик
    }
}
