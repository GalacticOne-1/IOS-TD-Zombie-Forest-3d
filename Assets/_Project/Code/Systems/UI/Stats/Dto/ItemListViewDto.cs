using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.UI.Core;

namespace Galactic1.Game.UI.Stats.DTO
{
    public class ItemListViewDto : StatDtoBase, IStatTextDto
    {

        public readonly StatStyleEntry Entry;
        public readonly List<RuntimeId> ItemIds;

        public StatStyleEntry StatStyleEntry => Entry;

        public ItemListViewDto(
            StatStyleEntry entry,
            List<RuntimeId> itemIds)
            : base(entry.layoutType)
        {
            Entry = entry;
            ItemIds = itemIds;
        }
    }
}