using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для отображения UI улучшения брони.
/// </summary>
public class ArmorUpgradeUI : MonoBehaviour
{
    [Header("Ссылки на кнопки")]
    [SerializeField] private Button frontArmorButton;
    [SerializeField] private Button sideArmorButton;
    [SerializeField] private Button rearArmorButton;
    [SerializeField] private Button turretArmorButton;
    
    [Header("Текстовые поля")]
    [SerializeField] private Text currencyText;
    [SerializeField] private Text frontArmorLevelText;
    [SerializeField] private Text sideArmorLevelText;
    [SerializeField] private Text rearArmorLevelText;
    [SerializeField] private Text turretArmorLevelText;
    
    [Header("Текстовые поля стоимости")]
    [SerializeField] private Text frontArmorCostText;
    [SerializeField] private Text sideArmorCostText;
    [SerializeField] private Text rearArmorCostText;
    [SerializeField] private Text turretArmorCostText;
    
    private ArmorUpgrade armorUpgrade;
    
    private void Start()
    {
        // Находим компонент ArmorUpgrade
        armorUpgrade = FindObjectOfType<ArmorUpgrade>();
        
        if (armorUpgrade == null)
        {
            Debug.LogError("ArmorUpgradeUI: не найден компонент ArmorUpgrade", this);
            return;
        }
        
        // Настраиваем обработчики кнопок
        if (frontArmorButton != null)
            frontArmorButton.onClick.AddListener(UpgradeFrontArmor);
        
        if (sideArmorButton != null)
            sideArmorButton.onClick.AddListener(UpgradeSideArmor);
        
        if (rearArmorButton != null)
            rearArmorButton.onClick.AddListener(UpgradeRearArmor);
        
        if (turretArmorButton != null)
            turretArmorButton.onClick.AddListener(UpgradeTurretArmor);
        
        // Обновляем отображение
        UpdateUI();
    }
    
    private void Update()
    {
        // Обновляем UI каждый кадр для отображения актуальной информации
        UpdateUI();
    }
    
    /// <summary>
    /// Обновляет отображение UI.
    /// </summary>
    private void UpdateUI()
    {
        if (armorUpgrade == null) return;
        
        // Обновляем отображение валюты
        if (currencyText != null)
            currencyText.text = $"Очки: {armorUpgrade.GetCurrency()}";
        
        // Обновляем отображение уровней брони
        if (frontArmorLevelText != null)
            frontArmorLevelText.text = $"Уровень: {armorUpgrade.GetArmorLevel(ArmorUpgrade.ArmorType.Front)}";
        
        if (sideArmorLevelText != null)
            sideArmorLevelText.text = $"Уровень: {armorUpgrade.GetArmorLevel(ArmorUpgrade.ArmorType.Side)}";
        
        if (rearArmorLevelText != null)
            rearArmorLevelText.text = $"Уровень: {armorUpgrade.GetArmorLevel(ArmorUpgrade.ArmorType.Rear)}";
        
        if (turretArmorLevelText != null)
            turretArmorLevelText.text = $"Уровень: {armorUpgrade.GetArmorLevel(ArmorUpgrade.ArmorType.Turret)}";
        
        // Обновляем отображение стоимости улучшений
        if (frontArmorCostText != null)
            frontArmorCostText.text = $"Стоимость: {armorUpgrade.GetNextUpgradeCost(ArmorUpgrade.ArmorType.Front)}";
        
        if (sideArmorCostText != null)
            sideArmorCostText.text = $"Стоимость: {armorUpgrade.GetNextUpgradeCost(ArmorUpgrade.ArmorType.Side)}";
        
        if (rearArmorCostText != null)
            rearArmorCostText.text = $"Стоимость: {armorUpgrade.GetNextUpgradeCost(ArmorUpgrade.ArmorType.Rear)}";
        
        if (turretArmorCostText != null)
            turretArmorCostText.text = $"Стоимость: {armorUpgrade.GetNextUpgradeCost(ArmorUpgrade.ArmorType.Turret)}";
        
        // Обновляем доступность кнопок в зависимости от наличия валюты
        UpdateButtonsAvailability();
    }
    
    /// <summary>
    /// Обновляет доступность кнопок улучшения в зависимости от наличия валюты.
    /// </summary>
    private void UpdateButtonsAvailability()
    {
        if (armorUpgrade == null) return;
        
        int currency = armorUpgrade.GetCurrency();
        
        if (frontArmorButton != null)
            frontArmorButton.interactable = currency >= armorUpgrade.GetNextUpgradeCost(ArmorUpgrade.ArmorType.Front) && 
                                          armorUpgrade.GetArmorLevel(ArmorUpgrade.ArmorType.Front) < 5;
        
        if (sideArmorButton != null)
            sideArmorButton.interactable = currency >= armorUpgrade.GetNextUpgradeCost(ArmorUpgrade.ArmorType.Side) && 
                                         armorUpgrade.GetArmorLevel(ArmorUpgrade.ArmorType.Side) < 5;
        
        if (rearArmorButton != null)
            rearArmorButton.interactable = currency >= armorUpgrade.GetNextUpgradeCost(ArmorUpgrade.ArmorType.Rear) && 
                                         armorUpgrade.GetArmorLevel(ArmorUpgrade.ArmorType.Rear) < 5;
        
        if (turretArmorButton != null)
            turretArmorButton.interactable = currency >= armorUpgrade.GetNextUpgradeCost(ArmorUpgrade.ArmorType.Turret) && 
                                           armorUpgrade.GetArmorLevel(ArmorUpgrade.ArmorType.Turret) < 5;
    }
    
    /// <summary>
    /// Улучшает лобовую броню.
    /// </summary>
    private void UpgradeFrontArmor()
    {
        if (armorUpgrade != null)
            armorUpgrade.UpgradeFrontArmor();
    }
    
    /// <summary>
    /// Улучшает боковую броню.
    /// </summary>
    private void UpgradeSideArmor()
    {
        if (armorUpgrade != null)
            armorUpgrade.UpgradeSideArmor();
    }
    
    /// <summary>
    /// Улучшает заднюю броню.
    /// </summary>
    private void UpgradeRearArmor()
    {
        if (armorUpgrade != null)
            armorUpgrade.UpgradeRearArmor();
    }
    
    /// <summary>
    /// Улучшает броню башни.
    /// </summary>
    private void UpgradeTurretArmor()
    {
        if (armorUpgrade != null)
            armorUpgrade.UpgradeTurretArmor();
    }
    
    /// <summary>
    /// Создает базовый UI для улучшения брони.
    /// </summary>
    public void CreateDefaultUpgradeUI()
    {
        // Создаем канвас, если его нет
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("UpgradeUI");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Создаем панель для улучшений
        GameObject upgradePanel = new GameObject("UpgradePanel");
        upgradePanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = upgradePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.7f, 0.1f);
        panelRect.anchorMax = new Vector2(0.95f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Добавляем фон панели
        Image panelImage = upgradePanel.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Добавляем компонент ArmorUpgradeUI
        ArmorUpgradeUI upgradeUI = upgradePanel.AddComponent<ArmorUpgradeUI>();
        
        // Создаем заголовок
        CreateText("TitleText", upgradePanel.transform, new Vector2(0.5f, 0.95f), "Улучшение брони", 24);
        
        // Создаем отображение валюты
        upgradeUI.currencyText = CreateText("CurrencyText", upgradePanel.transform, new Vector2(0.5f, 0.9f), "Очки: 0", 18);
        
        // Создаем секции для каждого типа брони
        float yPos = 0.8f;
        float sectionHeight = 0.15f;
        
        // Лобовая броня
        CreateArmorUpgradeSection("FrontArmor", upgradePanel.transform, new Vector2(0.5f, yPos), "Лобовая броня", 
                                 out upgradeUI.frontArmorButton, out upgradeUI.frontArmorLevelText, out upgradeUI.frontArmorCostText);
        yPos -= sectionHeight;
        
        // Боковая броня
        CreateArmorUpgradeSection("SideArmor", upgradePanel.transform, new Vector2(0.5f, yPos), "Боковая броня", 
                                 out upgradeUI.sideArmorButton, out upgradeUI.sideArmorLevelText, out upgradeUI.sideArmorCostText);
        yPos -= sectionHeight;
        
        // Задняя броня
        CreateArmorUpgradeSection("RearArmor", upgradePanel.transform, new Vector2(0.5f, yPos), "Задняя броня", 
                                 out upgradeUI.rearArmorButton, out upgradeUI.rearArmorLevelText, out upgradeUI.rearArmorCostText);
        yPos -= sectionHeight;
        
        // Броня башни
        CreateArmorUpgradeSection("TurretArmor", upgradePanel.transform, new Vector2(0.5f, yPos), "Броня башни", 
                                 out upgradeUI.turretArmorButton, out upgradeUI.turretArmorLevelText, out upgradeUI.turretArmorCostText);
        
        Debug.Log("Создан базовый UI для улучшения брони");
    }
    
    /// <summary>
    /// Создает секцию для улучшения определенного типа брони.
    /// </summary>
    private void CreateArmorUpgradeSection(string name, Transform parent, Vector2 position, string title, 
                                         out Button upgradeButton, out Text levelText, out Text costText)
    {
        // Создаем контейнер секции
        GameObject section = new GameObject(name + "Section");
        section.transform.SetParent(parent, false);
        RectTransform sectionRect = section.AddComponent<RectTransform>();
        sectionRect.anchorMin = new Vector2(0.1f, position.y - 0.07f);
        sectionRect.anchorMax = new Vector2(0.9f, position.y + 0.07f);
        sectionRect.offsetMin = Vector2.zero;
        sectionRect.offsetMax = Vector2.zero;
        
        // Создаем заголовок секции
        Text titleText = CreateText(name + "Title", section.transform, new Vector2(0.5f, 0.8f), title, 16);
        titleText.alignment = TextAnchor.MiddleCenter;
        
        // Создаем текст с уровнем
        levelText = CreateText(name + "Level", section.transform, new Vector2(0.25f, 0.4f), "Уровень: 0", 14);
        levelText.alignment = TextAnchor.MiddleLeft;
        
        // Создаем текст со стоимостью
        costText = CreateText(name + "Cost", section.transform, new Vector2(0.25f, 0.1f), "Стоимость: 0", 14);
        costText.alignment = TextAnchor.MiddleLeft;
        
        // Создаем кнопку улучшения
        GameObject buttonObj = new GameObject(name + "Button");
        buttonObj.transform.SetParent(section.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.7f, 0.2f);
        buttonRect.anchorMax = new Vector2(0.9f, 0.8f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Добавляем компоненты кнопки
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.7f, 0.3f);
        upgradeButton = buttonObj.AddComponent<Button>();
        upgradeButton.targetGraphic = buttonImage;
        ColorBlock colors = upgradeButton.colors;
        colors.highlightedColor = new Color(0.4f, 0.8f, 0.4f);
        colors.pressedColor = new Color(0.2f, 0.6f, 0.2f);
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f);
        upgradeButton.colors = colors;
        
        // Добавляем текст на кнопку
        Text buttonText = CreateText(name + "ButtonText", buttonObj.transform, new Vector2(0.5f, 0.5f), "Улучшить", 14);
        buttonText.alignment = TextAnchor.MiddleCenter;
    }
    
    /// <summary>
    /// Создает текстовый элемент UI.
    /// </summary>
    private Text CreateText(string name, Transform parent, Vector2 position, string content, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(position.x - 0.4f, position.y - 0.05f);
        rect.anchorMax = new Vector2(position.x + 0.4f, position.y + 0.05f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        return text;
    }
}