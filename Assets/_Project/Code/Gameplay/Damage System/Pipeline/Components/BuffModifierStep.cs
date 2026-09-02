namespace Galactic1.Code.Gameplay.Damage
{
    public sealed class BuffModifierStep : IDamageStep
    {
        public bool Process(DamageContext ctx)
        {
            //ctx.Target.Stats.Buffs.ModifyDamage(ref ctx.Damage, ctx.Type);
            return true;
        }
    }
}