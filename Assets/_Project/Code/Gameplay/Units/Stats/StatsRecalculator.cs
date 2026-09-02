using System.Collections.Generic;
using System.Linq;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    public class StatsRecalculator
    {
        private readonly StatsRuntimeBase runtime;
        
        /// <summary>
        /// Список статов, которые были модифицированы в ходе пересчёта.
        /// </summary>
        public readonly HashSet<StatId> DirtyStats = new();

        public StatsRecalculator(StatsRuntimeBase runtime)
        {
            this.runtime = runtime;
        }

        public void Recalculate()
        {
            DirtyStats.Clear();
            
            // #1 Сохраняем старые значения
            var oldValues = new Dictionary<StatId, float>(runtime.CalculatedStats);
            
            // #2 Сброс в базовые значения
            foreach (var key in runtime.BaseStats.Keys)
                runtime.CalculatedStats[key] = runtime.BaseStats[key];

            // #3 Применяем моды экипировки
            ApplyEquipmentModifiers();

            // #4 Применяем модификаторы бафов
            ApplyBuffModifiers();
            
            
            // #5 Сравниваем старое и новое
            foreach (var stat in runtime.CalculatedStats.Keys.Union(oldValues.Keys))
            {
                float oldV = oldValues.TryGetValue(stat, out var o) ? o : 0;
                float newV = runtime.CalculatedStats.TryGetValue(stat, out var n) ? n : 0;

                if (!Mathf.Approximately(oldV, newV))
                    DirtyStats.Add(stat);
            }
        }


        // ----------------------------------------------------------
        // EQUIPMENT
        // ----------------------------------------------------------
        private void ApplyEquipmentModifiers()
        {
            Dictionary<StatId, float> flatAdd = new();
            Dictionary<StatId, float> percentAdd = new();
            Dictionary<StatId, float> percentMult = new();

            var equipmentStatModifiers = runtime.EquipmentStatsProvider.GetEquippedModifiers();

            if (equipmentStatModifiers == null)
                return;
            
            foreach (var mod in equipmentStatModifiers) 
            {
                if (!runtime.CalculatedStats.ContainsKey(mod.StatId))
                    continue;

                switch (mod.Operation)
                {
                    case ModifierOperation.Flat:
                        flatAdd.TryAdd(mod.StatId, 0);
                        flatAdd[mod.StatId] += mod.Value;
                        break;

                    case ModifierOperation.Percent:
                        percentAdd.TryAdd(mod.StatId, 0);
                        percentAdd[mod.StatId] += mod.Value;
                        break;

                    case ModifierOperation.Multiplier:
                        percentMult.TryAdd(mod.StatId, 0);
                        percentMult[mod.StatId] += mod.Value;
                        break;
                }
                
            }
            
            // Применяем модификаторы
            var statTypes = runtime.CalculatedStats.Keys.ToList();
            foreach (var stat in statTypes)
            {
                float baseValue = runtime.CalculatedStats[stat];

                if (flatAdd.TryGetValue(stat, out float f))
                    baseValue += f;

                if (percentAdd.TryGetValue(stat, out float a))
                    baseValue *= (1 + a);

                if (percentMult.TryGetValue(stat, out float m))
                    baseValue *= (1 + m);

                runtime.CalculatedStats[stat] = baseValue;
            }
        }


        // ----------------------------------------------------------
        // BUFFS (старая правильная формула: Flat → Add → Mult)
        // ----------------------------------------------------------
        private void ApplyBuffModifiers()
        {
            // Сначала собираем по типам статов
            Dictionary<StatId, float> flatAdd = new();
            Dictionary<StatId, float> percentAdd = new();
            Dictionary<StatId, float> percentMult = new();

            foreach (var buff in runtime.Buffs.ActiveBuffs)
            {
                foreach (var mod in buff.source.modifiers)
                {
                    if (!runtime.CalculatedStats.ContainsKey(mod.StatId))
                        continue;

                    switch (mod.Operation)
                    {
                        case ModifierOperation.Flat:
                            flatAdd.TryAdd(mod.StatId, 0);
                            flatAdd[mod.StatId] += mod.Value;
                            break;

                        case ModifierOperation.Percent:
                            percentAdd.TryAdd(mod.StatId, 0);
                            percentAdd[mod.StatId] += mod.Value;
                            break;

                        case ModifierOperation.Multiplier:
                            percentMult.TryAdd(mod.StatId, 0);
                            percentMult[mod.StatId] += mod.Value;
                            break;
                    }
                }
            }

            // Применяем модификаторы в правильном порядке
            foreach (var stat in runtime.CalculatedStats.Keys.ToList())
            {
                float baseValue = runtime.CalculatedStats[stat];

                // Flat
                if (flatAdd.TryGetValue(stat, out float f))
                    baseValue += f;

                // Percent Additive
                if (percentAdd.TryGetValue(stat, out float a))
                    baseValue *= (1 + a);

                // Percent Multiplicative
                if (percentMult.TryGetValue(stat, out float m))
                    baseValue *= (1 + m);

                runtime.CalculatedStats[stat] = baseValue;
            }
        }
    }
}
