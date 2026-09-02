using System.Collections.Generic;
using Galactic1.Game.Meta.Items;

namespace Galactic1.RaidLoot.Runtime
{
    /// <summary>
    /// Runtime-состояние экономики лута за текущий рейд.
    /// Хранит накопленные количества предметов — используется LootNormalizationService.
    ///
    /// Stateful. Создаётся один раз в BuildLootSystem, живёт до конца рейда.
    /// Сервисы его читают и пишут, но не владеют им.
    /// </summary>
    public sealed class RaidLootEconomyState
    {
        // ItemId.Guid → суммарное количество за рейд
        private readonly Dictionary<string, int> _totals = new();

        /// <summary>Сколько единиц данного предмета уже добавлено в буфер за рейд.</summary>
        public int GetTotal(ItemConfig item) 
            => _totals.TryGetValue(item.Id.Guid, out var v) ? v : 0;

        /// <summary>Зарегистрировать факт добавления amount единиц предмета.</summary>
        public void Register(ItemConfig item, int amount)
        {
            var key = item.Id.Guid;
            _totals[key] = (_totals.TryGetValue(key, out var v) ? v : 0) + amount;
        }

        /// <summary>Сбросить состояние (при завершении или перезапуске рейда).</summary>
        public void Clear() => _totals.Clear();
    }
}