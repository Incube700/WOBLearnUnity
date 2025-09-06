using UnityEngine;

/// <summary>
/// Вспомогательный класс для настройки отображения брони в UI.
/// Создает необходимые UI элементы при запуске игры, если они отсутствуют.
/// </summary>
public class ArmorDisplaySetup : MonoBehaviour
{
    [SerializeField] private GameObject armorDisplayPrefab;
    
    private void Awake()
    {
        // Проверяем, существует ли уже ArmorDisplay в сцене
        if (FindObjectOfType<ArmorDisplay>() == null && armorDisplayPrefab != null)
        {
            // Создаем экземпляр префаба отображения брони
            Instantiate(armorDisplayPrefab, transform);
        }
    }
    
    /// <summary>
    /// Создает базовый UI для отображения брони, если префаб не назначен.
    /// </summary>
    public void CreateDefaultArmorDisplay()
    {
        if (FindObjectOfType<ArmorDisplay>() != null) return;
        
        // Создаем канвас, если его нет
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("ArmorUI");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Создаем панель для отображения брони
        GameObject armorPanel = new GameObject("ArmorPanel");
        armorPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = armorPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.01f, 0.01f);
        panelRect.anchorMax = new Vector2(0.2f, 0.2f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Добавляем компонент ArmorDisplay
        ArmorDisplay armorDisplay = armorPanel.AddComponent<ArmorDisplay>();
        
        // Создаем индикаторы брони
        CreateArmorIndicator("FrontArmorIndicator", armorPanel.transform, new Vector2(0.5f, 0.8f), ref armorDisplay.frontArmorIndicator);
        CreateArmorIndicator("SideArmorIndicator", armorPanel.transform, new Vector2(0.5f, 0.6f), ref armorDisplay.sideArmorIndicator);
        CreateArmorIndicator("RearArmorIndicator", armorPanel.transform, new Vector2(0.5f, 0.4f), ref armorDisplay.rearArmorIndicator);
        CreateArmorIndicator("TurretArmorIndicator", armorPanel.transform, new Vector2(0.5f, 0.2f), ref armorDisplay.turretArmorIndicator);
        
        Debug.Log("Создан базовый UI для отображения брони");
    }
    
    /// <summary>
    /// Создает отдельный индикатор брони.
    /// </summary>
    private void CreateArmorIndicator(string name, Transform parent, Vector2 anchorPosition, ref UnityEngine.UI.Image indicatorRef)
    {
        GameObject indicator = new GameObject(name);
        indicator.transform.SetParent(parent, false);
        
        RectTransform rect = indicator.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(anchorPosition.x - 0.4f, anchorPosition.y - 0.05f);
        rect.anchorMax = new Vector2(anchorPosition.x + 0.4f, anchorPosition.y + 0.05f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        UnityEngine.UI.Image image = indicator.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.green;
        image.type = UnityEngine.UI.Image.Type.Filled;
        image.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        image.fillOrigin = (int)UnityEngine.UI.Image.OriginHorizontal.Left;
        image.fillAmount = 1.0f;
        
        // Создаем текстовую метку
        GameObject label = new GameObject(name + "Label");
        label.transform.SetParent(indicator.transform, false);
        
        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        UnityEngine.UI.Text text = label.AddComponent<UnityEngine.UI.Text>();
        text.text = name.Replace("ArmorIndicator", "");
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        // Сохраняем ссылку на индикатор
        indicatorRef = image;
    }
}