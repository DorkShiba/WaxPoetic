using System.Collections;
using UnityEngine;

namespace Domain.Enemy
{
    public class Vespa : EnemyBase
    {
        [SerializeField] private Transform target;
        [SerializeField] private EnemyAttackSettings[] attacks = { new EnemyAttackSettings() };
        private float[] readyAt;
        [Header("Target pursuit")]
        [SerializeField, Min(0f)] private float detectionRadius = 3f;
        [SerializeField, Min(0f)] private float disengageRadius = 5f;
        [SerializeField, Min(0f)] private float chaseSpeed = 2f;
        private bool targetAcquired;
        [Header("Behaviour selection")]
        [SerializeField] private Vector2 idleDuration = new Vector2(1f, 3f);
        [SerializeField] private Vector2 walkDuration = new Vector2(1f, 3f);
        [SerializeField, Min(0f)] private float idleWeight = 1f;
        [SerializeField, Min(0f)] private float walkWeight = 1f;
        [SerializeField, Min(0f)] private float wanderRadius = 3f;
        [SerializeField, Min(0f)] private float moveSpeed = 1f;
        [SerializeField, Min(0.01f)] private float arrivalDistance = 0.1f;
        [Tooltip("Solid obstacle layers only; exclude Player, Enemy and Item.")]
        [SerializeField] private LayerMask obstacleLayers;
        private Vector2 wanderCenter;
        private Vector2 destination;
        private float chooseAt;
        private bool resumeWithIdle;

        protected override void Start()
        {
            base.Start();
            ResetBehaviour();
        }

        // Call after placement when reusing a pooled monster.
        public void ResetBehaviour()
        {
            wanderCenter = transform.position;
            targetAcquired = false;
            resumeWithIdle = false;
            if (CanAct) BeginIdle();
        }

        private static float Duration(Vector2 range)
        {
            float min = Mathf.Max(0.1f, Mathf.Min(range.x, range.y));
            return Random.Range(min, Mathf.Max(min, Mathf.Max(range.x, range.y)));
        }

        private void BeginIdle()
        {
            SetMoving(false);
            chooseAt = Time.time + Duration(idleDuration);
        }

        private void Update()
        {
            if (!CanAct) { resumeWithIdle = true; return; }
            // Pursuit and attacks take priority over random wandering.
            if (UpdateTargetAcquisition())
            {
                resumeWithIdle = true;
                if (SelectPattern(requireReady: false) >= 0)
                {
                    SetMoving(false);
                    Attack();
                }
                else
                {
                    SetMoving(true);
                    State = EnemyState.Chasing;
                }
                return;
            }
            if (resumeWithIdle)
            {
                resumeWithIdle = false;
                BeginIdle();
                return;
            }
            if (Time.time < chooseAt) return;
            float walk = wanderRadius > 0f && moveSpeed > 0f ? Mathf.Max(0f, walkWeight) : 0f;
            float idle = Mathf.Max(0f, idleWeight);
            float choice = Random.value * (idle + walk);
            if (walk > 0f && choice >= idle)
            {
                destination = wanderCenter + Random.insideUnitCircle * wanderRadius;
                SetMoving(true);
                chooseAt = Time.time + Duration(walkDuration);
            }
            else BeginIdle();
        }

        private void FixedUpdate()
        {
            if (!CanAct) return;
            if (State == EnemyState.Chasing)
            {
                if (!UpdateTargetAcquisition() || SelectPattern(requireReady: false) >= 0)
                {
                    SetMoving(false);
                    resumeWithIdle = true;
                    return;
                }
                MoveTowards(GetApproachDestination(), chaseSpeed);
                return;
            }
            if (State != EnemyState.Moving) return;
            // Update will choose pursuit on the next frame; don't wander away meanwhile.
            if (UpdateTargetAcquisition()) { SetMoving(false); return; }
            Vector2 position = transform.position;
            if (Vector2.Distance(position, destination) <= arrivalDistance)
            {
                BeginIdle();
                return;
            }
            if (!MoveTowards(destination, moveSpeed)) BeginIdle();
        }

        private bool MoveTowards(Vector2 goal, float speed)
        {
            Vector2 position = transform.position;
            Vector2 next = Vector2.MoveTowards(position, goal, Mathf.Max(0f, speed) * Time.fixedDeltaTime);
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(obstacleLayers);
            Vector2 delta = next - position;
            if (delta.sqrMagnitude > 0f && Physics2D.CircleCast(position, 0.15f, delta.normalized,
                filter, obstacleHits, delta.magnitude) > 0)
            {
                return false;
            }
            transform.position = new Vector3(next.x, next.y, transform.position.z);
            return true;
        }
        private readonly RaycastHit2D[] obstacleHits = new RaycastHit2D[1];

        private bool UpdateTargetAcquisition()
        {
            if (target == null || !target.gameObject.activeInHierarchy)
                return targetAcquired = false;
            float distance = Vector2.Distance(transform.position, target.position);
            return targetAcquired = distance <= detectionRadius;
        }

        private Vector2 GetApproachDestination()
        {
            Vector2 position = transform.position;
            Vector2 targetPosition = target.position;
            float distance = Vector2.Distance(position, targetPosition);
            EnemyAttackSettings nearest = null;
            float gap = float.PositiveInfinity;
            foreach (var pattern in attacks)
            {
                if (pattern == null || pattern.maxRange < pattern.minRange || pattern.maxRange < 0f) continue;
                float candidateGap = Mathf.Abs(distance - Mathf.Clamp(distance, Mathf.Max(0f, pattern.minRange), pattern.maxRange));
                if (candidateGap < gap) { gap = candidateGap; nearest = pattern; }
            }
            if (nearest == null) return position;
            // Aim slightly inside a valid range, rather than overshooting into the player.
            float range = (Mathf.Max(0f, nearest.minRange) + nearest.maxRange) * 0.5f;
            Vector2 outward = distance > 0.001f ? (position - targetPosition) / distance : Vector2.right;
            return targetPosition + outward * range;
        }

        protected override void Awake()
        {
            base.Awake();
            readyAt = new float[attacks.Length];
        }

        public void SetTarget(Transform value)
        {
            target = value;
            targetAcquired = false;
        }

        private int SelectPattern(bool requireReady = true)
        {
            if (!CanAct || target == null || !target.gameObject.activeInHierarchy) return -1;
            float distance = Vector2.Distance(transform.position, target.position);
            int selected = -1;
            for (int i = 0; i < attacks.Length; i++)
            {
                var pattern = attacks[i];
                if (pattern == null || (requireReady && Time.time < readyAt[i])) continue;
                if (distance < pattern.minRange || distance > pattern.maxRange) continue;
                if (selected < 0 || pattern.priority > attacks[selected].priority) selected = i;
            }
            return selected;
        }

        public override void Attack()
        {
            int selected = SelectPattern();
            if (selected < 0) return;
            var chosen = attacks[selected];
            if (TryStartAttack(chosen, ExecutePattern(selected, chosen)))
                readyAt[selected] = Time.time + Mathf.Max(0f, chosen.cooldown);
        }
        // Add pattern-specific routines here when movement/projectile rules are defined.
        protected virtual IEnumerator ExecutePattern(int index, EnemyAttackSettings pattern)
        {
            yield return TriggerAttackMotion(pattern.animatorState);
        }

        protected override void Die()
        {
            Debug.Log("[Vespa] Health depleted. Resetting health.");
            currentHealth = maxHealth;
            State = EnemyState.Idle;
        }
    }
}
