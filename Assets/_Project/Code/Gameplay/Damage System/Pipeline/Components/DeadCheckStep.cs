namespace Galactic1.Code.Gameplay.Damage
{
    public sealed class DeadCheckStep : IDamageStep
    {
        public bool Process(DamageContext ctx)
        {
            if (ctx.Target.Stats.IsDead)
            {
                ctx.Cancel();
                return false;
            }
            return true;
        }
    }
}