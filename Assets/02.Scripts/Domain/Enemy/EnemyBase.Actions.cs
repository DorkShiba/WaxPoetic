using System.Collections;
using UnityEngine;

namespace Domain.Enemy
{
    public enum EnemyState { Idle, Moving, Attacking, Hit, Dead, Chasing }

    public abstract partial class EnemyBase
    {
        [SerializeField] protected Animator animator;
        private EnemyAttackSettings currentAttack;
        private Coroutine attackRoutine;
        private Coroutine hitRoutine;
        private bool hitWindowOpen;
        public EnemyState State { get; protected set; } = EnemyState.Idle;
        public bool CanAct => isActiveAndEnabled && (State == EnemyState.Idle || State == EnemyState.Moving || State == EnemyState.Chasing);
        public abstract void Attack();

        public void SetMoving(bool moving)
        {
            if (!CanAct) return;
            State = moving ? EnemyState.Moving : EnemyState.Idle;
            if (animator != null) animator.SetBool("IsMoving", moving);
        }

        protected bool TryStartAttack(EnemyAttackSettings pattern, IEnumerator routine)
        {
            if (!CanAct || pattern == null || routine == null || animator == null || !animator.isActiveAndEnabled) return false;
            if (!animator.HasState(0, Animator.StringToHash(pattern.animatorState)))
            {
                Debug.LogError($"[{GetType().Name}] Missing attack state: {pattern.animatorState}", this);
                return false;
            }
            hitReaction?.CancelKnockback();
            currentAttack = pattern;
            State = EnemyState.Attacking;
            animator.SetBool("IsMoving", false);
            attackRoutine = StartCoroutine(RunAttack(routine));
            return true;
        }

        private IEnumerator RunAttack(IEnumerator routine)
        {
            // Ensure the handle is assigned before a routine can complete synchronously.
            yield return null;
            yield return routine;
            EndAttackHit();
            currentAttack = null;
            attackRoutine = null;
            if (State == EnemyState.Attacking) State = EnemyState.Idle;
            OnAttackStopped(false);
        }

        protected IEnumerator TriggerAttackMotion(string stateName)
        {
            int hash = Animator.StringToHash(stateName);
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
            float deadline = Time.time + 3f;
            // A Trigger is a request, not proof that the Animator has entered ATTACK.
            while (animator != null && animator.isActiveAndEnabled &&
                   animator.GetCurrentAnimatorStateInfo(0).fullPathHash != hash)
            {
                if (Time.time >= deadline)
                {
                    animator.ResetTrigger("Attack");
                    Debug.LogWarning($"[{GetType().Name}] Attack transition did not enter {stateName}. Check Controller conditions.", this);
                    yield break;
                }
                yield return null;
            }
            while (animator != null && animator.isActiveAndEnabled &&
                   animator.GetCurrentAnimatorStateInfo(0).fullPathHash == hash)
            {
                // Recover if an attack clip has finished but its return transition is missing.
                if (!animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                {
                    EndAttackHit();
                    animator.Play("Base Layer.IDLE", 0, 0f);
                    yield return null;
                    break;
                }
                yield return null;
            }
            // Keep the action locked until the outgoing blend has also finished.
            while (animator != null && animator.isActiveAndEnabled && animator.IsInTransition(0))
                yield return null;
            if (animator != null) animator.ResetTrigger("Attack");
        }

        protected IEnumerator PlayAttackMotion(string stateName)
        {
            int hash = Animator.StringToHash(stateName);
            animator.ResetTrigger("Attack");
            animator.Play(hash, 0, 0f);
            yield return null;
            while (animator != null && animator.isActiveAndEnabled)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.fullPathHash != hash) break;
                if (!animator.IsInTransition(0) && state.normalizedTime >= 1f)
                {
                    animator.Play("Base Layer.IDLE", 0, 0f);
                    yield return null;
                    break;
                }
                yield return null;
            }
        }

        // Animation Events on the monster's Animator, separate from player events.
        public void BeginAttackHit()
        {
            if (State != EnemyState.Attacking || currentAttack == null || hitWindowOpen) return;
            hitWindowOpen = true;
            if (currentAttack.hitbox != null) currentAttack.hitbox.BeginHit(currentAttack.damage, gameObject);
        }
        public void EndAttackHit()
        {
            if (currentAttack != null && currentAttack.hitbox != null) currentAttack.hitbox.EndHit();
            hitWindowOpen = false;
        }
        public void CancelAttack()
        {
            bool hadAttack = currentAttack != null;
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = null;
            EndAttackHit();
            currentAttack = null;
            if (animator != null) animator.ResetTrigger("Attack");
            if (State == EnemyState.Attacking)
            {
                State = EnemyState.Idle;
                if (animator != null && animator.isActiveAndEnabled) animator.Play("Base Layer.IDLE", 0, 0f);
            }
            if (hadAttack) OnAttackStopped(true);
        }
        // Override to clean up pattern-specific movement or spawned attack objects.
        protected virtual void OnAttackStopped(bool cancelled) { }

        private void EnterHitState()
        {
            CancelAttack();
            StopHitRoutine();
            State = EnemyState.Hit;
            if (animator != null) animator.SetBool("IsMoving", false);
            hitRoutine = StartCoroutine(PlayHitMotion());
        }
        private IEnumerator PlayHitMotion()
        {
            yield return null;
            int hash = Animator.StringToHash("Base Layer.HIT");
            if (animator != null && animator.isActiveAndEnabled && animator.HasState(0, hash))
            {
                // Direct restart makes repeated hits deterministic and avoids queued triggers.
                animator.ResetTrigger("HIt");
                yield return PlayAttackMotion("Base Layer.HIT");
            }
            else yield return new WaitForSeconds(0.2f);
            hitRoutine = null;
            if (State == EnemyState.Hit) State = EnemyState.Idle;
        }
        private void StopHitRoutine()
        {
            if (hitRoutine != null) StopCoroutine(hitRoutine);
            hitRoutine = null;
        }

    }
}
