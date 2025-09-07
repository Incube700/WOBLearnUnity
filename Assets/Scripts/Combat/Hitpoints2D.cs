using UnityEngine;

/// <summary>
/// Простое здоровье для 2D-объекта.
/// </summary>
public class Hitpoints2D : MonoBehaviour
{
    [SerializeField] private float maxHP = 1000f; // максимальное здоровье
    private float hp; // текущее здоровье

    public event System.Action<Hitpoints2D> OnDeath; // событие смерти для систем очков/логики

    private void Awake() { hp = maxHP; } // при старте устанавливаем здоровье

    public void ApplyDamage(float dmg)
    {
        hp -= dmg; // снимаем урон
        if (hp <= 0f) Die(); // проверяем смерть
    }

    private void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject); // уничтожаем объект
    }
}
