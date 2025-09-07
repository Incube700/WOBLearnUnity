using UnityEngine;

/// <summary>
/// Простой скрипт стрельбы игрока.
/// </summary>
public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;   // префаб пули
    [SerializeField] private Transform muzzle;          // точка вылета
    [SerializeField] private float fireCooldown = 0.25f;// задержка между выстрелами

    private float cooldown;                             // таймер перезарядки

    private void Awake()
    {
        if (bulletPrefab == null)                       // если префаб не назначен в инспекторе
            bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullet"); // пробуем найти в Resources
    }

    private void Update()
    {
        cooldown -= Time.deltaTime;                     // уменьшаем таймер

        bool wantShoot = false;                         // флаг желания стрелять
        if (GameInput.Instance != null)                 // если есть централизованный ввод
            wantShoot = GameInput.Instance.Fire || GameInput.Instance.FireHeld; // читаем действие Fire
        else
            wantShoot = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space); // резервный ввод

        if (wantShoot && cooldown <= 0f)                // можно стрелять
        {
            Shoot();                                    // создаём пулю
            cooldown = fireCooldown;                    // сбрасываем таймер
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || muzzle == null)     // проверяем необходимые ссылки
        {
            Debug.LogError("PlayerShooter: назначьте bulletPrefab и muzzle", this); // выводим ошибку
            return;                                     // прекращаем выполнение
        }

        Instantiate(bulletPrefab, muzzle.position, muzzle.rotation); // создаём пулю в точке вылета
    }
}
