using UnityEngine;

/// <summary>
/// Поворачивает башню к курсору и ограничивает отклонение от корпуса.
/// Предполагается, что башня «смотрит» вдоль локальной оси Y.
/// </summary>
public class TurretAimer : MonoBehaviour
{
    [SerializeField] private Transform turretPivot;        // точка вращения башни
    [SerializeField] private float maxAimAngle = 30f;      // предел отклонения (°)
    [SerializeField] private bool limitByBody = false;     // ограничивать ли поворот относительно корпуса
    [SerializeField] private float spriteUpOffset = -90f;  // поправка, если спрайт «смотрит» вверх

    private Camera mainCamera;                            // ссылка на камеру
    private Transform body;                               // трансформ корпуса

    private void Awake()
    {
        mainCamera = Camera.main ?? FindObjectOfType<Camera>(); // ищем камеру
        body = transform;                                // скрипт висит на корпусе

        // Автопоиск pivot'а, если не задан
        if (turretPivot == null)
        {
            var found = transform.Find("TurretPivot");   // ищем по имени
            if (found == null)
                foreach (var t in GetComponentsInChildren<Transform>(true)) // перебираем дочерние
                {
                    if (t == transform) continue;
                    string n = t.name.ToLowerInvariant();
                    if (n.Contains("turret") || n.Contains("pivot"))
                    {
                        found = t;
                        break;
                    }
                }
            if (found == null && transform.childCount > 0)
                found = transform.GetChild(0);           // берём первого ребёнка

            turretPivot = found != null ? found : transform; //fallback на сам корпус
            if (found == null)
                Debug.LogWarning("TurretAimer: turretPivot не найден — использую transform корпуса.", this);
        }
    }

    private void Update()
    {
        if (mainCamera == null || GameInput.Instance == null)
            return;                                      // подстраховка

        Vector3 mouseScreen = GameInput.Instance.Aim;    // координаты курсора на экране
        // В перспективной камере нужно указать глубину до плоскости объекта
        mouseScreen.z = mainCamera.WorldToScreenPoint(turretPivot.position).z;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen); // переводим в мир

        Vector3 dir3 = (mouseWorld - turretPivot.position);
        dir3.z = 0f;                                     // работаем в 2D
        Vector2 dir = ((Vector2)dir3).normalized;        // направление к курсору

        // Угол к курсору с учётом ориентации спрайта
        float desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteUpOffset;

        float finalAngle;
        if (limitByBody)
        {
            float bodyAngle = body.eulerAngles.z;            // текущий угол корпуса
            float relative = Mathf.DeltaAngle(bodyAngle, desiredAngle);      // относительный угол
            float clamped = Mathf.Clamp(relative, -maxAimAngle, maxAimAngle);// ограничиваем
            finalAngle = bodyAngle + clamped;                 // итоговый угол башни
        }
        else
        {
            finalAngle = desiredAngle;                        // свободное наведение
        }

        turretPivot.rotation = Quaternion.Euler(0f, 0f, finalAngle); // применяем поворот
    }
}
