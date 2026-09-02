
using System.Collections.Generic;

namespace Galactic1.Game.UI.Stats.DTO
{
    public readonly struct StatGroupViewDto
    {
        public readonly string GroupId;
        public readonly string Title;
        public readonly IReadOnlyList<StatDtoBase> Stats;
        public readonly bool HideIfEmpty;

        public StatGroupViewDto(
            string groupId,
            string title,
            IReadOnlyList<StatDtoBase> stats,
            bool hideIfEmpty = true)
        {
            GroupId = groupId;
            Title = title;
            Stats = stats;
            HideIfEmpty = hideIfEmpty;
        }
    }
}