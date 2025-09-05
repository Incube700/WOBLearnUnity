using UnityEngine;

/// <summary>
/// Пуля с рикошетами, двигающаяся вручную через Raycast.
/// Без Rigidbody2D — чистая кинематика.
/// </summary>
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 25f;            // скорость полёта (м/с)
    [SerializeField] private float damage = 1f;            // урон при попадании
    [SerializeField] private int maxRicochets = 3;         // сколько раз можно отскочить
    [SerializeField] private float radius = 0.1f;          // радиус круга для проверки столкновений
    [SerializeField] private LayerMask hitMask = ~0;       // слои, по которым летит пуля
    [SerializeField] private float criticalAngle = ArmorAngleResolver.DefaultCriticalAngle; // угол пробития (°)

    private const int MaxRicochetIterations = 5;           // макс. итераций рикошета за кадр
    private const float SurfaceOffset = 0.001f;            // небольшой отступ от поверхности
    [SerializeField] private float spawnOffset = 0.1f;      // дополнительный вынос из дула при спауне

    private Vector2 direction;                             // текущее направление полёта
    private int ricochetCount;                             // число совершённых рикошетов
    private Collider2D ownerCollider;                       // коллайдер владельца (для игнора самопопаданий)

    [Header("Debug")]
    [SerializeField] private bool drawDebug = false;        // рисовать ли траекторию
    [SerializeField] private Color debugColor = Color.yellow;

    /// <summary>
    /// Запускаем пулю в заданном направлении.
    /// </summary>
    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;                        // нормализуем направление
        transform.up = direction;                          // поворачиваем визуал пули
        // небольшой вынос вперёд, чтобы не спауниться внутри дула/корпуса
        transform.position = (Vector2)transform.position + direction * (radius + SurfaceOffset + spawnOffset);
    }

    /// <summary>
    /// Запуск с указанием владельца для игнора самоколлизий.
    /// </summary>
    public void Launch(Vector2 dir, Collider2D owner)
    {
        ownerCollider = owner;
        Launch(dir);
    }

    private void FixedUpdate()
    {
        float distance = speed * Time.fixedDeltaTime;      // сколько метров пройти за кадр
        for (int i = 0; i < MaxRicochetIterations && distance > 0f; i++) // ограничиваем количество проверок в кадре
        {
            Vector2 start = transform.position;            // точка начала луча
            RaycastHit2D hit = Physics2D.CircleCast(start, radius, direction, distance, hitMask); // проверяем столкновение

            if (hit.collider == null)                      // столкновений нет
            {
                Vector2 end = start + direction * distance;
                if (drawDebug) Debug.DrawLine(start, end, debugColor, Time.fixedDeltaTime);
                transform.position = end; // двигаем пулю вперёд
                break;                                      // выходим из цикла
            }

            // Игнорируем попадания по владельцу (например, если дуло внутри коллайдера корпуса)
            if (ownerCollider != null && hit.collider == ownerCollider)
            {
                // Выносим пулю на поверхность и продолжаем трассировку без рикошета
                if (drawDebug) Debug.DrawLine(start, hit.point, Color.cyan, Time.fixedDeltaTime);
                transform.position = hit.point + hit.normal * (radius + SurfaceOffset);
                distance -= hit.distance;
                continue;
            }

            bool damaged = HitResolver.Resolve(hit.collider, direction * speed, hit.normal, damage, criticalAngle); // пытаемся нанести урон
            if (damaged)                                   // цель пробита
            {
                if (drawDebug) Debug.DrawRay(hit.point, hit.normal * 0.4f, Color.red, 0.1f);
                Destroy(gameObject);                       // уничтожаем пулю
                return;                                    // прекращаем обработку
            }

            if (ricochetCount >= maxRicochets)             // лимит рикошетов достигнут
            {
                Destroy(gameObject);                       // уничтожаем пулю
                return;                                    // выходим
            }

            ricochetCount++;                               // фиксируем факт рикошета
            direction = Vector2.Reflect(direction, hit.normal).normalized; // отражаем направление
            transform.up = direction;                      // обновляем ориентацию пули
            distance -= hit.distance;                      // остаток пути в этом кадре
            if (drawDebug)
            {
                Debug.DrawLine(start, hit.point, debugColor, Time.fixedDeltaTime);
                Debug.DrawRay(hit.point, hit.normal * 0.3f, Color.green, 0.1f);
            }
            transform.position = hit.point + hit.normal * (radius + SurfaceOffset); // выводим пулю из поверхности с отступом

            // Если столкновение произошло на месте (пуля застряла),
            // то после выталкивания прекращаем движение в этом кадре.
            if (hit.distance == 0)
            {
                break;
            }
        }
    }
}
