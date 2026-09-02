
using System.Collections.Generic;
using System.Linq;
using Galactic1.Core.Enums;
using UnityEngine;
using Galactic1.Items;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Galactic1.Code.Systems.Inventory
{
    public abstract class InventoryDataBase : ScriptableObject
    {
        [field: SerializeField] public string ConfigId { get; private set; }

        public string Id
        {
            get => ConfigId;
            set => ConfigId = value;
        }
        
        
        [field: SerializeField] public int BaseCapacity { get; private set; } = 10;
        /// Нужно для подгона вместимости под стартовый транспорт.
        /// Что бы инвентарь создался правильно.
        public void UpdateCapacity(int c) => BaseCapacity = c;
        
        
        protected IReadOnlyDictionary<int, EquipmentSlotType> equipmentSlots;

        public IReadOnlyDictionary<int, EquipmentSlotType> EquipmentSlots => equipmentSlots;


        /// <summary>
        /// Инициализация контейнера (создание слотов)
        /// </summary>
        public abstract void Initialize(Object data = null);
        
        public virtual int? FindSlotIndex(EquipmentSlotType requiresType)
        {
            var equipmentSlots = this.equipmentSlots;
            foreach (var slot in equipmentSlots)
                if (slot.Value == requiresType)
                    return slot.Key;

            return null;
        }
        
        public EquipmentSlotType? GetSlotType(int slotIndex)
            => equipmentSlots.TryGetValue(slotIndex, out var slotType) ? slotType : null;
        
        public virtual EquipSlotType GetEquipmentSlotType(int slotIndex) => EquipSlotType.None;


    }
}