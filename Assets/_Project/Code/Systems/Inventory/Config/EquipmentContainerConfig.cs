
using System.Collections.Generic;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    [CreateAssetMenu(fileName = "EquipmentContainerConfig", menuName = "Game Configs/Inventory/Equipment Container Config")]
    public class EquipmentContainerConfig : ScriptableObject
    {

        [SerializeField] private List<EquipmentSlotType> equipmentSlotTypes;



        public Dictionary<int, EquipmentSlotType> GetEquipmentSlotTypes()
        {
            var result = new Dictionary<int, EquipmentSlotType>();

            var l = equipmentSlotTypes.Count;
            for (int i = 0; i < l; i++)
                result[i] = equipmentSlotTypes[i];

            return result;
        }
    }
}