using System;
using System.Collections;
using UnityEngine;

namespace Domain.Combat
{
    /// <summary>
    /// Shared hit-reaction component for both the player and enemies.
    /// Handles damage flash, knockback, invincibility frames, hit stop, and death fade.
    /// Attach alongside PlayerController or EnemyBase on the root GameObject.
    /// </summary>
    public class HitReactionController : MonoBehaviour
    {
        [Header("Flash")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private Color flashColor = Color.red;

        [Header("Invincibility Frames")]
        [Tooltip("Duration of post-hit invincibility. Set to 0 to disable.")]
        [SerializeField, Min(0f)] private float iFrameDuration = 0f;
        [SerializeField] private bool blinkDuringIFrames = false;
        [SerializeField, Min(0.01f)] private float blinkInterval = 0.1f;

        [Header("Death")]
        [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

        private Color _originalColor;
        private float _flashUntil;
        private bool _isFlashing;
        private Coroutine _knockbackCoroutine;
        private Coroutine _iFrameCoroutine;
        private Coroutine _blinkCoroutine;
        private Animator _animator;

        /// <summary>True while post-hit invincibility frames are active.</summary>
        public bool IsInvincible { get; private set; }

        /// <summary>True while a knockback impulse is being applied.</summary>
        public bool IsKnockedBack => _knockbackCoroutine != null;

        /// <summary>Fired after the death fade finishes.</summary>
        public event Action OnDied;

        private void Awake()
        {
            _animator = GetComponentInParent<Animator>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
                _originalColor = spriteRenderer.color;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Trigger flash, knockback, invincibility frames, and hit stop.
        /// Call from TakeDamage on both player and enemies.
        /// </summary>
        public void OnHit(DamageInfo info)
        {
            // Flash — repeated hits extend the timer without snapping to the restored colour.
            _flashUntil = Time.unscaledTime + flashDuration;
            _isFlashing = true;

            // Knockback
            if (info.knockbackForce > 0f)
            {
                if (_knockbackCoroutine != null) StopCoroutine(_knockbackCoroutine);
                _knockbackCoroutine = StartCoroutine(KnockbackRoutine(
                    info.knockbackDirection, info.knockbackForce));
            }

            // Invincibility frames
            if (iFrameDuration > 0f)
            {
                if (_iFrameCoroutine != null) StopCoroutine(_iFrameCoroutine);
                _iFrameCoroutine = StartCoroutine(IFrameRoutine());
            }

            // Hit stop (freezes the Animator briefly)
            if (info.hitStopDuration > 0f)
                StartCoroutine(HitStopRoutine(info.hitStopDuration));
        }

        /// <summary>
        /// Immediately cancel any active knockback impulse.
        /// Call when the entity starts an attack or an action that should not be displaced.
        /// </summary>
        public void CancelKnockback()
        {
            if (_knockbackCoroutine == null) return;
            StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = null;
        }

        /// <summary>
        /// Play the death fade and fire OnDied when it finishes.
        /// Call when health reaches zero, before the object is deactivated or pooled.
        /// </summary>
        public void OnDie()
        {
            CancelKnockback();
            if (_iFrameCoroutine != null) { StopCoroutine(_iFrameCoroutine); _iFrameCoroutine = null; }
            StopBlink();
            IsInvincible = false;
            StartCoroutine(DeathRoutine());
        }

        // ──────────────────────────────────────────────────────────────────────
        // Unity messages
        // ──────────────────────────────────────────────────────────────────────

        // Apply flash after the Animator has written its default colours.
        private void LateUpdate()
        {
            if (!_isFlashing) return;
            if (Time.unscaledTime >= _flashUntil) { RestoreColor(); return; }
            if (spriteRenderer != null) spriteRenderer.color = flashColor;
        }

        private void OnDisable()
        {
            CancelKnockback();
            if (_iFrameCoroutine != null) { StopCoroutine(_iFrameCoroutine); _iFrameCoroutine = null; }
            StopBlink();
            IsInvincible = false;
            RestoreColor();
            if (spriteRenderer != null) spriteRenderer.enabled = true;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Private routines
        // ──────────────────────────────────────────────────────────────────────

        private void RestoreColor()
        {
            if (!_isFlashing) return;
            if (spriteRenderer != null) spriteRenderer.color = _originalColor;
            _isFlashing = false;
        }

        private void StopBlink()
        {
            if (_blinkCoroutine == null) return;
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        private IEnumerator KnockbackRoutine(Vector2 direction, float force)
        {
            const float duration = 0.2f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float speed = force * (1f - elapsed / duration);
                transform.Translate(direction * speed * Time.unscaledDeltaTime);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _knockbackCoroutine = null;
        }

        private IEnumerator IFrameRoutine()
        {
            IsInvincible = true;
            if (blinkDuringIFrames)
                _blinkCoroutine = StartCoroutine(BlinkRoutine());
            yield return new WaitForSecondsRealtime(iFrameDuration);
            IsInvincible = false;
            StopBlink();
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            _iFrameCoroutine = null;
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSecondsRealtime(blinkInterval);
            }
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            if (_animator != null) _animator.speed = 0f;
            yield return new WaitForSecondsRealtime(duration);
            if (_animator != null) _animator.speed = 1f;
        }

        private IEnumerator DeathRoutine()
        {
            if (spriteRenderer == null) { OnDied?.Invoke(); yield break; }
            Color startColor = spriteRenderer.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                spriteRenderer.color = Color.Lerp(startColor, endColor, elapsed / fadeDuration);
                yield return null;
            }
            spriteRenderer.color = endColor;
            OnDied?.Invoke();
        }
    }
}
