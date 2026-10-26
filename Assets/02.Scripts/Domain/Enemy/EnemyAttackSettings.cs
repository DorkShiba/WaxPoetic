using System;
using Domain.Combat;
using UnityEngine;

namespace Domain.Enemy
{
    // Configuration only. Runtime cooldowns belong to each monster instance.
    [Serializable]
    public class EnemyAttackSettings
    {
        public string name = "Attack";
        public int priority;
        [Min(0f)] public float minRange;
        [Min(0f)] public float maxRange = 10f;
        [Min(0f)] public float cooldown = 1f;
        public bool interruptible = true;
        public bool allowKnockback = true;
        public string animatorState = "Base Layer.ATTACK";
        public HitboxController hitbox;
        [Min(0f)] public float damage;
    }
}
