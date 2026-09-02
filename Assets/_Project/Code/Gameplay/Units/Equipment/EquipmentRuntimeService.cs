using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Equipment.Snapshots;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Meta.Stats;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Equipment
{
    /// <summary>
    /// Геймплейная логика экипировки (НЕ MonoBehaviour).
    /// Работает поверх IInventorySource и не зависит от Proxy/Snapshot.
    ///
    /// Отвечает за:
    /// • определение экипированных предметов
    /// • модификаторы статов
    /// • урон прочности
    /// • авто-снятие при 0 durability
    /// • события экипировки
    /// </summary>
    public sealed class EquipmentRuntimeService : IEquipmentStatsProvider
    {
        public IInventorySource Source { get; private set; }

        public event Action<EquipSlotType, ItemConfig> OnEquipped;
        public event Action<EquipSlotType> OnUnequipped;
        public event Action OnClearAll; 
        public event Action<ItemConfig> OnItemBroken;
        public event Action OnPreviewUpdate; 
        public event Action OnUpdate;

        
        public void BindSource(IInventorySource source)
        {
            Source = source;
        }
        
        
        
        // =========================================================
        // EQUIPPED STATE
        // =========================================================
        public void RestoreEquipmentFromInventory()
        {
            // удаляем старые модели
            OnClearAll?.Invoke();
            /*foreach (var kvp in activeModels)
                if (kvp.Value)
                    Destroy(kvp.Value);
            activeModels.Clear();*/
            

            // создаём модели для каждого слота
            var slots = Source.GetSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                var item = slots[i].Item;
                if (item == null) continue;

                var slotType = Source.GetSlotType(i);

                // не спавним быстрые слоты
                if (slotType == EquipmentSlotType.QuickSlot1 ||
                    slotType == EquipmentSlotType.QuickSlot2) continue;

                //BindVisual(item.Config.EquipSlotType, item);
                OnEquipped?.Invoke(item.GetEquipSlot(), item);
            }

            // когда всё восстановлено — пересчитываем статы
            RefreshStats();
        }


        // =========================================================
        // EQUIP / UNEQUIP
        // =========================================================


        /// <summary> Надеть предмет, вернуть true если успешно </summary>
        public bool Equip(int slotIndex)
        {
            var slot = Source.GetSlot(slotIndex);
            var item = slot.Item;
            if (item == null) 
                return false;

            //
            // if (!Inventory.CanEquip(slot, item))
            //     return false;

            // снимаем старый предмет в слоте
            // var oldItem = Inventory.GetItem(slot);
            // if (oldItem != null)
            //     Unequip(slot);
            //
            // Inventory.SetItem(slot, item);

            //DebugInputService.I.On(KeyCode.F2, () => OnItemBroken(item));
            RefreshStats();
            //BindVisual(item.EquipSlotType, item);
            OnEquipped?.Invoke(item.GetEquipSlot(), item);

            return true;
        }


        /// <summary> Снять предмет из слота </summary>
        public void Unequip(int slotIndex)
        {
            var equipSlotType = Source.GetEquipmentSlotType(slotIndex);
            //Inventory.SetItem(slot, null);

            RefreshStats();
            //ClearVisual(equipSlotType);
            OnUnequipped?.Invoke(equipSlotType);
        }


        // =========================================================
        // DURABILITY
        // =========================================================
        

        /// <summary> Уменьшить прочность — как в LDOE </summary>
        public void OnItemUsed(int slotIndex, int newDurability)
        {
            var slotProxy = Source.GetSlot(slotIndex);
            if (slotProxy.IsEmpty) 
                return;

            slotProxy.Durability = newDurability;

            if (slotProxy.Durability <= 0)
            {
#if UNITY_EDITOR
                DLog.Alert($"Weapon is broke", EDlogColor.ORANGE);
#endif
                HandleItemBroken(slotIndex, false);
                return;
            }

#if UNITY_EDITOR
            DLog.Alert($"Weapon durability: {slotProxy.Durability}", EDlogColor.YELLOW);
#endif

            Source.SetSlot(slotIndex, slotProxy);
            Source.NotifyChanged();
        }

        /// <summary>
        /// Удаляем предмет из инвентаря после поломки
        /// </summary>
        /// <param name="slotIndex"></param>
        private void HandleItemBroken(int slotIndex, bool isTool)
        {
            var slotProxy = Source.GetSlot(slotIndex);
            if (!slotProxy.IsEmpty)
            {
                var item = slotProxy.Item;
                var equipClass = item.GetEquipClass();
            
                
                Unequip(slotIndex);
                Source.ClearSlot(slotIndex);
                
                // ищем замену
                TryAutoEquipReplacement(slotIndex, item, isTool);
                RefreshStats();
                
                OnItemBroken?.Invoke(item);
                OnPreviewUpdate?.Invoke();
            }
        }
        
        /// <summary>
        /// Урон для снаряжения
        /// </summary>
        /// <param name="damage"></param>
        public void ApplyDurabilityDamage(float damage)
        {
            var slots = Source.GetSlots();

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.IsEmpty)
                    continue;
                
                var slotType = Source.GetSlotType(i);

                if (!IsArmor(slotType.Value))
                    continue;
                
                int loss = CalculateDurabilityLoss(damage, slotType.Value);

                if (loss <= 0)
                    continue;


                OnItemUsed(i, slot.Durability - loss);
            }
        }
        
        private int CalculateDurabilityLoss(float damage, EquipmentSlotType slot)
        {
            // базовый коэффициент (регулируется)
            float k = 0.04f;

            float slotFactor = slot switch
            {
                EquipmentSlotType.Body => 1.0f,
                EquipmentSlotType.Head => 0.7f,
                EquipmentSlotType.Legs => 0.5f,
                _ => 0.3f
            };

            float raw = damage * k * slotFactor;

            return Mathf.Max(1, Mathf.FloorToInt(raw));
        }

        private void TryAutoEquipReplacement(int slotIndex, ItemConfig item, bool isTool)
        {
            /*(IInventoryContainer inventory, IInventoryContainer equipment) target = GetInventory();

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
            }*/
        }

        // =========================================================
        // STAT MODIFIERS
        // =========================================================

        /// <summary>
        /// Вернуть все модификаторы экипированных предметов.
        /// StatRecalculator делает Aggregate().
        /// </summary>
        public IReadOnlyList<ItemStatEntry> GetEquippedModifiers()
        {
            var stats = new List<ItemStatEntry>();
            var slots = Source.GetSlots();
            var l = slots.Count;
            for (int i = 0; i < l; i++)
            {
                var slotType = Source.GetSlotType(i);
                var kvp = slots[i];
                if (kvp.Item == null ||
                    slotType == EquipmentSlotType.QuickSlot1 ||
                    slotType == EquipmentSlotType.QuickSlot2) continue;

                // если прочность 0 → моды не дают бонусов (как в LDOE)
                if (kvp.Durability <= 0)
                    continue;

                var statsModifiers = kvp.Item.GetStats();
                foreach (var mod in statsModifiers)
                    stats.Add(mod);
            }

            return stats;
        }


        /// <summary>
        /// Используется для обновления статов после любых изменений снаряжения.
        /// </summary>
        public void RefreshStats()
        {
            OnUpdate?.Invoke();
            //GetComponent<StatsControllerBase>()?.Recalculate();
        }


        private bool IsArmor(EquipmentSlotType type)
        {
            return type == EquipmentSlotType.Head ||
                   type == EquipmentSlotType.Body ||
                   type == EquipmentSlotType.Legs;
        }

        #region Shanpshot

        /// <summary>
        /// Восстанавливает экипировку из snapshot.
        /// Используется в RaidUnitRuntime.
        /// </summary>
        public void RestoreFromSnapshot(EquipmentSnapshot snapshot)
        {
            OnClearAll?.Invoke();

            foreach (var kvp in snapshot.Items)
            {
                var slotType = kvp.Key;
                var itemSnapshot = kvp.Value;

                // визуал / события
                OnEquipped?.Invoke(slotType, itemSnapshot.Item);
            }

            RefreshStats();
        }
        
        /// <summary>
        /// Создаёт readonly-снимок экипировки.
        /// Используется при входе в рейд.
        /// </summary>
        public EquipmentSnapshot CreateReadonlySnapshot()
        {
            var dict = new Dictionary<EquipSlotType, EquipmentItemSnapshot>();

            var slots = Source.GetSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.Item == null)
                    continue;

                var slotType = Source.GetSlotType(i);
                if (slotType == EquipmentSlotType.QuickSlot1 ||
                    slotType == EquipmentSlotType.QuickSlot2)
                    continue;

                dict[slot.Item.GetEquipSlot()] =
                    new EquipmentItemSnapshot(slot.Item, slot.Durability, slot.AmmoInMagazine);
            }

            return new EquipmentSnapshot(dict);
        }
        
        

        #endregion
    }
}
