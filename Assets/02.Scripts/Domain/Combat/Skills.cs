namespace Domain.Combat
{
    // Attack IDs also match the Animator's AnimState: 1=A, 2=B, 3=C, 4=Skill2, 5=Skill3, 6=Skill4.
    public class SwingSkill : BaseSkill
    {
        protected override float Cooldown => 5f;
        protected override int AttackId => 1;
        private int nextAttack = 1;
        private float activationStart;
        private float activationEnd;

        public override void Tick(float now)
        {
            if (nextAttack == 1 || now < activationEnd) return;
            // Expiry starts cooldown at the deadline, even if observed on a later frame.
            cooldownUntil = activationEnd + Cooldown;
            nextAttack = 1;
        }

        public override bool TryActivate(float now, out int attackId)
        {
            Tick(now);
            attackId = nextAttack;
            if (now < cooldownUntil || (nextAttack != 1 && now < activationStart)) return false;
            if (nextAttack == 3)
            {
                nextAttack = 1;
                cooldownUntil = now + Cooldown;
            }
            else
            {
                nextAttack++;
                activationStart = now + 0.5f;
                activationEnd = activationStart + 5f;
            }
            return true;
        }
    }

    public class BiteSkill : BaseSkill
    {
        protected override float Cooldown => 0.8f;
        protected override int AttackId => 4;
    }
    public class RoarSkill : BaseSkill
    {
        protected override float Cooldown => 5f;
        protected override int AttackId => 5;
    }
    public class JumpSlamSkill : BaseSkill
    {
        protected override float Cooldown => 3f;
        protected override int AttackId => 6;
    }
}
