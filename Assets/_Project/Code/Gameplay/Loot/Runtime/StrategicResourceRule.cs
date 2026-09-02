using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    /// <summary>
    /// Правило raid-wide min/max для одного стратегического ресурса.
    /// Часть LootBalanceProfile.
    ///
    /// Цель: гарантировать минимум и ограничить максимум за рейд.
    /// Например: gas_cyl >= 2, gas_cyl &lt;= 5
    /// </summary>
    [Serializable]
    public struct StrategicResourceRule
    {
        [Tooltip("Предмет — стратегический ресурс (IsStrategicResource должен быть true в LootModule).")]
        [SerializeField]
        private ItemId itemId;

        [Tooltip("Максимум за рейд. Избыток обрезается при нормализации." +
                 " 0 = нет ограничения.")]
        [Min(0)]
        [SerializeField]
        private int _maxPerRaid;

        [TextArea(1, 2)] [Tooltip("Объяснение для дизайнера.")] [SerializeField]
        private string _designNote;

        public ItemId ItemId => itemId;
        public int MaxPerRaid => _maxPerRaid;
        public string DesignNote => _designNote;
    }
}