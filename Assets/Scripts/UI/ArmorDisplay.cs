using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для визуального отображения состояния брони игрока.
/// </summary>
public class ArmorDisplay : MonoBehaviour
{
    [Header("Armor Indicators")]
    [SerializeField] private Image frontArmorIndicator;
    [SerializeField] private Image sideArmorIndicator;
    [SerializeField] private Image rearArmorIndicator;
    [SerializeField] private Image turretArmorIndicator;
    
    [Header("Colors")]
    [SerializeField] private Color fullArmorColor = Color.green;
    [SerializeField] private Color mediumArmorColor = Color.yellow;
    [SerializeField] private Color lowArmorColor = Color.red;
    
    [Header("Thresholds")]
    [SerializeField] private float mediumArmorThreshold = 0.6f; // Порог для среднего состояния брони (60%)
    [SerializeField] private float lowArmorThreshold = 0.3f;    // Порог для низкого состояния брони (30%)
    
    private Health targetHealth;
    
    private void Start()
    {
        // Находим компонент Health игрока
        targetHealth = FindObjectOfType<PlayerController>()?.GetComponent<Health>();
        
        if (targetHealth == null)
        {
            Debug.LogWarning("ArmorDisplay: не найден компонент Health игрока", this);
        }
        
        // Инициализируем отображение
        UpdateDisplay();
    }
    
    private void Update()
    {
        if (targetHealth != null)
        {
            UpdateDisplay();
        }
    }
    
    /// <summary>
    /// Обновляет визуальное отображение состояния брони.
    /// </summary>
    private void UpdateDisplay()
    {
        if (targetHealth == null) return;
        
        // Получаем текущие значения брони
        float frontArmor = targetHealth.FrontArmor;
        float sideArmor = targetHealth.SideArmor;
        float rearArmor = targetHealth.RearArmor;
        float turretArmor = targetHealth.TurretArmor;
        
        // Получаем максимальные значения брони
        float maxFrontArmor = targetHealth.MaxFrontArmor;
        float maxSideArmor = targetHealth.MaxSideArmor;
        float maxRearArmor = targetHealth.MaxRearArmor;
        float maxTurretArmor = targetHealth.MaxTurretArmor;
        
        // Обновляем индикаторы
        UpdateArmorIndicator(frontArmorIndicator, frontArmor, maxFrontArmor);
        UpdateArmorIndicator(sideArmorIndicator, sideArmor, maxSideArmor);
        UpdateArmorIndicator(rearArmorIndicator, rearArmor, maxRearArmor);
        UpdateArmorIndicator(turretArmorIndicator, turretArmor, maxTurretArmor);
    }
    
    /// <summary>
    /// Обновляет отдельный индикатор брони.
    /// </summary>
    private void UpdateArmorIndicator(Image indicator, float currentArmor, float maxArmor)
    {
        if (indicator == null) return;
        
        // Рассчитываем процент оставшейся брони
        float armorPercent = maxArmor > 0 ? currentArmor / maxArmor : 0;
        
        // Устанавливаем заполнение индикатора
        indicator.fillAmount = armorPercent;
        
        // Определяем цвет в зависимости от состояния
        if (armorPercent <= lowArmorThreshold)
        {
            indicator.color = lowArmorColor;
        }
        else if (armorPercent <= mediumArmorThreshold)
        {
            indicator.color = mediumArmorColor;
        }
        else
        {
            indicator.color = fullArmorColor;
        }
    }
    
    /// <summary>
    /// Устанавливает новый объект для отслеживания брони.
    /// </summary>
    public void SetTarget(Health newTarget)
    {
        targetHealth = newTarget;
        UpdateDisplay();
    }
}