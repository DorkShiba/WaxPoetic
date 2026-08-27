using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;
using Domain.Camera;

namespace Domain.Combat
{
    /// <summary>
    /// Attached to a child GameObject that holds the attack Collider2D(s).
    /// Animation Events call Enable/Disable on this to open/close the hitbox window.
    /// </summary>
    public class HitboxController : MonoBehaviour
    {
        [SerializeField] private Collider2D hitbox;

        private float _damage;
        private float _knockbackForce;
        private float _hitStopDuration;
        private float _cameraShakeIntensity;
        private Coroutine _hitStopCoroutine;

        private HashSet<GameObject> _alreadyHit = new();

        void Awake()
        {
            if (hitbox == null)
                hitbox = GetComponent<Collider2D>();

            hitbox.isTrigger = true;
            hitbox.enabled = false;
        }

        public void EnableHitbox(float damage, float knockbackForce = 2f, float hitStopDuration = 0.05f, float cameraShakeIntensity = 0.05f)
        {
            Debug.Log($"[Hitbox] Enabled with {damage} dmg, KB: {knockbackForce}, HitStop: {hitStopDuration}, Shake: {cameraShakeIntensity}");
            _damage = damage;
            _knockbackForce = knockbackForce;
            _hitStopDuration = hitStopDuration;
            _cameraShakeIntensity = cameraShakeIntensity;
            _alreadyHit.Clear();
            hitbox.enabled = true;
        }

        public void DisableHitbox()
        {
            Debug.Log("[Hitbox] Disabled");
            hitbox.enabled = false;
            _alreadyHit.Clear();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;
            if (_alreadyHit.Contains(other.gameObject)) return;

            _alreadyHit.Add(other.gameObject);

            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 hitPoint = other.ClosestPoint(transform.position);
                Vector2 attackerPos = transform.parent != null ? (Vector2)transform.parent.position : (Vector2)transform.position;
                Vector2 knockbackDirection = ((Vector2)other.transform.position - attackerPos).normalized;

                DamageInfo damageInfo = new DamageInfo
                {
                    damage = _damage,
                    hitPoint = hitPoint,
                    knockbackDirection = knockbackDirection,
                    knockbackForce = _knockbackForce,
                    hitStopDuration = _hitStopDuration,
                    cameraShakeIntensity = _cameraShakeIntensity,
                    attacker = transform.parent != null ? transform.parent.gameObject : gameObject
                };

                damageable.TakeDamage(damageInfo);
                Debug.Log($"[Hitbox] Hit {other.gameObject.name} for {damageInfo.damage} dmg");

                CameraController cam = UnityEngine.Camera.main != null ? UnityEngine.Camera.main.GetComponent<CameraController>() : null;
                if (cam != null && _cameraShakeIntensity > 0f)
                {
                    cam.Shake(_hitStopDuration + 0.1f, _cameraShakeIntensity);
                }

                ApplyHitStop(_hitStopDuration);
            }
        }

        private void ApplyHitStop(float duration)
        {
            if (duration <= 0f) return;
            if (_hitStopCoroutine != null)
                StopCoroutine(_hitStopCoroutine);
            _hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = originalTimeScale;
            _hitStopCoroutine = null;
        }
    }
}
