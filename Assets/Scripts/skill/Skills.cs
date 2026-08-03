using System.Collections;
using UnityEngine;

// AnimState values:
// 0 = idle
// 1 = Skill1a, 2 = Skill1b, 3 = Skill1c
// 4 = Skill2, 5 = Skill3, 6 = Skill4

// ─────────────────────────────────────────────
// Skill 1 – Combo Swing (a → b → c)
//
// 흐름:
//   모션 재생 → ① 0.5초 고정 락(입력 무시) → ② 3초 재사용 활성화 창
//   → 창 안에 재사용하면 다음 모션, 없으면 콤보 종료 후 스킬 쿨타임(5초)
//   ③ 3번째(마지막) 모션을 쓴 경우엔 활성화 창 없이 바로 종료
// ─────────────────────────────────────────────
public class SwingSkill : BaseSkill
{
    protected override float Cooldown => 5f;   // 스킬 전체 쿨타임 (명세: 5초)
    protected override int AnimState => 1;

    private static readonly int[] ComboStates = { 1, 2, 3 };
    private static readonly float[] ClipDurations = { 0.5f, 0.5f, 0.5f };

    private const float MotionLockTime = 0.5f;  // ① 모션간 고정 쿨타임
    private const float RecastWindow = 3f;      // ② 다음 모션 활성화 시간
    private const float HitDuration = 0.15f;

    private int _comboStep = 0;
    private bool _comboActive = false;
    private bool _canRecast = false;     // 지금이 3초 활성화 창 구간인지
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

    // AttackController가 Z 입력 시 호출.
    // 활성화 창(_canRecast)이 열려있을 때만 유효하게 반영되고,
    // 모션 재생 중 / 0.5초 락 구간에 눌러도 그냥 무시됨.
    public void BufferNextCombo()
    {
        if (_canRecast)
            _recastRequested = true;
    }

    public void CancelCombo()
    {
        ResetCombo();
        IsOnCooldown = false; // 외부에서 강제 중단된 경우이므로 여기서 직접 해제
        Animator.SetInteger("AnimState", 0);
    }

    protected override IEnumerator SkillRoutine()
    {
        while (_comboStep < ComboStates.Length)
        {
            // ── 모션 재생 + 타격 판정 ──
            Animator.SetInteger("AnimState", ComboStates[_comboStep]);
            Debug.Log($"[SwingSkill] Motion {_comboStep + 1} start");

            Hitbox.EnableHitbox(PlayerData.GetSkillDamage(0));
            yield return new WaitForSeconds(HitDuration);
            Hitbox.DisableHitbox();

            float remaining = ClipDurations[_comboStep] - HitDuration;
            if (remaining > 0f)
                yield return new WaitForSeconds(remaining);

            // 모션 클립 재생이 끝났으니 바로 IDLE로 복귀 (콤보 로직/쿨타임은 계속 백그라운드로 진행)
            Animator.SetInteger("AnimState", 0);

            bool isLastMotion = _comboStep >= ComboStates.Length - 1;

            // ① 모션간 고정 락 (0.5초) — 이 동안 들어온 입력은 위 BufferNextCombo()에서 무시됨
            yield return new WaitForSeconds(MotionLockTime);

            if (isLastMotion)
            {
                // ③ 마지막 모션이면 활성화 창 없이 바로 종료 → 스킬 쿨타임으로
                Debug.Log("[SwingSkill] Last motion done, ending combo");
                break;
            }

            // ② 3초 재사용 활성화 창
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
        // 이후 스킬 쿨타임(5초)은 BaseSkill.Execute()가 Cooldown 프로퍼티 값만큼 기다렸다가 처리함.
    }

    private void ResetCombo()
    {
        _comboStep = 0;
        _comboActive = false;
        _canRecast = false;
        _recastRequested = false;
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