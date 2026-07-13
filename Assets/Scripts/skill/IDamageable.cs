/// <summary>
/// Any GameObject that can receive damage implements this.
/// Enemies, destructible objects, etc.
/// </summary>
public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo);
}
