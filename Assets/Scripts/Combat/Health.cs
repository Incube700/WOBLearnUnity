using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5; // начальное здоровье
    private int _current; // текущее здоровье

    private void Awake()
    {
        _current = maxHealth; // устанавливаем здоровье при запуске
    }

    public void ApplyDamage(int amount)
    {
        _current -= amount; // уменьшаем здоровье на величину урона
        if (_current <= 0) // проверяем, не умер ли объект
        {
            Destroy(gameObject); // уничтожаем объект при смерти
        }
    }
}

