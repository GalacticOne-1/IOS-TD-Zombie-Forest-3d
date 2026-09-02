using Galactic1.Game.Meta.Items;

namespace Galactic1.Gameplay.Interaction
{
    public interface ILoot
    {
        ItemConfig DropItem { get; }
        int DropAmount { get; }
    }
}