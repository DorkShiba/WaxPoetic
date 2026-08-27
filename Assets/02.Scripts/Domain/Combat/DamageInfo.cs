using UnityEngine;

namespace Domain.Combat
{
    /// <summary>
    /// Contains all properties associated with an attack impact / hit.
    /// Passed to IDamageable targets when they are struck.
    /// </summary>
    public struct DamageInfo
    {
        public float damage;
        public Vector2 hitPoint;
        public Vector2 knockbackDirection;
        public float knockbackForce;
        public float hitStopDuration;
        public float cameraShakeIntensity;
        public GameObject attacker;
    }
}
