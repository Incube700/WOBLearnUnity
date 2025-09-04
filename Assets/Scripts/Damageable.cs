using UnityEngine;

/// <summary>
/// Простой компонент здоровья, который можно повредить.
/// </summary>
public class Damageable : MonoBehaviour
{
    [SerializeField] private float maxHealth = 1f;          // максимальное здоровье

    private float currentHealth;                            // текущее здоровье

    private void Awake()
    {
        currentHealth = maxHealth;                          // устанавливаем начальное здоровье
    }

    /// <summary>
    /// Применяет урон к объекту.
    /// </summary>
    public void ApplyDamage(float amount)
    {
        currentHealth -= amount;                            // вычитаем урон
        if (currentHealth <= 0f)                            // здоровье закончилось
            Destroy(gameObject);                            // уничтожаем объект
    }
}