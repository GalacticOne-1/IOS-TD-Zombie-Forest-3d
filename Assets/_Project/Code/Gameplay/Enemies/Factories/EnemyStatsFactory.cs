using System.Collections.Generic;

namespace Galactic1.Game.Meta.Enemy
{
    /// <summary>
    /// Converts authored enemy stats into runtime stat dictionary.
    /// </summary>
    public static class EnemyStatsFactory
    {
        public static Dictionary<StatId, float> Build(
            EnemyStatsConfig stats,
            EnemyCombatConfig combat)
        {
            var s = stats.BaseStats;

            return new Dictionary<StatId, float>
            {
                [StatId.Health] = s.Health,
                [StatId.Armor] = s.Armor,

                [StatId.Damage] = combat.Damage,
                [StatId.ReloadSpeed] = combat.AttackCooldown,
                [StatId.AttackRange] = combat.AttackRange,
            };
        }
    }
}