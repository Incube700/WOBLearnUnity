using UnityEngine;

/// <summary>
/// Управляет здоровьем объекта и реализует интерфейс IDamageable.
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 10f;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} получил {damage} урона. Осталось здоровья: {currentHealth}");

        if (currentHealth <= 0)
        {
            Destroy(gameObject); // Уничтожаем объект, когда здоровье закончилось
        }
    }
}