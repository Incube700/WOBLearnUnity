using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f; // скорость врага
    [SerializeField] private float fireCooldown = 1f; // задержка между выстрелами
    [SerializeField] private Rigidbody2D rb; // Rigidbody2D врага
    [SerializeField] private Transform turret; // трансформ башни
    [SerializeField] private Transform muzzle; // точка вылета пули
    [SerializeField] private Bullet bulletPrefab; // префаб пули
    [SerializeField] private Health health; // здоровье врага

    private Transform _target; // ссылка на игрока
    private float _nextFireTime; // время следующего выстрела
    private float _nextContactDamageTime; // таймер контакта

    private void Start()
    {
        _target = FindObjectOfType<PlayerController>().transform; // ищем игрока в сцене
    }

    private void Update()
    {
        AimTurret(); // наводим башню на цель
        HandleFire(); // пытаемся стрелять
    }

    private void FixedUpdate()
    {
        Move(); // двигаемся к игроку в физическом апдейте
    }

    private void Move()
    {
        if (_target == null) return; // если игрока нет, ничего не делаем
        Vector2 dir = ((Vector2)_target.position - rb.position).normalized; // направление до игрока
        rb.velocity = dir * moveSpeed; // задаём скорость движения
    }

    private void AimTurret()
    {
        if (_target == null) return; // выходим, если игрок отсутствует
        Vector2 dir = (Vector2)_target.position - (Vector2)turret.position; // направление до игрока
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // вычисляем угол
        turret.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // поворачиваем башню
    }

    private void HandleFire()
    {
        if (_target == null) return; // нет цели — нет выстрела
        if (Time.time < _nextFireTime) return; // ещё не прошёл кулдаун
        _nextFireTime = Time.time + fireCooldown; // запоминаем время следующего выстрела
        Bullet bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation); // создаём пулю
        bullet.Init(muzzle.up); // задаём направление пули
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlayerController>() == null) return; // урон только игроку
        if (Time.time < _nextContactDamageTime) return; // наносим урон раз в секунду
        _nextContactDamageTime = Time.time + 1f; // запоминаем момент следующего урона
        collision.collider.GetComponent<Health>()?.ApplyDamage(1); // наносим 1 единицу урона
    }
}

