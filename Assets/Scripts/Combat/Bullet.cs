using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 12f; // скорость пули
    [SerializeField] private int damage = 1; // урон за попадание
    [SerializeField] private int maxRicochets = 3; // количество возможных рикошетов

    private Vector2 _direction; // направление полёта
    private Rigidbody2D _rb; // ссылка на Rigidbody2D
    private int _ricochetCount; // число уже совершённых рикошетов

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
        float angle = Vector2.Angle(-_direction, normal); // угол между пулей и нормалью
        if (angle > 45f) // если угол больше 45°
        {
            collision.collider.GetComponent<Health>()?.ApplyDamage(damage); // наносим урон цели
            Destroy(gameObject); // уничтожаем пулю
            return; // выходим, чтобы не отражаться
        }

        _direction = Vector2.Reflect(_direction, normal); // отражаем направление от нормали
        _rb.velocity = _direction * speed; // применяем новую скорость
        _ricochetCount++; // увеличиваем число рикошетов
        if (_ricochetCount > maxRicochets) Destroy(gameObject); // удаляем пулю после лимита рикошетов
    }
}

