using UnityEngine;

/// <summary>
/// Маркер точки укрытия для ИИ. Расставьте такие пустые объекты у кромок стен.
/// </summary>
public class CoverPoint2D : MonoBehaviour
{
    [Tooltip("Радиус, в пределах которого считаем точку достигнутой")]
    public float reachRadius = 0.35f;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
        Gizmos.DrawSphere(transform.position, 0.12f);
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, reachRadius);
    }
}
