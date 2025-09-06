using UnityEngine;

/// <summary>
/// Управляет здоровьем объекта и реализует интерфейс IDamageable.
/// Включает систему брони с разных сторон объекта.
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] private float maxHealth = 10f;
    private float currentHealth;

    [Header("Броня")]
    [SerializeField] private float frontArmor = 3f;    // Лобовая броня
    [SerializeField] private float sideArmor = 2f;     // Боковая броня
    [SerializeField] private float rearArmor = 1f;     // Задняя броня
    [SerializeField] private float turretArmor = 2f;   // Броня башни (если есть)

    [Header("Настройки брони")]
    [SerializeField] private bool hasTurret = true;    // Есть ли башня
    [SerializeField] private float armorDamageReduction = 0.5f; // Коэффициент снижения урона при попадании в броню
    
    // Текущее состояние брони (может уменьшаться при повреждениях)
    private float currentFrontArmor;
    private float currentSideArmor;
    private float currentRearArmor;
    private float currentTurretArmor;
    
    // Публичные свойства для доступа к значениям брони
    public float FrontArmor => currentFrontArmor;
    public float SideArmor => currentSideArmor;
    public float RearArmor => currentRearArmor;
    public float TurretArmor => currentTurretArmor;
    
    // Публичные свойства для доступа к максимальным значениям брони
    public float MaxFrontArmor => frontArmor;
    public float MaxSideArmor => sideArmor;
    public float MaxRearArmor => rearArmor;
    public float MaxTurretArmor => turretArmor;
    
    // Публичное свойство для доступа к текущему здоровью
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        ResetArmor();
    }

    /// <summary>
    /// Сбрасывает состояние брони к начальным значениям.
    /// </summary>
    public void ResetArmor()
    {
        currentFrontArmor = frontArmor;
        currentSideArmor = sideArmor;
        currentRearArmor = rearArmor;
        currentTurretArmor = turretArmor;
    }
    
    /// <summary>
    /// Увеличивает максимальное значение брони указанного типа.
    /// </summary>
    /// <param name="type">Тип брони (0 - лобовая, 1 - боковая, 2 - задняя, 3 - башня)</param>
    /// <param name="amount">Величина увеличения</param>
    public void UpgradeArmor(int type, float amount)
    {
        switch (type)
        {
            case 0: // Front
                frontArmor += amount;
                break;
            case 1: // Side
                sideArmor += amount;
                break;
            case 2: // Rear
                rearArmor += amount;
                break;
            case 3: // Turret
                turretArmor += amount;
                break;
        }
        
        // Обновляем текущие значения брони
        ResetArmor();
    }

    /// <summary>
    /// Получает значение брони в зависимости от направления попадания.
    /// </summary>
    /// <param name="hitDirection">Направление попадания в локальных координатах объекта</param>
    /// <param name="isTurretHit">Попадание в башню</param>
    /// <returns>Значение брони для данного направления</returns>
    public float GetArmorForDirection(Vector2 hitDirection, bool isTurretHit = false)
    {
        if (isTurretHit && hasTurret)
            return currentTurretArmor;

        // Нормализуем направление
        hitDirection.Normalize();
        
        // Преобразуем мировые координаты в локальные относительно объекта
        Vector2 localHitDir = transform.InverseTransformDirection(hitDirection);
        
        // Определяем угол попадания относительно передней части объекта (оси Y)
        float angle = Vector2.Angle(Vector2.up, localHitDir);
        
        // Определяем сторону попадания
        if (angle <= 45f) // Фронтальное попадание (в пределах 45 градусов от носа)
            return currentFrontArmor;
        else if (angle >= 135f) // Попадание сзади (в пределах 45 градусов от кормы)
            return currentRearArmor;
        else // Боковое попадание
            return currentSideArmor;
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

    // Типы брони для событий
    public enum ArmorType
    {
        Front,
        Side,
        Rear,
        Turret
    }
    
    // События
    public delegate void ArmorHitHandler(ArmorType type, float damage);
    public delegate void DamageTakenHandler(float damage);
    
    // События для подписки
    public event ArmorHitHandler OnArmorHit;
    public event DamageTakenHandler OnDamageTaken;
    
    /// <summary>
    /// Расширенный метод получения урона с учетом направления попадания и брони.
    /// </summary>
    /// <param name="damage">Базовый урон</param>
    /// <param name="hitDirection">Направление попадания</param>
    /// <param name="isTurretHit">Попадание в башню</param>
    /// <param name="impactAngle">Угол попадания снаряда</param>
    public void TakeDamageAdvanced(float damage, Vector2 hitDirection, bool isTurretHit, float impactAngle)
    {
        // Получаем значение брони для данного направления
        float armorValue = GetArmorForDirection(hitDirection, isTurretHit);
        
        // Рассчитываем эффективную толщину брони с учетом угла попадания
        float effectiveArmor = ArmorAngleResolver.GetEffectiveArmor(armorValue, impactAngle);
        
        // Рассчитываем модификатор урона в зависимости от угла
        float damageModifier = ArmorAngleResolver.GetDamageModifier(impactAngle);
        
        // Показываем индикатор направления попадания, если он есть
        HitDirectionIndicator hitIndicator = GetComponent<HitDirectionIndicator>();
        if (hitIndicator != null)
        {
            // Преобразуем угол попадания в градусы для индикатора
            float hitAngle = Vector2.SignedAngle(Vector2.up, hitDirection);
            hitIndicator.ShowHitDirection(hitAngle, isTurretHit);
        }
        
        // Применяем модификатор к базовому урону
        float modifiedDamage = damage * damageModifier;
        
        // Уменьшаем урон в зависимости от брони
        float finalDamage = Mathf.Max(0.1f, modifiedDamage - (effectiveArmor * armorDamageReduction));
        
        // Наносим урон броне (она тоже повреждается)
        if (isTurretHit && hasTurret)
        {
            float armorDamage = modifiedDamage * 0.1f;
            currentTurretArmor = Mathf.Max(0f, currentTurretArmor - armorDamage);
            OnArmorHit?.Invoke(ArmorType.Turret, armorDamage);
        }
        else if (impactAngle <= 45f)
        {
            float armorDamage = modifiedDamage * 0.1f;
            currentFrontArmor = Mathf.Max(0f, currentFrontArmor - armorDamage);
            OnArmorHit?.Invoke(ArmorType.Front, armorDamage);
        }
        else if (impactAngle >= 135f)
        {
            float armorDamage = modifiedDamage * 0.1f;
            currentRearArmor = Mathf.Max(0f, currentRearArmor - armorDamage);
            OnArmorHit?.Invoke(ArmorType.Rear, armorDamage);
        }
        else
        {
            float armorDamage = modifiedDamage * 0.1f;
            currentSideArmor = Mathf.Max(0f, currentSideArmor - armorDamage);
            OnArmorHit?.Invoke(ArmorType.Side, armorDamage);
        }
        
        // Применяем итоговый урон
        TakeDamage(finalDamage);
        
        // Вызываем событие получения урона
        OnDamageTaken?.Invoke(finalDamage);
        
        Debug.Log($"Попадание: угол {impactAngle:F1}°, броня {armorValue:F1} (эфф. {effectiveArmor:F1}), урон {damage:F1} → {finalDamage:F1}");
    }
}