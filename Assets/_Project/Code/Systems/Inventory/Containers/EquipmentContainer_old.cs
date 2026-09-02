using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Core.Enums;
using Galactic1.Core.Systems.PlayerCreation;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Центральный управляющий класс экипировкой, как в LDOE.
    /// Отвечает за:
    /// - надевание/снятие предметов
    /// - прочность
    /// - визуалы
    /// - передачу модификаторов статов
    /// - автоснятие предмета при 0 durability
    /// </summary>
    public abstract class EquipmentContainer_old : MonoBehaviour, IInventoryContainer
    {
        [SerializeField] protected EquipmentContainerConfig _equipmentContainerConfig;
        public virtual BaseInventoryData Inventory => null;
        


        [Header("⚔️ Equipment Visuals (как в LDOE)")]
        public List<EquipmentSlotVisual> visualSlots = new();

        /// Активные модели в сцене
        private Dictionary<EquipSlotType, GameObject> activeModels = new ();


        
        
        // события для UI
        public event Action<EquipSlotType, ItemConfig> OnEquipped;
        public event Action<EquipSlotType> OnUnequipped;
        public event Action<ItemConfig> OnDurabilityChanged;


        
        // =======================================================================
        // PUBLIC API
        // =======================================================================
        
        public void RestoreEquipmentFromInventory()
        {
            // удаляем старые модели
            foreach (var kvp in activeModels)
                if (kvp.Value) Destroy(kvp.Value);
            activeModels.Clear();

            // создаём модели для каждого слота
            var slots = Inventory.InventoryProxy.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                var item = slots[i].Item.Value;
                if (item == null) continue;

                var slotType = Inventory.GetSlotType(i);

                // не спавним быстрые слоты
                if (slotType == EquipmentSlotType.QuickSlot1 ||
                    slotType == EquipmentSlotType.QuickSlot2) continue;

                BindVisual(item.GetEquipSlot(), item);
            }
            
            // когда всё восстановлено — пересчитываем статы
            RefreshStats();
        }
        

        /// <summary> Надеть предмет, вернуть true если успешно </summary>
        public bool Equip(int slotIndex)
        {
            var slot = Inventory.InventoryProxy.Slots[slotIndex];
            var item = slot.Item.Value;
            if (item == null) return false;

            //
            // if (!Inventory.CanEquip(slot, item))
            //     return false;

            // снимаем старый предмет в слоте
            // var oldItem = Inventory.GetItem(slot);
            // if (oldItem != null)
            //     Unequip(slot);
            //
            // Inventory.SetItem(slot, item);

            RefreshStats();
            BindVisual(item.GetEquipSlot(), item);
            OnEquipped?.Invoke(item.GetEquipSlot(), item);
            
            return true;
        }


        /// <summary> Снять предмет из слота </summary>
        public void Unequip(int slotIndex)
        {
            var equipSlotType = Inventory.GetEquipmentSlotType(slotIndex);
            //Inventory.SetItem(slot, null);
            
            RefreshStats();
            ClearVisual(equipSlotType);
            OnUnequipped?.Invoke(equipSlotType);
        }


        /// <summary> Уменьшить прочность — как в LDOE </summary>
        public void OnItemUsed(int slotIndex, bool isTool)
        {
            var slotProxy = Inventory.InventoryProxy.Slots[slotIndex];
            slotProxy.Durability.Value--;

            if (slotProxy.Durability.Value <= 0)
            {
                HandleItemBroken(slotIndex, isTool);
            }
            
            Inventory.OnChanged?.Invoke();
        }

        /// <summary>
        /// Удаляем предмет из инвентаря после поломки
        /// </summary>
        /// <param name="slotIndex"></param>
        private void HandleItemBroken(int slotIndex, bool isTool)
        {
            var slotProxy = Inventory.InventoryProxy.Slots[slotIndex];
            if (!slotProxy.IsEmpty)
            {
                var item = slotProxy.Item.Value;
                var equipClass = item.GetEquipClass();
            
                // здесь можно вызывать попап о сломанном предмете
                // ...
            
                slotProxy.Clear();
                Unequip(slotIndex);
            

                // ищем замену
                TryAutoEquipReplacement(slotIndex, item, isTool);
                RefreshStats();
            }
        }

        private void TryAutoEquipReplacement(int slotIndex, ItemConfig item, bool isTool)
        {
            (IInventoryContainer inventory, IInventoryContainer equipment) target = GetInventory();

            // находим подходящий предмет
            (bool isEquipment, int? index) foundItem = 
                InventoryAutoEquipService.FindReplacement(target.inventory, target.equipment, item, isTool);
            if (foundItem.index.HasValue)
            {
                // переместить предмет в слот экипировки
                InventoryAutoEquipService.MoveToEquipment(
                    foundItem.isEquipment ? target.equipment : target.inventory,
                    foundItem.index.Value,
                    target.equipment,
                    slotIndex);
                Equip(slotIndex);
            }
        }


        /// <summary>
        /// Вернуть все модификаторы экипированных предметов.
        /// StatRecalculator делает Aggregate().
        /// </summary>
        // public IEnumerable<StatModifier> GetEquippedModifiers()
        // {
        //     var l = Inventory.InventoryProxy.Slots.Count;
        //     for (int i = 0; i < l; i++)
        //     {
        //         var slotType = Inventory.GetSlotType(i);
        //         var kvp= Inventory.InventoryProxy.Slots[i];
        //         if (kvp.Item.Value == null ||
        //             slotType == EquipmentSlotType.QuickSlot1 ||
        //             slotType == EquipmentSlotType.QuickSlot2) continue;
        //
        //         // если прочность 0 → моды не дают бонусов (как в LDOE)
        //         if (kvp.Durability.Value <= 0)
        //             continue;
        //
        //         var statsModifiers = kvp.Item.Value.GetStats();
        //         foreach (var mod in statsModifiers)
        //             yield return mod;
        //     }
        // }


        /// <summary>
        /// Используется для обновления статов после любых изменений снаряжения.
        /// </summary>
        public void RefreshStats()
        {
            //GetComponent<StatsControllerBase>()?.Recalculate();
        }
        
        public void ClearSlots() => Inventory.InventoryProxy.ClearSlots();
        
        protected abstract (IInventoryContainer inventory, IInventoryContainer equipment) GetInventory();

        protected abstract WeaponBuilderBase GetWeaponBuilder();


        // =======================================================================
        // VISUAL BINDING
        // =======================================================================
        
        public void BindVisual(EquipSlotType slot, ItemConfig item)
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

        public void ClearVisual(EquipSlotType slot)
        {
            if (activeModels.TryGetValue(slot, out var model) && model != null)
            {
                Destroy(model);
            }

            activeModels.Remove(slot);
        }

        /// <summary>
        /// вкл\выкл модель
        /// </summary>
        /// <param name="slot"></param>
        public void SwitchVisual(EquipSlotType slot)
        {
            if (activeModels.TryGetValue(slot, out var model))
                model?.SetActive(!model.activeSelf);
        }
        
    }
}
