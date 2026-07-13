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

    protected abstract float Cooldown { get; }
    protected abstract int AnimState { get; }  // int value for the AnimState parameter

    public BaseSkill(Animator animator, HitboxController hitbox, PlayerData playerData)
    {
        Animator = animator;
        Hitbox = hitbox;
        PlayerData = playerData;
    }

    public IEnumerator Execute(MonoBehaviour runner)
    {
        if (IsOnCooldown) yield break;

        IsOnCooldown = true;
        OnBeforeRoutine();

        Animator.SetInteger("AnimState", AnimState);

        yield return runner.StartCoroutine(SkillRoutine());

        Animator.SetInteger("AnimState", 0);  // back to idle
        yield return new WaitForSeconds(Cooldown);
        IsOnCooldown = false;
    }

    protected virtual void OnBeforeRoutine() { }

    protected abstract IEnumerator SkillRoutine();
}