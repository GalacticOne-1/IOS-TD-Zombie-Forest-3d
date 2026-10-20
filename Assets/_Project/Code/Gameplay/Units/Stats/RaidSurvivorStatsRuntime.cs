
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Gameplay.Player;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    /// <summary>
    /// Контроллер статов игрока
    /// </summary>
    public class RaidSurvivorStatsRuntime : StatsRuntimeBase
    {
        public RaidSurvivorStatsRuntime(
            string _owner,
            Dictionary<StatId, float> baseStats,
            Dictionary<StatId, float> savedCurrent,
            IEquipmentStatsProvider equipmentStatsProvider)
            : base(_owner, baseStats, equipmentStatsProvider)
        {
            // применяем snapshot значений (HP и т.п.)
            foreach (var kvp in savedCurrent)
                SetIfExists(kvp.Key, kvp.Value);
            
            ActivateLive();
            // ******************************************************************************************************
            // ******************************************************************************************************

        }

        public override void ModifyStat(StatId stat, float amount)
        {
            base.ModifyStat(stat, amount);                      // изменить CurrentStats с clamp
        }
        public override void SetStat(StatId stat, float amount)
        {
            base.SetStat(stat, amount);                         // изменить CurrentStats с clamp
        }
        
    }
}