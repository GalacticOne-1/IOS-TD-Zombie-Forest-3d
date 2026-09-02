using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Core.Enums;
using Galactic1.Core.Systems.PlayerCreation;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    public abstract class EquipmentPreviewContainer : MonoBehaviour
    {
        
        
        [SerializeField] private EquipmentContainerConfig _previewConfig;

        [Header("⚔️ Equipment Visuals (как в LDOE)")] [SerializeField]
        private List<EquipmentSlotVisual> visualSlots = new();
        
        /// Активные модели в сцене
        private Dictionary<EquipSlotType, GameObject> activeModels = new ();

        
        
        Dictionary<int, EquipmentSlotType> slotTypes = new();
        protected abstract WeaponBuilderBase GetWeaponBuilder();

        protected virtual InventoryProxy InventoryProxy => null;
        public EquipmentSlotType GetSlotType(int slotIndex) => slotTypes[slotIndex];


        
        
        
        protected virtual void Awake()
        {
            slotTypes = _previewConfig.GetEquipmentSlotTypes();
        }


        public void RestoreEquipmentFromInventory()
        {
            // удаляем старые модели
            foreach (var kvp in activeModels)
                if (kvp.Value) Destroy(kvp.Value);
            activeModels.Clear();

            // создаём модели для каждого слота
            var slots = InventoryProxy.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                var item = slots[i].Item.Value;
                if (item == null) continue;

                var slotType = GetSlotType(i);

                // не спавним быстрые слоты
                if (slotType == EquipmentSlotType.QuickSlot1 ||
                    slotType == EquipmentSlotType.QuickSlot2) continue;

                BindVisual(item.GetEquipSlot(), item);
            }
        }
        
        
        private void BindVisual(EquipSlotType slot, ItemConfig item)
        {
            // if (item == null || string.IsNullOrEmpty(item.PrefabPath))
            //     return;
            //
            // // Найти attach point
            // var visualDef = visualSlots.Find(v => v.slot == slot);
            // if (visualDef == null || visualDef.attachment == null)
            //     return;
            //
            // // Удалить существующую модель
            // ClearVisual(slot);
            //
            // // *** оружие создается через отдельный сервис
            // if (slot == EquipSlotType.Weapon)
            // {
            //     var weaponBuilder = GetWeaponBuilder();
            //     activeModels[slot] = weaponBuilder.Apply(item.Weapon, visualDef.attachment);
            //     return;
            // }
            //
            //
            // // *** для всего остального
            //
            // // Создать новую
            // var prefab = Resources.Load<GameObject>($"{item.PrefabPath}");
            // if (prefab == null || visualDef.attachment == null)
            // {
            //     Debug.LogWarning("Equipment not created!");
            //     return;
            // }
            // GameObject model = Instantiate(prefab, visualDef.attachment);
            // model.transform.localPosition = Vector3.zero;
            // model.transform.localRotation = Quaternion.identity;
            // model.transform.localScale    = Vector3.one;
            //
            // activeModels[slot] = model;
        }
        
        private void ClearVisual(EquipSlotType slot)
        {
            if (activeModels.TryGetValue(slot, out var model) && model != null)
            {
                Destroy(model);
            }

            activeModels.Remove(slot);
        }
    }
}