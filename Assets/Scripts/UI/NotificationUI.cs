using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для отображения уведомлений в игре.
/// </summary>
public class NotificationUI : MonoBehaviour
{
    [Header("Настройки уведомлений")]
    [SerializeField] private float defaultDuration = 2.0f;
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float fadeOutTime = 0.5f;
    
    [Header("Настройки внешнего вида")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color importantColor = new Color(1f, 0.8f, 0.2f, 1f);
    [SerializeField] private int fontSize = 24;
    
    private Queue<NotificationItem> notificationQueue = new Queue<NotificationItem>();
    private bool isShowingNotification = false;
    private Canvas notificationCanvas;
    private RectTransform notificationPanel;
    
    private void Awake()
    {
        // Создаем канвас для уведомлений, если его нет
        CreateNotificationCanvas();
    }
    
    /// <summary>
    /// Создает канвас для отображения уведомлений.
    /// </summary>
    private void CreateNotificationCanvas()
    {
        // Проверяем, есть ли уже канвас для уведомлений
        if (notificationCanvas != null) return;
        
        // Создаем объект для канваса
        GameObject canvasObj = new GameObject("NotificationCanvas");
        canvasObj.transform.SetParent(transform);
        
        // Добавляем компоненты канваса
        notificationCanvas = canvasObj.AddComponent<Canvas>();
        notificationCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        notificationCanvas.sortingOrder = 100; // Высокий порядок сортировки, чтобы уведомления были поверх других UI
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Создаем панель для уведомлений
        GameObject panelObj = new GameObject("NotificationPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        // Настраиваем RectTransform для панели
        notificationPanel = panelObj.AddComponent<RectTransform>();
        notificationPanel.anchorMin = new Vector2(0.5f, 0.8f);
        notificationPanel.anchorMax = new Vector2(0.5f, 0.8f);
        notificationPanel.sizeDelta = new Vector2(600f, 80f);
        notificationPanel.anchoredPosition = Vector2.zero;
        
        // Добавляем фон для панели
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        
        // Создаем текстовый элемент для уведомлений
        GameObject textObj = new GameObject("NotificationText");
        textObj.transform.SetParent(panelObj.transform, false);
        
        // Настраиваем RectTransform для текста
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 5f);
        textRect.offsetMax = new Vector2(-10f, -5f);
        
        // Добавляем компонент Text
        Text notificationText = textObj.AddComponent<Text>();
        notificationText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        notificationText.fontSize = fontSize;
        notificationText.alignment = TextAnchor.MiddleCenter;
        notificationText.color = normalColor;
        notificationText.text = "";
        
        // Скрываем панель изначально
        panelObj.SetActive(false);
    }
    
    /// <summary>
    /// Показывает уведомление с указанным текстом.
    /// </summary>
    /// <param name="message">Текст уведомления</param>
    /// <param name="duration">Продолжительность отображения в секундах</param>
    /// <param name="isImportant">Является ли уведомление важным (выделяется цветом)</param>
    public void ShowNotification(string message, float duration = 0f, bool isImportant = false)
    {
        if (duration <= 0f) duration = defaultDuration;
        
        // Создаем новый элемент уведомления
        NotificationItem item = new NotificationItem
        {
            Message = message,
            Duration = duration,
            IsImportant = isImportant
        };
        
        // Добавляем уведомление в очередь
        notificationQueue.Enqueue(item);
        
        // Если в данный момент не отображается уведомление, начинаем показ
        if (!isShowingNotification)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }
    
    /// <summary>
    /// Обрабатывает очередь уведомлений.
    /// </summary>
    private IEnumerator ProcessNotificationQueue()
    {
        isShowingNotification = true;
        
        while (notificationQueue.Count > 0)
        {
            // Получаем следующее уведомление из очереди
            NotificationItem item = notificationQueue.Dequeue();
            
            // Показываем уведомление
            yield return StartCoroutine(ShowNotificationCoroutine(item));
            
            // Небольшая пауза между уведомлениями
            yield return new WaitForSeconds(0.2f);
        }
        
        isShowingNotification = false;
    }
    
    /// <summary>
    /// Корутина для отображения одного уведомления.
    /// </summary>
    private IEnumerator ShowNotificationCoroutine(NotificationItem item)
    {
        // Получаем компоненты панели и текста
        GameObject panelObj = notificationPanel.gameObject;
        Text notificationText = panelObj.GetComponentInChildren<Text>();
        Image panelImage = panelObj.GetComponent<Image>();
        
        // Настраиваем текст и цвет
        notificationText.text = item.Message;
        notificationText.color = item.IsImportant ? importantColor : normalColor;
        
        // Показываем панель
        panelObj.SetActive(true);
        
        // Анимация появления
        float startTime = Time.time;
        Color textStartColor = notificationText.color;
        textStartColor.a = 0f;
        notificationText.color = textStartColor;
        
        Color panelStartColor = panelImage.color;
        panelStartColor.a = 0f;
        panelImage.color = panelStartColor;
        
        // Плавное появление
        while (Time.time < startTime + fadeInTime)
        {
            float t = (Time.time - startTime) / fadeInTime;
            
            Color textColor = notificationText.color;
            textColor.a = t;
            notificationText.color = textColor;
            
            Color panelColor = panelImage.color;
            panelColor.a = t * 0.7f; // Максимальная прозрачность панели 0.7
            panelImage.color = panelColor;
            
            yield return null;
        }
        
        // Устанавливаем полную непрозрачность
        Color finalTextColor = notificationText.color;
        finalTextColor.a = 1f;
        notificationText.color = finalTextColor;
        
        Color finalPanelColor = panelImage.color;
        finalPanelColor.a = 0.7f;
        panelImage.color = finalPanelColor;
        
        // Ждем указанное время
        yield return new WaitForSeconds(item.Duration);
        
        // Анимация исчезновения
        startTime = Time.time;
        
        // Плавное исчезновение
        while (Time.time < startTime + fadeOutTime)
        {
            float t = 1f - ((Time.time - startTime) / fadeOutTime);
            
            Color textColor = notificationText.color;
            textColor.a = t;
            notificationText.color = textColor;
            
            Color panelColor = panelImage.color;
            panelColor.a = t * 0.7f;
            panelImage.color = panelColor;
            
            yield return null;
        }
        
        // Скрываем панель
        panelObj.SetActive(false);
    }
    
    /// <summary>
    /// Класс для хранения информации об уведомлении.
    /// </summary>
    private class NotificationItem
    {
        public string Message { get; set; }
        public float Duration { get; set; }
        public bool IsImportant { get; set; }
    }
}