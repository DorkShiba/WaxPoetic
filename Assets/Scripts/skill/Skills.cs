using System.Collections;
using UnityEngine;

// ─────────────────────────────────────────────
// Skill 1 – Combo Swing (a → b → c)
// Each Z press advances the combo.
// Can be cancelled by pressing another skill.
// ─────────────────────────────────────────────
public class SwingSkill : BaseSkill
{
    protected override float Cooldown => 0.3f;
    protected override string AnimTrigger => "";  // each step has its own trigger

    private static readonly string[] ComboTriggers = { "Skill1a", "Skill1b", "Skill1c" };
    private const float ComboWindow = 0.8f;
    private const float HitDuration = 0.15f;

    private int _comboStep = 0;
    private float _comboTimer = 0f;
    private bool _comboActive = false;

    public bool IsComboActive => _comboActive;

    public SwingSkill(Animator animator, HitboxController hitbox, PlayerData playerData)
        : base(animator, hitbox, playerData) { }

    public void OnUpdate()
    {
        if (!_comboActive) return;

        _comboTimer += Time.deltaTime;
        if (_comboTimer >= ComboWindow)
            ResetCombo();
    }

    /// <summary>Called by AttackController when another skill interrupts the combo.</summary>
    public void CancelCombo()
    {
        ResetCombo();
        IsOnCooldown = false;  // allow the interrupting skill to run immediately
    }

    protected override IEnumerator SkillRoutine()
    {
        _comboActive = true;
        _comboTimer = 0f;

        Animator.SetTrigger(ComboTriggers[_comboStep]);
        Hitbox.EnableHitbox(PlayerData.GetSkillDamage(0));
        yield return new WaitForSeconds(HitDuration);
        Hitbox.DisableHitbox();

        _comboStep++;
        if (_comboStep >= ComboTriggers.Length)
            ResetCombo();
    }

    private void ResetCombo()
    {
        _comboStep = 0;
        _comboActive = false;
        _comboTimer = 0f;
    }
}

// ─────────────────────────────────────────────
// Skill 2 – Bite
// ─────────────────────────────────────────────
public class BiteSkill : BaseSkill
{
    protected override float Cooldown => 0.8f;
    protected override string AnimTrigger => "Skill2";

    private const float BiteDuration = 0.2f;

    public BiteSkill(Animator animator, HitboxController hitbox, PlayerData playerData)
        : base(animator, hitbox, playerData) { }

    protected override IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(0.1f);

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
    protected override string AnimTrigger => "Skill3";

    private const float RoarDuration = 0.4f;

    public RoarSkill(Animator animator, HitboxController hitbox, PlayerData playerData)
        : base(animator, hitbox, playerData) { }

    protected override IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(0.25f);

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
    protected override string AnimTrigger => "Skill4";

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