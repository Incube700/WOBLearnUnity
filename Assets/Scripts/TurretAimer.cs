using UnityEngine;

/// <summary>
/// Поворачивает башню к курсору и ограничивает отклонение от корпуса.
/// </summary>
public class TurretAimer : MonoBehaviour
{
    [SerializeField] private Transform turretPivot; // точка вращения башни
    [SerializeField] private float maxAimAngle = 30f; // максимальное отклонение от корпуса

    private Camera mainCamera;                      // ссылка на основную камеру
    private Transform body;                         // ссылка на трансформ корпуса

    private void Awake()
    {
        mainCamera = Camera.main ?? FindObjectOfType<Camera>(); // находим основную камеру
        body = transform;                           // скрипт висит на корпусе игрока

        // Если pivot не назначен в инспекторе — попробуем найти автоматически
        if (turretPivot == null)
        {
            // 1) Ищем дочерний трансформ с точным именем
            var found = transform.Find("TurretPivot");

            // 2) Ищем по эвристике имени среди всех потомков
            if (found == null)
            {
                foreach (var t in GetComponentsInChildren<Transform>(true))
                {
                    if (t == transform) continue;
                    string n = t.name.ToLowerInvariant();
                    if (n.Contains("turret") || n.Contains("pivot"))
                    {
                        found = t;
                        break;
                    }
                }
            }

            // 3) Если ничего не нашли — берём первого ребёнка, иначе сам объект
            if (found == null && transform.childCount > 0)
                found = transform.GetChild(0);

            turretPivot = found != null ? found : transform;

            if (found == null)
                Debug.LogWarning("TurretAimer: turretPivot не назначен и не найден — использую transform самого объекта.", this);
        }
    }

    private void Update()
    {
        if (mainCamera == null || GameInput.Instance == null)
            return; // подстраховка, чтобы не падать, если что-то не инициализировалось

        Vector3 mouseScreen = GameInput.Instance.Aim;            // позиция курсора на экране
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen); // переводим в мировые координаты
        Vector2 dir = (mouseWorld - turretPivot.position).normalized;    // направление от башни к курсору
        float desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;  // требуемый угол к курсору
        float bodyAngle = body.eulerAngles.z;                            // текущий угол корпуса
        float relative = Mathf.DeltaAngle(bodyAngle, desiredAngle);      // угол между корпусом и курсором
        float clamped = Mathf.Clamp(relative, -maxAimAngle, maxAimAngle); // ограничиваем отклонение
        float finalAngle = bodyAngle + clamped;                          // итоговый угол башни
        turretPivot.rotation = Quaternion.Euler(0f, 0f, finalAngle);     // устанавливаем поворот башни
    }
}
