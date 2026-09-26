using System.Collections.Generic;
using UnityEngine;
using Interfaces;

namespace Domain.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class HitboxController : MonoBehaviour
    {
        [SerializeField] private Collider2D hitbox;
        [SerializeField] private LayerMask targetLayers = ~0;
        [Tooltip("Initial knockback speed. Zero disables knockback for this attack.")]
        [SerializeField, Min(0f)] private float knockbackForce = 0f;
        private readonly HashSet<ICombatant> alreadyHit = new();
        private readonly List<Collider2D> overlaps = new();
        private GameObject attacker;
        private ICombatant owner;
        private float damage;
        private bool active;

        private void Awake()
        {
            if (hitbox == null) hitbox = GetComponent<Collider2D>();
            hitbox.isTrigger = true;
            EndHit();
        }
        public void BeginHit(float amount, GameObject source)
        {
            if (!isActiveAndEnabled || source == null || hitbox == null) return;
            attacker = source;
            owner = source.GetComponentInParent<ICombatant>();
            damage = amount;
            alreadyHit.Clear();
            active = true;
            hitbox.enabled = true;
            Physics2D.SyncTransforms();
            var filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(targetLayers);
            hitbox.Overlap(filter, overlaps);
            foreach (var other in overlaps) TryHit(other);
        }
        public void EndHit()
        {
            active = false;
            if (hitbox != null) hitbox.enabled = false;
            alreadyHit.Clear();
        }
        private void OnDisable() => EndHit();
        private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
        private void TryHit(Collider2D other)
        {
            if (!active || other == null || attacker == null) return;
            if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
            if (other.transform.IsChildOf(attacker.transform)) return;
            var target = other.GetComponentInParent<ICombatant>();
            if (target == null || ReferenceEquals(target, owner)) return;
            if (!alreadyHit.Add(target)) return;
            target.TakeDamage(new DamageInfo
            {
                damage = damage,
                attacker = attacker,
                hitPoint = other.ClosestPoint(transform.position),
                knockbackDirection = ((Vector2)other.transform.position - (Vector2)attacker.transform.position).normalized,
                knockbackForce = Mathf.Max(0f, knockbackForce),
                hitStopDuration = 0f,
                cameraShakeIntensity = 0f
            });
        }
    }
}
