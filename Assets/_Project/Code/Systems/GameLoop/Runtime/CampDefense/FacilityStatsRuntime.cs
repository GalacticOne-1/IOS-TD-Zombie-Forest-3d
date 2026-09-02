using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Game.Buildings.Proxy;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Runtime статов боевого сооружения.
    /// Полностью аналогичен SurvivorStatsRuntime.
    /// </summary>
    public sealed class FacilityStatsRuntime : StatsRuntimeBase
    {
        public FacilityProxy Proxy { get; }

        public FacilityStatsRuntime(
            string owner,
            FacilityProxy proxy,
            Dictionary<StatId, float> baseStats,
            IEquipmentStatsProvider equipmentStatsProvider)
            : base(owner, baseStats, equipmentStatsProvider)
        {
            Proxy = proxy;

            ActivateLive();

            OnDeath += () =>
            {
                Proxy.IsDead.Value = true;

#if UNITY_EDITOR
                DLog.Alert($"Facility destroyed: {owner}", EDlogColor.ORANGE);
#endif
            };

            // обновить UI после загрузки
            foreach (var stat in Proxy.Stats)
                stat.Value.ForceNotify();
        }

        protected override void ApplySave()
        {
            if (Proxy.Stats.TryGetValue(StatId.Health, out var hp))
                SetIfExists(StatId.Health, hp.Value);
        }

        public override void ModifyStat(StatId stat, float amount)
        {
            base.ModifyStat(stat, amount);

            if (Proxy.Stats.ContainsKey(stat))
                Proxy.Stats[stat].Value = CurrentStats[stat];
        }

        public override void SetStat(StatId stat, float amount)
        {
            base.SetStat(stat, amount);

            if (Proxy.Stats.ContainsKey(stat))
                Proxy.Stats[stat].Value = CurrentStats[stat];
        }

        protected override void SyncProxyStats()
        {
            foreach (var stat in _recalculator.DirtyStats)
            {
                if (!Proxy.Stats.ContainsKey(stat))
                    continue;

                Proxy.Stats[stat].Value = resourceStats.Contains(stat)
                    ? CurrentStats[stat]
                    : CalculatedStats[stat];
            }
        }
    }
}