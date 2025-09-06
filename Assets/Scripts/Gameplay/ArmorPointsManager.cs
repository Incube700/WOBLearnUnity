using UnityEngine;

/// <summary>
/// Компонент для управления очками улучшения брони.
/// Добавляет очки при уничтожении противников.
/// </summary>
public class ArmorPointsManager : MonoBehaviour
{
    [Header("Настройки очков")]
    [SerializeField] private int pointsPerEnemyKill = 10;
    [SerializeField] private int pointsPerBossKill = 50;
    [SerializeField] private int bonusPointsPerWave = 20;
    
    private ArmorUpgrade playerArmorUpgrade;
    
    private void Start()
    {
        // Находим компонент ArmorUpgrade игрока
        playerArmorUpgrade = FindObjectOfType<ArmorUpgrade>();
        
        if (playerArmorUpgrade == null)
        {
            Debug.LogError("ArmorPointsManager: не найден компонент ArmorUpgrade", this);
            return;
        }
        
        // Подписываемся на события уничтожения противников (Health на врагах)
        Health[] healthComponents = FindObjectsOfType<Health>();
        foreach (var h in healthComponents)
        {
            // Игнорируем игрока
            if (h.GetComponent<PlayerController>() != null) continue;
            h.OnDeath += OnEnemyKilled;
        }
        
        // Подписываемся на события завершения волны, если есть система волн
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnWaveCompleted += OnWaveCompleted;
        }
    }
    
    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        Health[] healthComponents = FindObjectsOfType<Health>();
        foreach (var h in healthComponents)
        {
            if (h.GetComponent<PlayerController>() != null) continue;
            h.OnDeath -= OnEnemyKilled;
        }
        
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnWaveCompleted -= OnWaveCompleted;
        }
    }
    
    /// <summary>
    /// Вызывается при уничтожении противника.
    /// </summary>
    private void OnEnemyKilled(Health enemy)
    {
        if (playerArmorUpgrade == null) return;
        
        // Проверяем, является ли противник боссом
        bool isBoss = enemy.CompareTag("Boss");
        
        // Добавляем очки в зависимости от типа противника
        int pointsToAdd = isBoss ? pointsPerBossKill : pointsPerEnemyKill;
        
        // Добавляем очки игроку
        playerArmorUpgrade.AddCurrency(pointsToAdd);
        
        // Показываем уведомление о полученных очках
        ShowPointsNotification(pointsToAdd);
    }
    
    /// <summary>
    /// Вызывается при завершении волны противников.
    /// </summary>
    private void OnWaveCompleted(int waveNumber)
    {
        if (playerArmorUpgrade == null) return;
        
        // Добавляем бонусные очки за завершение волны
        int bonusPoints = bonusPointsPerWave * waveNumber;
        playerArmorUpgrade.AddCurrency(bonusPoints);
        
        // Показываем уведомление о бонусных очках
        ShowPointsNotification(bonusPoints, true);
    }
    
    /// <summary>
    /// Показывает уведомление о полученных очках.
    /// </summary>
    private void ShowPointsNotification(int points, bool isBonus = false)
    {
        // Находим или создаем UI для уведомлений
        NotificationUI notificationUI = FindObjectOfType<NotificationUI>();
        
        if (notificationUI != null)
        {
            string message = isBonus 
                ? $"Бонус за волну: +{points} очков брони!" 
                : $"+{points} очков брони";
                
            notificationUI.ShowNotification(message, 2.0f);
        }
        else
        {
            // Если UI для уведомлений не найден, выводим сообщение в консоль
            Debug.Log(isBonus 
                ? $"Бонус за волну: +{points} очков брони!" 
                : $"+{points} очков брони");
        }
    }
    
    /// <summary>
    /// Добавляет очки улучшения брони вручную (для тестирования или событий).
    /// </summary>
    public void AddArmorPoints(int points)
    {
        if (playerArmorUpgrade == null) return;
        
        playerArmorUpgrade.AddCurrency(points);
        ShowPointsNotification(points);
    }
}
