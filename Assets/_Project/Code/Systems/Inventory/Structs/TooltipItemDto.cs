using System.Collections.Generic;
using Galactic1.Code.Items;
using Galactic1.Game.Meta.Stats;

namespace Galactic1.UI
{
    public class TooltipItemDto
    {
         public IReadOnlyList<ItemStatEntry> stats;
         public IReadOnlyList<DescriptorDisplayEntry> descriptors;
    }
}