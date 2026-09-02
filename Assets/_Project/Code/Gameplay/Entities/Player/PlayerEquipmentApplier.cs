
using Galactic1.Code.Gameplay.Units;
using Galactic1.Items;

namespace Galactic1.Gameplay.Player
{
    /// <summary>
    /// Applies body armor and weapons to player avatar.
    /// Similar to LDoE equipment binding.
    /// </summary>
    public static class PlayerEquipmentApplier
    {
        public static void Apply(PlayerLoadData playerData, SurvivorInstance survGO)
        {
            // Восстановили снарягу (визуал + durability + статы)
            //survGO.EquipmentContainer_old.RestoreEquipmentFromInventory();

             //if (playerData.combatUnit.EquipmentPresentation != null)
                 playerData.UnitRuntime.EquipmentService.RestoreEquipmentFromInventory();
             //else
                 //playerData.combatUnit.RaidUnitRuntime.MetaUnit.EquipmentService.RestoreEquipmentFromInventory();
        }
    }
}