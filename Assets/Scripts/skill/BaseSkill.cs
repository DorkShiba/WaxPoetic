using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract base for all player skills.
/// Subclasses define their own coroutine logic (multi-swing timing, AOE, etc).
/// </summary>
public abstract class BaseSkill
{
    protected Animator Animator;
    protected HitboxController Hitbox;
    protected PlayerData PlayerData;

    public bool IsOnCooldown { get; protected set; }

    // Override in subclasses
    protected abstract float Cooldown { get; }
    protected abstract string AnimTrigger { get; }

    public BaseSkill(Animator animator, HitboxController hitbox, PlayerData playerData)
    {
        Animator = animator;
        Hitbox = hitbox;
        PlayerData = playerData;
    }

    /// <summary>
    /// Entry point called by AttackController. Handles cooldown gating.
    /// </summary>
    public IEnumerator Execute(MonoBehaviour runner)
    {
        if (IsOnCooldown) yield break;

        IsOnCooldown = true;
        Animator.SetTrigger(AnimTrigger);

        yield return runner.StartCoroutine(SkillRoutine());

        yield return new WaitForSeconds(Cooldown);
        IsOnCooldown = false;
    }

    /// <summary>
    /// The actual skill logic: when to open/close hitboxes, delays, etc.
    /// </summary>
    protected abstract IEnumerator SkillRoutine();
}