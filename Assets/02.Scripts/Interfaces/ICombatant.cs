using Domain.Combat;

namespace Interfaces
{
    /// <summary>
    /// Common contract for combat participants.
    /// Currently defines damage reception; attack actions are not yet part of this contract.
    /// </summary>
    public interface ICombatant
    {
        void TakeDamage(DamageInfo damageInfo);
    }
}
