using UnityEngine;
using Domain.Combat;
using GameData;
using Systems;

namespace Domain.Player
{
    public class AttackController : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Animator animator;
        [SerializeField] private HitboxController skill1A;
        [SerializeField] private HitboxController skill1B;
        [SerializeField] private HitboxController skill1C;
        [SerializeField] private HitboxController skill2;
        [SerializeField] private HitboxController skill3;
        [SerializeField] private HitboxController skill4;

        private BaseSkill[] skills;
        private HitboxController[] hitboxes;
        private int activeAttack;
        private int activeSkill;
        private bool hitStarted;
        private InputManager input;
        private DashController dash;
        public bool IsAttacking => activeAttack != 0;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            dash = GetComponent<DashController>();
            skills = new BaseSkill[] { new SwingSkill(), new BiteSkill(), new RoarSkill(), new JumpSlamSkill() };
            hitboxes = new[] { skill1A, skill1B, skill1C, skill2, skill3, skill4 };
        }

        private void Start() => SubscribeInput();
        private void OnEnable()
        {
            if (skills != null) SubscribeInput();
        }
        private void SubscribeInput()
        {
            if (input != null) return;
            input = Managers.Input;
            if (input != null) input.OnAttackPerformed += OnAttackPerformed;
        }
        private void OnDisable()
        {
            if (input != null) input.OnAttackPerformed -= OnAttackPerformed;
            input = null;
            CancelAttack();
        }
        private void Update()
        {
            foreach (var skill in skills) skill.Tick(Time.time);
            if (IsAttacking && dash != null && dash.IsDashing) CancelAttack();
        }

        private void OnAttackPerformed(int skillIndex)
        {
            if (!isActiveAndEnabled || IsAttacking || (dash != null && dash.IsDashing)) return;
            if (skillIndex < 0 || skillIndex >= skills.Length) return;
            if (animator == null || playerData == null)
            {
                Debug.LogError("[AttackController] Assign Animator and Player Data.", this);
                return;
            }
            // Validate all ranges before accepting input, so missing setup cannot consume a skill.
            foreach (var box in hitboxes)
            {
                if (box == null || !box.isActiveAndEnabled)
                {
                    Debug.LogError("[AttackController] Assign all six active HitboxControllers.", this);
                    return;
                }
            }
            if (!skills[skillIndex].TryActivate(Time.time, out int attackId)) return;
            activeAttack = attackId;
            activeSkill = skillIndex;
            hitStarted = false;
            animator.SetInteger("AnimState", attackId);
        }

        public void BeginHit(int attackId)
        {
            if (!isActiveAndEnabled || attackId != activeAttack || !IsAttacking || hitStarted) return;
            hitStarted = true;
            hitboxes[attackId - 1].BeginHit(playerData.GetSkillDamage(activeSkill), gameObject);
        }
        public void EndHit(int attackId)
        {
            if (!IsAttacking || attackId != activeAttack) return;
            hitboxes[attackId - 1].EndHit();
            // Allow another hit window within the same attack animation.
            hitStarted = false;
        }
        public void FinishAttack(int attackId)
        {
            if (!IsAttacking || attackId != activeAttack) return;
            CancelAttack();
        }
        public void CancelAttack()
        {
            if (hitboxes != null)
                foreach (var box in hitboxes)
                    if (box != null) box.EndHit();
            activeAttack = 0;
            hitStarted = false;
            if (animator != null) animator.SetInteger("AnimState", 0);
            // Combo deadlines and cooldowns survive motion cancellation.
        }
    }
}
