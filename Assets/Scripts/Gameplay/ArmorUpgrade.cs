using UnityEngine;

/// <summary>
/// Компонент для управления улучшениями брони игрока.
/// </summary>
public class ArmorUpgrade : MonoBehaviour
{
    [Header("Настройки улучшений")]
    [SerializeField] private float frontArmorUpgradeAmount = 1.0f;
    [SerializeField] private float sideArmorUpgradeAmount = 0.7f;
    [SerializeField] private float rearArmorUpgradeAmount = 0.5f;
    [SerializeField] private float turretArmorUpgradeAmount = 0.8f;
    
    [Header("Стоимость улучшений")]
    [SerializeField] private int frontArmorUpgradeCost = 100;
    [SerializeField] private int sideArmorUpgradeCost = 75;
    [SerializeField] private int rearArmorUpgradeCost = 50;
    [SerializeField] private int turretArmorUpgradeCost = 80;
    
    [Header("Максимальные уровни")]
    [SerializeField] private int maxUpgradeLevel = 5;
    
    // Текущие уровни улучшений
    private int frontArmorLevel = 0;
    private int sideArmorLevel = 0;
    private int rearArmorLevel = 0;
    private int turretArmorLevel = 0;
    
    // Ссылки на броню/здоровье игрока
    private Armor2D playerArmor2D;
    private Health playerHealth; // для совместимости UI
    
    // Игровая валюта (очки, деньги и т.д.)
    private int currency = 0;
    
    private void Start()
    {
        // Находим компоненты игрока
        playerArmor2D = GetComponent<Armor2D>();
        playerHealth = GetComponent<Health>();
        if (playerArmor2D == null && playerHealth == null)
        {
            Debug.LogError("ArmorUpgrade: не найдены Armor2D/Health на объекте", this);
        }
    }
    
    /// <summary>
    /// Добавляет игровую валюту.
    /// </summary>
    public void AddCurrency(int amount)
    {
        currency += amount;
        Debug.Log($"Получено {amount} очков. Всего: {currency}");
    }
    
    /// <summary>
    /// Возвращает текущее количество валюты.
    /// </summary>
    public int GetCurrency()
    {
        return currency;
    }
    
    /// <summary>
    /// Улучшает лобовую броню.
    /// </summary>
    /// <returns>true, если улучшение выполнено успешно</returns>
    public bool UpgradeFrontArmor()
    {
        return UpgradeArmor(ref frontArmorLevel, frontArmorUpgradeCost, frontArmorUpgradeAmount, ArmorType.Front);
    }
    
    /// <summary>
    /// Улучшает боковую броню.
    /// </summary>
    /// <returns>true, если улучшение выполнено успешно</returns>
    public bool UpgradeSideArmor()
    {
        return UpgradeArmor(ref sideArmorLevel, sideArmorUpgradeCost, sideArmorUpgradeAmount, ArmorType.Side);
    }
    
    /// <summary>
    /// Улучшает заднюю броню.
    /// </summary>
    /// <returns>true, если улучшение выполнено успешно</returns>
    public bool UpgradeRearArmor()
    {
        return UpgradeArmor(ref rearArmorLevel, rearArmorUpgradeCost, rearArmorUpgradeAmount, ArmorType.Rear);
    }
    
    /// <summary>
    /// Улучшает броню башни.
    /// </summary>
    /// <returns>true, если улучшение выполнено успешно</returns>
    public bool UpgradeTurretArmor()
    {
        return UpgradeArmor(ref turretArmorLevel, turretArmorUpgradeCost, turretArmorUpgradeAmount, ArmorType.Turret);
    }
    
    /// <summary>
    /// Общий метод для улучшения брони определенного типа.
    /// </summary>
    private bool UpgradeArmor(ref int level, int cost, float amount, ArmorType type)
    {
        if (playerArmor2D == null && playerHealth == null) return false;
        
        // Проверяем, не достигнут ли максимальный уровень
        if (level >= maxUpgradeLevel)
        {
            Debug.Log($"Достигнут максимальный уровень улучшения для {type}");
            return false;
        }
        
        // Проверяем, достаточно ли валюты
        int actualCost = cost * (level + 1); // Увеличиваем стоимость с каждым уровнем
        if (currency < actualCost)
        {
            Debug.Log($"Недостаточно очков для улучшения {type}. Нужно: {actualCost}, имеется: {currency}");
            return false;
        }
        
        // Списываем валюту
        currency -= actualCost;
        
        // Увеличиваем уровень
        level++;
        
        // Применяем улучшение к броне
        ApplyArmorUpgrade(type, amount);
        
        Debug.Log($"Улучшена броня {type} до уровня {level}. Потрачено {actualCost} очков.");
        return true;
    }
    
    /// <summary>
    /// Применяет улучшение к определенному типу брони.
    /// </summary>
    private void ApplyArmorUpgrade(ArmorType type, float amount)
    {
        // 1) Новая система: увеличиваем мм в Armor2D
        if (playerArmor2D != null)
        {
            switch (type)
            {
                case ArmorType.Front: playerArmor2D.AddArmor(ArmorArc2D.Front, amount); break;
                case ArmorType.Side:  playerArmor2D.AddArmor(ArmorArc2D.Side,  amount); break;
                case ArmorType.Rear:  playerArmor2D.AddArmor(ArmorArc2D.Rear,  amount); break;
                case ArmorType.Turret: /* не используется в 2D-солвере */ break;
            }
        }

        // 2) Совместимость со старым UI: обновляем Health, если присутствует
        if (playerHealth != null)
        {
            playerHealth.UpgradeArmor((int)type, amount);
        }
    }
    
    /// <summary>
    /// Типы брони для улучшения.
    /// </summary>
    public enum ArmorType
    {
        Front,
        Side,
        Rear,
        Turret
    }
    
    /// <summary>
    /// Возвращает текущий уровень улучшения для указанного типа брони.
    /// </summary>
    public int GetArmorLevel(ArmorType type)
    {
        switch (type)
        {
            case ArmorType.Front:
                return frontArmorLevel;
            case ArmorType.Side:
                return sideArmorLevel;
            case ArmorType.Rear:
                return rearArmorLevel;
            case ArmorType.Turret:
                return turretArmorLevel;
            default:
                return 0;
        }
    }
    
    /// <summary>
    /// Возвращает стоимость следующего улучшения для указанного типа брони.
    /// </summary>
    public int GetNextUpgradeCost(ArmorType type)
    {
        int level = GetArmorLevel(type);
        if (level >= maxUpgradeLevel) return 0; // Максимальный уровень достигнут
        
        switch (type)
        {
            case ArmorType.Front:
                return frontArmorUpgradeCost * (level + 1);
            case ArmorType.Side:
                return sideArmorUpgradeCost * (level + 1);
            case ArmorType.Rear:
                return rearArmorUpgradeCost * (level + 1);
            case ArmorType.Turret:
                return turretArmorUpgradeCost * (level + 1);
            default:
                return 0;
        }
    }
}
