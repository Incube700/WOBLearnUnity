using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // скорость перемещения
    [SerializeField] private Rigidbody2D rb; // Rigidbody2D игрока
    [SerializeField] private Transform turret; // трансформ башни
    [SerializeField] private Transform muzzle; // точка вылета пули
    [SerializeField] private Bullet bulletPrefab; // префаб пули
    [SerializeField] private float fireCooldown = 0.3f; // задержка между выстрелами
    [SerializeField] private Health health; // здоровье игрока

    private float _nextFireTime; // время следующего выстрела
    private float _nextContactDamageTime; // таймер урона от контакта

    private void Update()
    {
        AimTurret(); // поворачиваем башню на курсор
        HandleFire(); // проверяем возможность выстрела
    }

    private void FixedUpdate()
    {
        Move(); // перемещаем игрока в физическом апдейте
    }

    private void Move()
    {
        Vector2 input = GameInput.Instance.Move; // считываем оси движения
        rb.velocity = input.normalized * moveSpeed; // задаём скорость без ускорения
    }

    private void AimTurret()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
            GameInput.Instance.PointerScreen); // мировые координаты курсора
        Vector2 dir = mouseWorld - turret.position; // направление от башни к курсору
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // переводим направление в градусы
        turret.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // направляем башню на курсор
    }

    private void HandleFire()
    {
        if (!GameInput.Instance.FirePressed) return; // выходим, если кнопка не нажата
        if (Time.time < _nextFireTime) return; // проверяем кулдаун
        _nextFireTime = Time.time + fireCooldown; // устанавливаем время следующего выстрела
        Bullet bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation); // создаём пулю
        bullet.Init(muzzle.up); // задаём пуле направление по оси вверх башни
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<EnemyController>() == null) return; // урон только врагам
        if (Time.time < _nextContactDamageTime) return; // применяем урон не чаще раза в секунду
        _nextContactDamageTime = Time.time + 1f; // запоминаем время следующего тика урона
        collision.collider.GetComponent<Health>()?.ApplyDamage(1); // наносим 1 единицу урона
    }
}
