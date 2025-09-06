using UnityEngine;

/// <summary>
/// Считает попадания, пробития и непробития по объекту.
/// </summary>
public class DebugHitCounter : MonoBehaviour
{
    private int hits = 0; // общее число попаданий
    private int penetrations = 0; // пробитий
    private int nonPenetrations = 0; // непробитий (включая рикошеты)

    public void RegisterImpact(ImpactResult2D res)
    {
        hits++; // любое попадание
        if (res.penetrated) penetrations++; // пробили
        else nonPenetrations++; // иначе непробитие
        Debug.Log($"[HitCounter] {name}: Hits={hits} Pens={penetrations} NoPen={nonPenetrations}"); // выводим статистику
    }
}
