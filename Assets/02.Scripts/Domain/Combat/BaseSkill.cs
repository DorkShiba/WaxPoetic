namespace Domain.Combat
{
    // Timers use the caller's game time and are independent of animation playback.
    public abstract class BaseSkill
    {
        protected float cooldownUntil;
        protected abstract float Cooldown { get; }
        protected abstract int AttackId { get; }
        public virtual void Tick(float now) { }
        public float CooldownRemaining(float now) => System.Math.Max(0f, cooldownUntil - now);
        public virtual bool TryActivate(float now, out int attackId)
        {
            attackId = AttackId;
            if (now < cooldownUntil) return false;
            cooldownUntil = now + Cooldown;
            return true;
        }
    }
}
