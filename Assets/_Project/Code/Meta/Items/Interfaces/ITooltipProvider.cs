using Galactic1.UI;

namespace Galactic1.Game.Meta.Items
{
    public interface ITooltipProvider
    {
        void BuildTooltip(ref TooltipItemDto data);
    }
}