
namespace Galactic1.Code.Gameplay.Damage
{
    public sealed class ApplyDamageStep : IDamageStep
    {
        public bool Process(DamageContext ctx)
        {
            if (ctx.Damage <= 0)
                return false;
            
            // 🔹 фиксируем финальный урон
            ctx.FinalDamage = ctx.Damage;

            // 🔹 применяем
            ctx.Target.Stats.ModifyStat(StatId.Health, -ctx.Damage);
            
#if UNITY_EDITOR
            DLog.Alert($"*** {ctx.Target} got a damage {ctx.Damage} => {ctx.Target.Stats.Get(StatId.Health)} ***", EDlogColor.ORANGE);
#endif

            return true;
        }
    }
}