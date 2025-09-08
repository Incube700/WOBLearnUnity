using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 12f; // постоянная скорость пули
    [SerializeField] private int damage = 1; // урон за попадание
    [SerializeField] private int maxRicochets = 3; // количество допустимых рикошетов
    [SerializeField] private float armorAngle = 45f; // пробиваем, если угол меньше или равен

    private Vector2 _direction; // текущее направление полёта
    private Rigidbody2D _rb; // ссылка на Rigidbody2D
    private int _ricochetCount; // число совершённых рикошетов

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>(); // кэшируем Rigidbody2D
    }

    public void Init(Vector2 direction)
    {
        _direction = direction.normalized; // нормализуем направление
        _rb.velocity = _direction * speed; // задаём начальную скорость
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 normal = collision.GetContact(0).normal; // нормаль поверхности в точке удара
        float angle = Vector2.Angle(-_direction, normal); // угол между направлением и нормалью
        if (angle <= armorAngle) // прямой угол — пробиваем броню
        {
            collision.collider.GetComponent<Health>()?.ApplyDamage(damage); // наносим урон цели
            Destroy(gameObject); // уничтожаем пулю
            return; // выходим, чтобы не отражаться
        }

        if (_ricochetCount >= maxRicochets) // если уже было достаточно рикошетов
        {
            Destroy(gameObject); // уничтожаем пулю без отражения
            return; // выходим из метода
        }

        _direction = Vector2.Reflect(_direction, normal); // отражаем направление от нормали
        _rb.velocity = _direction * speed; // сохраняем прежнюю скорость после рикошета
        _ricochetCount++; // увеличиваем счётчик рикошетов
    }
}
