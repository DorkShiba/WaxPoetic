using System.Collections;
using UnityEngine;

// AnimState values:
// 0 = idle
// 1 = Skill1a, 2 = Skill1b, 3 = Skill1c
// 4 = Skill2, 5 = Skill3, 6 = Skill4

// ─────────────────────────────────────────────
// Skill 1 – Combo Swing (a → b → c)
// ─────────────────────────────────────────────
public class SwingSkill : BaseSkill
{
    protected override float Cooldown => 0f;
    protected override int AnimState => 1;

    private static readonly int[] ComboStates = { 1, 2, 3 };
    private static readonly float[] ClipDurations = { 0.5f, 0.5f, 0.5f };

    private const float ComboWindow = 0.5f;
    private const float HitDuration = 0.15f;

    private int _comboStep = 0;
    private bool _comboActive = false;
    private bool _nextComboBuffered = false;

    public bool IsComboActive => _comboActive;

    public SwingSkill(Animator animator, HitboxController hitbox, PlayerData playerData)
        : base(animator, hitbox, playerData) { }

    protected override void OnBeforeRoutine()
    {
        _comboActive = true;
        _nextComboBuffered = false;
    }

    public void BufferNextCombo()
    {
        if (_comboActive)
            _nextComboBuffered = true;
    }

    public void CancelCombo()
    {
        ResetCombo();
        IsOnCooldown = false;
        Animator.SetInteger("AnimState", 0);
    }

    protected override IEnumerator SkillRoutine()
    {
        // Loop through combo steps
        while (_comboStep < ComboStates.Length)
        {
            _nextComboBuffered = false;

            Animator.SetInteger("AnimState", ComboStates[_comboStep]);
            Debug.Log($"Combo step {_comboStep} started");

            Hitbox.EnableHitbox(PlayerData.GetSkillDamage(0));
            yield return new WaitForSeconds(HitDuration);
            Hitbox.DisableHitbox();

            float remaining = ClipDurations[_comboStep] - HitDuration;
            yield return new WaitForSeconds(remaining);

            Debug.Log($"Clip done. Buffered: {_nextComboBuffered}, step: {_comboStep}");

            _comboStep++;

            // Stop if no input buffered or combo finished
            if (!_nextComboBuffered || _comboStep >= ComboStates.Length)
            {
                Debug.Log("Ending combo");
                break;
            }

            Debug.Log($"Advancing to step {_comboStep}");
        }

        ResetCombo();
        Animator.SetInteger("AnimState", 0);
    }

    private void ResetCombo()
    {
        _comboStep = 0;
        _comboActive = false;
        _nextComboBuffered = false;
        IsOnCooldown = false;
    }
}

// ─────────────────────────────────────────────
// Skill 2 – Bite
// ─────────────────────────────────────────────
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

// ─────────────────────────────────────────────
// Skill 3 – Roar (AoE)
// ─────────────────────────────────────────────
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

// ─────────────────────────────────────────────
// Skill 4 – Jump Slam
// ─────────────────────────────────────────────
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