using System.Collections;
using UnityEngine;
using GameData;

namespace Domain.Combat
{
    // AnimState values:
    // 0 = idle
    // 1 = Skill1a, 2 = Skill1b, 3 = Skill1c
    // 4 = Skill2, 5 = Skill3, 6 = Skill4

    public class SwingSkill : BaseSkill
    {
        protected override float Cooldown => 5f;
        protected override int AnimState => 1;

        private static readonly int[] ComboStates = { 1, 2, 3 };
        private static readonly float[] ClipDurations = { 0.5f, 0.5f, 0.5f };

        private const float MotionLockTime = 0.5f;
        private const float RecastWindow = 3f;
        private const float HitDuration = 0.15f;

        private int _comboStep = 0;
        private bool _comboActive = false;
        private bool _canRecast = false;
        private bool _recastRequested = false;

        public bool IsComboActive => _comboActive;

        public SwingSkill(Animator animator, HitboxController hitbox, PlayerData playerData)
            : base(animator, hitbox, playerData) { }

        protected override void OnBeforeRoutine()
        {
            _comboStep = 0;
            _comboActive = true;
            _canRecast = false;
            _recastRequested = false;
        }

        public void BufferNextCombo()
        {
            if (_canRecast)
                _recastRequested = true;
        }

        public void CancelCombo()
        {
            ResetCombo();
            IsOnCooldown = false;
            Animator.SetInteger("AnimState", 0);
        }

        protected override IEnumerator SkillRoutine()
        {
            while (_comboStep < ComboStates.Length)
            {
                Animator.SetInteger("AnimState", ComboStates[_comboStep]);
                Debug.Log($"[SwingSkill] Motion {_comboStep + 1} start");

                Hitbox.EnableHitbox(PlayerData.GetSkillDamage(0));
                yield return new WaitForSeconds(HitDuration);
                Hitbox.DisableHitbox();

                float remaining = ClipDurations[_comboStep] - HitDuration;
                if (remaining > 0f)
                    yield return new WaitForSeconds(remaining);

                Animator.SetInteger("AnimState", 0);

                bool isLastMotion = _comboStep >= ComboStates.Length - 1;

                yield return new WaitForSeconds(MotionLockTime);

                if (isLastMotion)
                {
                    Debug.Log("[SwingSkill] Last motion done, ending combo");
                    break;
                }

                _recastRequested = false;
                _canRecast = true;

                float timer = RecastWindow;
                while (timer > 0f && !_recastRequested)
                {
                    timer -= Time.deltaTime;
                    yield return null;
                }
                _canRecast = false;

                if (!_recastRequested)
                {
                    Debug.Log("[SwingSkill] Recast window expired, ending combo");
                    break;
                }

                Debug.Log("[SwingSkill] Recast received, advancing combo");
                _comboStep++;
            }

            ResetCombo();
            Animator.SetInteger("AnimState", 0);
        }

        private void ResetCombo()
        {
            _comboStep = 0;
            _comboActive = false;
            _canRecast = false;
            _recastRequested = false;
        }
    }

    public class BiteSkill : BaseSkill
    {
        protected override float Cooldown => 0.8f;
        protected override int AnimState => 4;

        private const float WindUp = 0.1f;
        private const float BiteDuration = 0.2f;

        public BiteSkill(Animator animator, HitboxController hitbox, PlayerData playerData)
            : base(animator, hitbox, playerData) { }

        protected override IEnumerator SkillRoutine()
        {
            yield return new WaitForSeconds(WindUp);
            Hitbox.EnableHitbox(PlayerData.GetSkillDamage(1));
            yield return new WaitForSeconds(BiteDuration);
            Hitbox.DisableHitbox();
        }
    }

    public class RoarSkill : BaseSkill
    {
        protected override float Cooldown => 5.0f;
        protected override int AnimState => 5;

        private const float WindUp = 0.25f;
        private const float RoarDuration = 0.4f;

        public RoarSkill(Animator animator, HitboxController hitbox, PlayerData playerData)
            : base(animator, hitbox, playerData) { }

        protected override IEnumerator SkillRoutine()
        {
            yield return new WaitForSeconds(WindUp);
            Hitbox.EnableHitbox(PlayerData.GetSkillDamage(2));
            yield return new WaitForSeconds(RoarDuration);
            Hitbox.DisableHitbox();
        }
    }

    public class JumpSlamSkill : BaseSkill
    {
        protected override float Cooldown => 3.0f;
        protected override int AnimState => 6;

        private const float AirTime = 0.45f;
        private const float LandingDuration = 0.2f;

        public JumpSlamSkill(Animator animator, HitboxController hitbox, PlayerData playerData)
            : base(animator, hitbox, playerData) { }

        protected override IEnumerator SkillRoutine()
        {
            yield return new WaitForSeconds(AirTime);
            Hitbox.EnableHitbox(PlayerData.GetSkillDamage(3));
            yield return new WaitForSeconds(LandingDuration);
            Hitbox.DisableHitbox();
        }
    }
}