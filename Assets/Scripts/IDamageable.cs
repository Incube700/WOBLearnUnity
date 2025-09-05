/// <summary>
/// Интерфейс для всех объектов, которые могут получать урон.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Применяет урон к объекту.
    /// </summary>
    void TakeDamage(float damage);
}