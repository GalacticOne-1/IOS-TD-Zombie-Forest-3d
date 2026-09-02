
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.UI.Units.Presentation;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// ТОолько для юнита игрока
    /// </summary>
    public interface ISceneUnit : IUnitSceneContext
    {
        IUnitRuntime Runtime { get; }
        //IUnitStatsScene Stats { get; }               // snapshot + ReactiveProperty для UI
        IEquipmentStatsProvider EquipmentStatsProvider { get; }
        
        //IReadOnlyInventoryView Inventory { get; }      // snapshot для UI

        //IEquipmentPresentation Equipment { get; }      // для отображения снаряги

    }
}