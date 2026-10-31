using UnityEngine;
using Interfaces;
using Domain.Combat;

namespace Domain.Enemy
{
    /// <summary>
    /// Shared health and hit reactions for enemies implementing ICombatant.
    /// Visual feedback (flash, knockback, death fade) is delegated to HitReactionController.
    /// Subclasses define what happens when health is depleted.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(HitReactionController))]
    public abstract partial class EnemyBase : MonoBehaviour, ICombatant
    {
        [SerializeField] protected float maxHealth = 1000f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected HitReactionController hitReaction;

        protected virtual void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (hitReaction == null) hitReaction = GetComponent<HitReactionController>();
        }

        protected virtual void Start()
        {
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            if (!isActiveAndEnabled || State == EnemyState.Dead) return;
            bool allowKnockback = currentAttack == null || currentAttack.allowKnockback;
            bool interrupt = State != EnemyState.Attacking || currentAttack == null || currentAttack.interruptible;
            if (interrupt) EnterHitState();
            currentHealth -= damageInfo.damage;
            Debug.Log($"[{GetType().Name}] Took {damageInfo.damage} dmg at point {damageInfo.hitPoint} from {damageInfo.attacker?.name}. HP remaining: {currentHealth}/{maxHealth}");

            // Suppress knockback if the enemy is mid-attack and knockback is disabled for that pattern.
            var info = damageInfo;
            if (!allowKnockback) info.knockbackForce = 0f;
            hitReaction?.OnHit(info);

            if (currentHealth <= 0f)
            {
                CancelAttack();
                StopHitRoutine();
                State = EnemyState.Dead;
                hitReaction?.OnDie();
                Die();
            }
        }

        protected virtual void OnDisable()
        {
            CancelAttack();
            StopHitRoutine();
            if (State != EnemyState.Dead) State = EnemyState.Idle;
        }

        protected abstract void Die();
    }
}
