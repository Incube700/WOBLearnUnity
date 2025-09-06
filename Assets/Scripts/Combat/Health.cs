using UnityEngine;

/// <summary>
/// Управляет здоровьем и бронированием объекта.
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] private float maxHealth = 10f; // максимальное количество здоровья
    private float currentHealth;                    // текущее здоровье

    [Header("Броня")]
    [SerializeField] private float frontArmor = 30f;  // толщина лобовой брони
    [SerializeField] private float sideArmor = 20f;   // толщина боковой брони
    [SerializeField] private float rearArmor = 10f;   // толщина задней брони
    [SerializeField] private float turretArmor = 20f; // толщина брони башни
    [SerializeField] private bool hasTurret = true;   // есть ли у танка башня

    public float CurrentHealth => currentHealth;     // свойство для доступа к здоровью

    private void Awake()
    {
        currentHealth = maxHealth;                   // при создании ставим полное здоровье
    }

    /// <summary>
    /// Возвращает значение брони в зависимости от направления попадания.
    /// </summary>
    /// <param name="hitDirection">Мировое направление попадания</param>
    /// <param name="isTurretHit">Флаг попадания в башню</param>
    public float GetArmorForDirection(Vector2 hitDirection, bool isTurretHit = false)
    {
        if (isTurretHit && hasTurret)                // если удар пришёлся в башню
            return turretArmor;                      // используем броню башни

        hitDirection.Normalize();                    // нормализуем вектор
        Vector2 localDir = transform.InverseTransformDirection(hitDirection); // переводим в локальные координаты

        float angle = Vector2.Angle(Vector2.up, localDir); // вычисляем угол относительно носа танка

        if (angle <= 45f)                            // сектор ±45° спереди
            return frontArmor;                       // лобовая броня
        if (angle >= 135f)                           // сектор ±45° сзади
            return rearArmor;                        // кормовая броня
        return sideArmor;                            // иначе — бортовая броня
    }

    /// <summary>
    /// Базовое получение урона без учёта брони.
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;                     // уменьшаем здоровье
        Debug.Log($"{gameObject.name} получил {damage} урона. Осталось здоровья: {currentHealth}"); // выводим лог

        if (currentHealth <= 0f)                     // проверяем смерть
            Destroy(gameObject);                     // уничтожаем объект при нуле здоровья
    }

    public delegate void DamageTakenHandler(float damage); // делегат события получения урона
    public event DamageTakenHandler OnDamageTaken;          // событие для внешних подписчиков

    /// <summary>
    /// Получение урона с учётом направления и брони.
    /// </summary>
    /// <param name="damage">Базовый урон</param>
    /// <param name="hitDirection">Направление попадания</param>
    /// <param name="isTurretHit">Попадание в башню</param>
    /// <param name="impactAngle">Угол между пулей и нормалью</param>
    public void TakeDamageAdvanced(float damage, Vector2 hitDirection, bool isTurretHit, float impactAngle)
    {
        float armorValue = GetArmorForDirection(hitDirection, isTurretHit); // определяем броню по стороне

        float finalDamage = Mathf.Max(0f, damage - armorValue);             // броня поглощает часть урона

        TakeDamage(finalDamage);                                            // наносим остаток урона здоровью
        OnDamageTaken?.Invoke(finalDamage);                                 // оповещаем подписчиков

        Debug.Log($"Попадание: угол {impactAngle:F1}°, броня {armorValue:F1}, урон {damage:F1} → {finalDamage:F1}"); // подробный лог
    }
}

