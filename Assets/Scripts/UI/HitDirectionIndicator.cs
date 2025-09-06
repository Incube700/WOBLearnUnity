using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для отображения индикатора направления попадания.
/// Показывает игроку, с какой стороны произошло попадание.
/// </summary>
public class HitDirectionIndicator : MonoBehaviour
{
    [Header("Настройки индикатора")]
    [SerializeField] private float indicatorDuration = 1.0f;
    [SerializeField] private float fadeOutTime = 0.5f;
    
    [Header("Настройки внешнего вида")]
    [SerializeField] private Color frontHitColor = new Color(1f, 0.2f, 0.2f, 0.7f);
    [SerializeField] private Color sideHitColor = new Color(1f, 0.6f, 0.2f, 0.7f);
    [SerializeField] private Color rearHitColor = new Color(1f, 0.2f, 0.2f, 0.7f);
    [SerializeField] private Color turretHitColor = new Color(0.2f, 0.6f, 1f, 0.7f);
    
    private Canvas indicatorCanvas;
    private Image frontIndicator;
    private Image leftIndicator;
    private Image rightIndicator;
    private Image rearIndicator;
    private Image turretIndicator;
    
    private void Awake()
    {
        // Создаем индикаторы направления попадания
        CreateHitDirectionIndicators();
    }
    
    /// <summary>
    /// Создает индикаторы направления попадания.
    /// </summary>
    private void CreateHitDirectionIndicators()
    {
        // Создаем канвас для индикаторов
        GameObject canvasObj = new GameObject("HitDirectionCanvas");
        canvasObj.transform.SetParent(transform);
        
        indicatorCanvas = canvasObj.AddComponent<Canvas>();
        indicatorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        indicatorCanvas.sortingOrder = 5; // Ниже, чем уведомления, но выше, чем обычный UI
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Создаем индикаторы для разных направлений
        frontIndicator = CreateDirectionIndicator("FrontIndicator", new Vector2(0.5f, 0.05f), new Vector2(0.3f, 0.05f));
        leftIndicator = CreateDirectionIndicator("LeftIndicator", new Vector2(0.05f, 0.5f), new Vector2(0.05f, 0.3f));
        rightIndicator = CreateDirectionIndicator("RightIndicator", new Vector2(0.95f, 0.5f), new Vector2(0.05f, 0.3f));
        rearIndicator = CreateDirectionIndicator("RearIndicator", new Vector2(0.5f, 0.95f), new Vector2(0.3f, 0.05f));
        turretIndicator = CreateDirectionIndicator("TurretIndicator", new Vector2(0.5f, 0.5f), new Vector2(0.1f, 0.1f), true);
    }
    
    /// <summary>
    /// Создает индикатор для указанного направления.
    /// </summary>
    private Image CreateDirectionIndicator(string name, Vector2 position, Vector2 size, bool isCircle = false)
    {
        GameObject indicatorObj = new GameObject(name);
        indicatorObj.transform.SetParent(indicatorCanvas.transform, false);
        
        // Настраиваем RectTransform
        RectTransform rectTransform = indicatorObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = position;
        rectTransform.anchorMax = position;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(size.x * Screen.width, size.y * Screen.height);
        
        // Добавляем компонент Image
        Image image = indicatorObj.AddComponent<Image>();
        
        // Если это круговой индикатор (для башни)
        if (isCircle)
        {
            image.sprite = CreateCircleSprite();
        }
        
        // Скрываем индикатор изначально
        image.color = new Color(1f, 1f, 1f, 0f);
        
        return image;
    }
    
    /// <summary>
    /// Создает круговой спрайт для индикатора башни.
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        // Создаем текстуру для круга
        int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        // Заполняем текстуру круговым градиентом
        Color[] colors = new Color[textureSize * textureSize];
        float radius = textureSize / 2f;
        Vector2 center = new Vector2(radius, radius);
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance < radius ? 1f - (distance / radius) : 0f;
                colors[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        // Создаем спрайт из текстуры
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// Показывает индикатор попадания с указанной стороны.
    /// </summary>
    /// <param name="hitDirection">Направление попадания (0-359 градусов)</param>
    /// <param name="isTurretHit">Было ли попадание в башню</param>
    public void ShowHitDirection(float hitDirection, bool isTurretHit = false)
    {
        // Если попадание в башню, показываем индикатор башни
        if (isTurretHit)
        {
            StartCoroutine(ShowIndicatorCoroutine(turretIndicator, turretHitColor));
            return;
        }
        
        // Определяем, в какую сторону было попадание
        // Нормализуем угол до 0-359
        hitDirection = (hitDirection + 360) % 360;
        
        // Фронтальное попадание (315-45 градусов)
        if (hitDirection > 315 || hitDirection <= 45)
        {
            StartCoroutine(ShowIndicatorCoroutine(frontIndicator, frontHitColor));
        }
        // Правое попадание (45-135 градусов)
        else if (hitDirection > 45 && hitDirection <= 135)
        {
            StartCoroutine(ShowIndicatorCoroutine(rightIndicator, sideHitColor));
        }
        // Заднее попадание (135-225 градусов)
        else if (hitDirection > 135 && hitDirection <= 225)
        {
            StartCoroutine(ShowIndicatorCoroutine(rearIndicator, rearHitColor));
        }
        // Левое попадание (225-315 градусов)
        else
        {
            StartCoroutine(ShowIndicatorCoroutine(leftIndicator, sideHitColor));
        }
    }
    
    /// <summary>
    /// Корутина для отображения индикатора попадания.
    /// </summary>
    private IEnumerator ShowIndicatorCoroutine(Image indicator, Color hitColor)
    {
        // Устанавливаем цвет и показываем индикатор
        Color color = hitColor;
        indicator.color = color;
        
        // Ждем указанное время
        yield return new WaitForSeconds(indicatorDuration - fadeOutTime);
        
        // Плавно скрываем индикатор
        float startTime = Time.time;
        
        while (Time.time < startTime + fadeOutTime)
        {
            float t = 1f - ((Time.time - startTime) / fadeOutTime);
            color.a = hitColor.a * t;
            indicator.color = color;
            yield return null;
        }
        
        // Полностью скрываем индикатор
        color.a = 0f;
        indicator.color = color;
    }
}