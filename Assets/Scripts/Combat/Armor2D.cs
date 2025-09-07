using UnityEngine;

/// <summary>
/// Определяет толщину брони танка по сектору и подсвечивает результат последнего попадания.
/// </summary>
public class Armor2D : MonoBehaviour
{
    [Header("Толщина брони, мм")]
    [SerializeField] private float frontArmorMM = 90f;   // лоб
    [SerializeField] private float sideArmorMM = 70f;    // борта
    [SerializeField] private float rearArmorMM = 50f;    // корма

    [Header("Углы секторов, градусы")]
    [SerializeField] private float frontHalfAngle = 45f; // половина фронтового сектора
    [SerializeField] private float sideHalfAngle = 120f; // половина бокового сектора

    [Header("Нос танка (если null, берём transform.up)")]
    public Transform noseReference;

    private ArmorArc2D lastHitArc = ArmorArc2D.Rear; // последний сектор попадания
    private ArmorHitResult2D lastHitResult = ArmorHitResult2D.None; // исход последнего попадания

    // Публичные геттеры для UI/систем
    public float FrontArmorMM => frontArmorMM;
    public float SideArmorMM => sideArmorMM;
    public float RearArmorMM => rearArmorMM;

    /// <summary>
    /// Увеличивает толщину брони указанного сектора.
    /// </summary>
    public void AddArmor(ArmorArc2D arc, float amount)
    {
        switch (arc)
        {
            case ArmorArc2D.Front: frontArmorMM = Mathf.Max(0f, frontArmorMM + amount); break;
            case ArmorArc2D.Side:  sideArmorMM  = Mathf.Max(0f, sideArmorMM  + amount); break;
            case ArmorArc2D.Rear:  rearArmorMM  = Mathf.Max(0f, rearArmorMM  + amount); break;
        }
    }

    private Vector2 GetNoseForward2D()
    {
        if (noseReference != null) return (Vector2)noseReference.up; // явный нос
        return (Vector2)transform.up; // иначе up корпуса
    }

    /// <summary>
    /// Возвращает номинальную толщину брони и запоминает сектор.
    /// </summary>
    public float GetNominalArmorMM(Vector2 hitPoint, Vector2 incomingWorldDir)
    {
        Vector2 toShooter = -incomingWorldDir.normalized; // направление к стрелку
        Vector2 tankForward = GetNoseForward2D(); // направление носа

        float delta = Vector2.SignedAngle(tankForward, toShooter); // отклонение от носа
        float absDelta = Mathf.Abs(delta); // берём модуль угла

        ArmorArc2D arc = ArmorArc2D.Rear;
        if (absDelta <= frontHalfAngle) arc = ArmorArc2D.Front; // фронт
        else if (absDelta <= sideHalfAngle) arc = ArmorArc2D.Side; // борт

        lastHitArc = arc; // запоминаем сектор
        lastHitResult = ArmorHitResult2D.None; // сбрасываем исход

        switch (arc)
        {
            case ArmorArc2D.Front: return frontArmorMM; // лоб
            case ArmorArc2D.Side: return sideArmorMM;   // борт
            default: return rearArmorMM;                // корма
        }
    }

    /// <summary>
    /// Сообщает компоненту исход попадания для подсветки.
    /// </summary>
    public void ReportHitResult(ImpactResult2D res)
    {
        if (res.ricochet) lastHitResult = ArmorHitResult2D.Ricochet; // рикошет
        else if (res.penetrated) lastHitResult = ArmorHitResult2D.Penetration; // пробитие
        else lastHitResult = ArmorHitResult2D.NoPenetration; // непробитие
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; // показываем нос
        Vector3 pos = transform.position;
        Vector2 fwd = noseReference ? (Vector2)noseReference.up : (Vector2)transform.up;
        Gizmos.DrawLine(pos, pos + (Vector3)fwd * 2.8f); // линия носа

        Gizmos.color = Color.green;
        DrawArc(pos, fwd, frontHalfAngle, 2f); // сектор фронта

        Gizmos.color = Color.yellow;
        DrawArc(pos, fwd, sideHalfAngle, 2.2f); // сектор борта

        Gizmos.color = Color.red;
        DrawArc(pos, fwd, 180f, 2.4f); // сектор кормы

        if (lastHitResult != ArmorHitResult2D.None) // подсвечиваем результат
        {
            Gizmos.color = GetResultColor(lastHitResult); // выбираем цвет
            float half = GetHalfAngleForArc(lastHitArc); // нужный сектор
            DrawArc(pos, fwd, half, 2.6f); // рисуем поверх
        }
    }

    private Color GetResultColor(ArmorHitResult2D res)
    {
        switch (res)
        {
            case ArmorHitResult2D.Penetration: return Color.green; // пробитие
            case ArmorHitResult2D.NoPenetration: return Color.yellow; // непробитие
            case ArmorHitResult2D.Ricochet: return Color.red; // рикошет
            default: return Color.white;
        }
    }

    private float GetHalfAngleForArc(ArmorArc2D arc)
    {
        switch (arc)
        {
            case ArmorArc2D.Front: return frontHalfAngle; // фронт
            case ArmorArc2D.Side: return sideHalfAngle;   // борт
            default: return 180f;                         // корма
        }
    }

    private void DrawArc(Vector3 pos, Vector2 forward, float halfAngle, float r)
    {
        int steps = 32; // количество сегментов
        Vector3 prev = pos + (Vector3)(Quaternion.AngleAxis(-halfAngle, Vector3.forward) * forward) * r; // стартовая точка
        for (int i = 1; i <= steps; i++)
        {
            float t = Mathf.Lerp(-halfAngle, halfAngle, i / (float)steps); // интерполяция угла
            Vector3 cur = pos + (Vector3)(Quaternion.AngleAxis(t, Vector3.forward) * forward) * r; // текущая точка
            Gizmos.DrawLine(prev, cur); // рисуем сегмент
            prev = cur; // переходим к следующей точке
        }
    }
}

/// <summary>Сектора брони для 2D-танка.</summary>
public enum ArmorArc2D { Front, Side, Rear }

/// <summary>Исход последнего попадания.</summary>
public enum ArmorHitResult2D { None, Penetration, NoPenetration, Ricochet }
