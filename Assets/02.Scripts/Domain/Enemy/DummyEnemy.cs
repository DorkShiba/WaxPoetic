using System.Collections;
using UnityEngine;
using Interfaces;
using Domain.Combat;

namespace Domain.Enemy
{
    /// <summary>
    /// A dummy enemy script implementing IDamageable.
    /// Responds to hit damage, flashes red, and takes knockback.
    /// Resetting health automatically upon "death" for easy sandbox testing.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DummyEnemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private float currentHealth;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Coroutine _damageFlashCoroutine;
        private Coroutine _knockbackCoroutine;

        void Start()
        {
            currentHealth = maxHealth;
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            currentHealth -= damageInfo.damage;
            Debug.Log($"[DummyEnemy] Took {damageInfo.damage} dmg at point {damageInfo.hitPoint} from {damageInfo.attacker.name}. HP remaining: {currentHealth}/{maxHealth}");

            // Flash Red
            if (_damageFlashCoroutine != null)
                StopCoroutine(_damageFlashCoroutine);
            _damageFlashCoroutine = StartCoroutine(DamageFlashRoutine(0.15f));

            // Apply knockback
            if (damageInfo.knockbackForce > 0f)
            {
                if (_knockbackCoroutine != null)
                    StopCoroutine(_knockbackCoroutine);
                _knockbackCoroutine = StartCoroutine(KnockbackRoutine(damageInfo.knockbackDirection, damageInfo.knockbackForce, 0.2f));
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private IEnumerator DamageFlashRoutine(float duration)
        {
            if (spriteRenderer == null) yield break;
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            // Use Realtime so the visual flash starts showing immediately during HitStop (timeScale = 0)
            yield return new WaitForSecondsRealtime(duration);
            spriteRenderer.color = originalColor;
            _damageFlashCoroutine = null;
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

        private void Die()
        {
            Debug.Log("[DummyEnemy] Dummy died! Resetting health for next test.");
            currentHealth = maxHealth;
        }
    }
}
