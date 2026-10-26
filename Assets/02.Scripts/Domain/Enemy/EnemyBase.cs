using System.Collections;
using UnityEngine;
using Interfaces;
using Domain.Combat;

namespace Domain.Enemy
{
    /// <summary>
    /// Shared health and hit reactions for enemies implementing ICombatant.
    /// Responds to hit damage, flashes red, and takes knockback.
    /// Subclasses define what happens when health is depleted.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract partial class EnemyBase : MonoBehaviour, ICombatant
    {
        [SerializeField] protected float maxHealth = 1000f;
        [SerializeField] protected float currentHealth;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Color _originalColor;
        private float _damageFlashUntil;
        private bool _isDamageFlashing;
        private Coroutine _knockbackCoroutine;

        protected virtual void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
                _originalColor = spriteRenderer.color;
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

            // Repeated hits extend the flash without capturing the temporary red color.
            _damageFlashUntil = Time.unscaledTime + 0.15f;
            _isDamageFlashing = true;

            // Apply knockback
            if (allowKnockback && damageInfo.knockbackForce > 0f)
            {
                if (_knockbackCoroutine != null)
                    StopCoroutine(_knockbackCoroutine);
                _knockbackCoroutine = StartCoroutine(KnockbackRoutine(damageInfo.knockbackDirection, damageInfo.knockbackForce, 0.2f));
            }

            if (currentHealth <= 0f)
            {
                CancelAttack();
                StopHitRoutine();
                State = EnemyState.Dead;
                Die();
            }
        }

        protected virtual void LateUpdate()
        {
            if (!_isDamageFlashing) return;
            if (Time.unscaledTime >= _damageFlashUntil)
            {
                RestoreFlashColor();
                return;
            }

            // Apply after Animator evaluation so animation defaults cannot hide the flash.
            if (spriteRenderer != null)
                spriteRenderer.color = Color.red;
        }

        protected virtual void OnDisable()
        {
            CancelAttack();
            StopHitRoutine();
            if (_knockbackCoroutine != null) StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = null;
            if (State != EnemyState.Dead) State = EnemyState.Idle;
            RestoreFlashColor();
        }

        private void RestoreFlashColor()
        {
            if (!_isDamageFlashing) return;
            if (spriteRenderer != null)
                spriteRenderer.color = _originalColor;
            _isDamageFlashing = false;
        }

        private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                // Decelerating knockback velocity
                float speed = force * (1f - (elapsed / duration));
                transform.Translate(direction * speed * Time.unscaledDeltaTime);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _knockbackCoroutine = null;
        }

        protected abstract void Die();
    }
}
