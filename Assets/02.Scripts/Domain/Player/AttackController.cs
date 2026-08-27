using System.Collections;
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
        [SerializeField] private HitboxController hitbox;

        private BaseSkill[] _skills;
        private SwingSkill _swingSkill;
        private Coroutine _currentSkill;

        void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (hitbox == null) hitbox = GetComponentInChildren<HitboxController>();
        }

        void Start()
        {
            _swingSkill = new SwingSkill(animator, hitbox, playerData);

            _skills = new BaseSkill[]
            {
                _swingSkill,
                new BiteSkill(animator, hitbox, playerData),
                new RoarSkill(animator, hitbox, playerData),
                new JumpSlamSkill(animator, hitbox, playerData)
            };

            Managers.Input.OnAttackPerformed -= OnAttackPerformed;
            Managers.Input.OnAttackPerformed += OnAttackPerformed;
        }

        void OnDestroy()
        {
            Managers.Input.OnAttackPerformed -= OnAttackPerformed;
        }

        void Update()
        {
        }

        private void OnAttackPerformed(int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= _skills.Length) return;

            if (skillIndex == 0 && _swingSkill.IsComboActive)
            {
                _swingSkill.BufferNextCombo();
                return;
            }

            if (skillIndex != 0 && _swingSkill.IsComboActive)
            {
                _swingSkill.CancelCombo();
                hitbox.DisableHitbox();
                if (_currentSkill != null) StopCoroutine(_currentSkill);
            }

            _currentSkill = StartCoroutine(_skills[skillIndex].Execute(this));
        }
    }
}