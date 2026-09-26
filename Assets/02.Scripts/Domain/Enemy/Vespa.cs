using UnityEngine;

namespace Domain.Enemy
{
    public class Vespa : EnemyBase
    {
        protected override void Die()
        {
            // Preserve the current Vespa behavior until its death rules are defined.
            Debug.Log("[Vespa] Health depleted. Resetting health.");
            currentHealth = maxHealth;
        }
    }
}
