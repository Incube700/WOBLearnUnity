using UnityEngine;

/// <summary>
/// Отвечает за стрельбу игрока.
/// Предполагается, что точка вылета пули ориентирована вдоль локальной оси Y.
/// </summary>
public class PlayerShooter : MonoBehaviour
{
    private static int lastShotFrame = -1;                  // защита от мультиспавна в один кадр
    [SerializeField] private Bullet bulletPrefab;           // префаб пули
    [SerializeField] private Transform muzzle;              // точка вылета
    [SerializeField] private float fireCooldown = 0.25f;    // время между выстрелами (с)

    private float cooldownTimer;                            // таймер перезарядки

    private void Update()
    {
        if (GameInput.Instance == null)                    // нет системы ввода
            return;                                        // выходим

        cooldownTimer -= Time.deltaTime;                   // уменьшаем таймер
        if (GameInput.Instance.Fire && cooldownTimer <= 0f)// нажата кнопка и готово стрелять
        {
            Shoot();                                       // выполняем выстрел
            cooldownTimer = fireCooldown;                  // сбрасываем таймер
        }
    }

    private void Shoot()
    {
        // Глобальная защита от дубликатов в этот же кадр (если компонент продублирован)
        if (Time.frameCount == lastShotFrame)
            return;
        if (bulletPrefab == null || muzzle == null)       // проверяем ссылки
        {
            Debug.LogError("PlayerShooter: отсутствует префаб пули или точка вылета", this);
            return;
        }

        Bullet bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation); // создаём пулю

        // Игнорируем столкновения пули с коллайдерами стрелка
        var shooterCols = GetComponentsInChildren<Collider2D>(true);
        var bulletCols = bullet.GetComponentsInChildren<Collider2D>(true);
        foreach (var sc in shooterCols)
            foreach (var bc in bulletCols)
                if (sc != null && bc != null)
                    Physics2D.IgnoreCollision(sc, bc, true);

        bullet.Launch(muzzle.up);                          // задаём направление полёта

        lastShotFrame = Time.frameCount;                   // фиксируем кадр выстрела
    }
}
