using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.UI.Inventory;

namespace Galactic1.Gameplay.Player
{
    public interface IPlayerController
    {
        _Entity Entity { get; }
        //StatsControllerBase StatsController { get; }
        //EquipmentContainer_old EquipmentContainer_old { get; }
    }
}