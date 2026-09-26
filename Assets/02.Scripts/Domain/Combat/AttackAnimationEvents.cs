using UnityEngine;
using Domain.Player;

namespace Domain.Combat
{
    // Attach to the GameObject with the Animator. All events take attack IDs 1 through 6.
    public class AttackAnimationEvents : MonoBehaviour
    {
        [SerializeField] private AttackController attacks;
        private void Awake()
        {
            if (attacks == null) attacks = GetComponentInParent<AttackController>();
            if (attacks == null) Debug.LogError("[AttackAnimationEvents] Assign AttackController.", this);
        }
        public void BeginHit(int attackId)
        {
            if (attacks != null) attacks.BeginHit(attackId);
        }
        public void EndHit(int attackId)
        {
            if (attacks != null) attacks.EndHit(attackId);
        }
        public void FinishAttack(int attackId)
        {
            if (attacks != null) attacks.FinishAttack(attackId);
        }
    }
}
