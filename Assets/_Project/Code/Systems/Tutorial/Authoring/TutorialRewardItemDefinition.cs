using System;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Один предмет-награда шага. Авторинг-only — ссылается на существующий ItemConfig,
    /// не дублирует item-данные. Семантика полей идентична InboxService.AddReward:
    /// durability = -1 значит "полная прочность".
    /// </summary>
    [Serializable]
    public sealed class TutorialRewardItemDefinition
    {
        public ItemId itemId;
        [Min(1)] public int amount = 1;

        [Tooltip("-1 = полная прочность (см. InboxService.AddReward).")]
        public int durability = -1;

        public int ammoInMagazine = 0;

    }
}