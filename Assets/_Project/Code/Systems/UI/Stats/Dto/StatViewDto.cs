
using Galactic1.UI.Core;

namespace Galactic1.Game.UI.Stats.DTO
{
    public class StatViewDto : StatDtoBase, IStatTextDto
    {

        public readonly StatStyleEntry Entry;
        public readonly float Value;

        public StatStyleEntry StatStyleEntry => Entry;

        public StatViewDto(
            StatStyleEntry entry,
            float value)
            : base(entry.layoutType)
        {
            Entry = entry;
            Value = value;
        }
    }
}