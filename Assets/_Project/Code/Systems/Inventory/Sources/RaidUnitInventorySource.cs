using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Code.UI.Inventory;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.Inventory.Sources
{
    /// <summary>
    /// Адаптер, позволяющий UI работать с snapshot-инвентарём,
    /// как будто это обычный InventorySource.
    /// </summary>
    public sealed class RaidUnitInventorySource : IInventorySource, IInventoryResourcesPort
    {
        
        public string SourceId { get; }
        public object Owner { get; }
        public InventorySourceType Type => InventorySourceType.UnitEquipment;
        public bool IsReadOnly { get; }
        
        public event Action OnChanged;
        public event Action OnChangedPersistent;
        
        private readonly InventorySnapshot snapshot;
        public InventoryDataBase InventoryData { get; }
        public IEquipmentStateListener EquipmentListener { get; }
        
        public IReadOnlyDictionary<int, EquipmentSlotType> EquipmentSlots => InventoryData.EquipmentSlots;
        
        
        
        public RaidUnitInventorySource(
            string sourceId,
            object owner,
            InventorySnapshot snapshot,
            InventoryDataBase inventoryData,
            IEquipmentStateListener listener)
        {
            SourceId = sourceId;
            Owner = owner;
            this.snapshot = snapshot;
            InventoryData = inventoryData;
            EquipmentListener = listener;

            InventoryData.Initialize();
        }
        
        
        
        
        // ================= SLOT API =================

        public void Dispose()
        {
            OnChanged = null;
        }
        /// <summary>
        /// Возвращает runtime-слоты для отображения.
        /// </summary>
        public IReadOnlyList<InventorySlotRuntime> GetSlots() => snapshot.Slots;

        public InventorySlotRuntime GetSlot(int index) => snapshot.Slots[index];

        public void SetSlot(int index, InventorySlotRuntime slot)
        {
            snapshot.Slots[index] = slot;
            NotifyChanged();
            // ❗ НЕТ Proxy — изменения живут только в рейде
            
#if UNITY_EDITOR
            DLog.Alert($"Set slot => RaidUnitInventorySource [{slot.Item}]", 
                EDlogColor.YELLOW, 
                AppConstants.show_log_core);
#endif
        }
        
        public void ClearSlot(int index) 
            => SetSlot(index, new InventorySlotRuntime(null, 0, 0, 0));

        
        // ================= META =================

        public void NotifyChanged()
        {
            OnChanged?.Invoke();
            OnChangedPersistent?.Invoke();
        }

        public bool HasItems() => snapshot.Slots.Any(slot => !slot.IsEmpty);
        
        public int? FindSlotIndex(EquipmentSlotType requiresType)
            => InventoryData.FindSlotIndex(requiresType);

        public EquipmentSlotType? GetSlotType(int slotIndex)
            => InventoryData.GetSlotType(slotIndex);


        public EquipSlotType GetEquipmentSlotType(int slotIndex)
            => InventoryData.GetEquipmentSlotType(slotIndex);

        
        
        
        
        // =========================================================
        // QUERY
        // =========================================================

        public bool HasOverflow(int newCapacity) => false;

        public List<InventorySlotRuntime> GetOverflowItems(int newCapacity) => new();

        public int GetTotalAmount(RuntimeId itemId)
        {
            int total = 0;

            var slots = GetSlots();
            var l = slots.Count;
            for (int i = 0; i < l; i++)
            {
                var slot = slots[i];

                if (slot.IsEmpty)
                    continue;

                if (slot.Item.Id == itemId)
                    total += slot.Amount;
            }

            return total;
        }

        // =========================================================
        // CONSUME
        // =========================================================

        public bool TrySpend(RuntimeId itemId, int amount)
        {
            if (amount <= 0)
                return true;

            int available = GetTotalAmount(itemId);
            if (available < amount)
                return false;

            int remaining = amount;
            var slots = GetSlots();
            var l = slots.Count;

            for (int i = 0; i < l && remaining > 0; i++)
            {
                var slot = slots[i];

                if (slot.IsEmpty || slot.Item.Id != itemId)
                    continue;

                if (slot.Amount <= remaining)
                {
                    remaining -= slot.Amount;
                    ClearSlot(i);
                }
                else
                {
                    slot.Amount -= remaining;
                    SetSlot(i, slot);
                    remaining = 0;
                }
            }

            //NotifyChanged(); // единая точка реактивности

            return true;
        }

        // =========================================================
        // ADD
        // =========================================================

        public AddItemResult TryAdd(InventorySlotRuntime slotRuntime)
        {
            var slots = GetSlots();
            int initialAmount = slotRuntime.Amount;
            int maxStack = slotRuntime.Item.Classification.maxStack;

            // 1) Заполняем неполные стаки
            InventorySlotRuntime slot;
            var l = slots.Count;
            for (int i = 0; i < l; i++)
            {
                slot = slots[i];
                if (slot.Item == slotRuntime.Item && slot.Amount < maxStack)
                {
                    int free = maxStack - slot.Amount;
                    int toAdd = Mathf.Min(free, slotRuntime.Amount);

                    slot.Amount += toAdd;
                    SetSlot(i, slot);
                    
                    slotRuntime.Amount -= toAdd;

                    if (slotRuntime.Amount <= 0)
                    {
                        //NotifyChanged();
                        return new AddItemResult(initialAmount, 0);
                    }
                }
            }

            // 2) Используем пустые слоты
            for (int i = 0; i < l; i++)
            {
                slot = slots[i];
                if (slot.IsEmpty)
                {
                    slot.Item = slotRuntime.Item;
                    slot.Durability = slotRuntime.Durability;

                    int toAdd = Mathf.Min(slotRuntime.Amount, maxStack);
                    slot.Amount = toAdd;
                    SetSlot(i, slot);

                    slotRuntime.Amount -= toAdd;

                    if (slotRuntime.Amount <= 0)
                    {
                        //NotifyChanged();
                        return new AddItemResult(initialAmount, 0);
                    }
                }
            }

            // ❗ Если дошли сюда — amount > 0 → часть не вошла
            //NotifyChanged();

            return new AddItemResult(initialAmount - slotRuntime.Amount, slotRuntime.Amount);
        }
        
        public bool CanAdd(InventorySlotRuntime slotRuntime)
        {
            var slots = GetSlots();
            int amountToAdd = slotRuntime.Amount;
            int maxStack = slotRuntime.Item.Classification.maxStack;

            int l = slots.Count;

            // 1) Считаем свободное место в неполных стаках
            for (int i = 0; i < l; i++)
            {
                var slot = slots[i];

                if (slot.Item == slotRuntime.Item && slot.Amount < maxStack)
                {
                    int free = maxStack - slot.Amount;
                    amountToAdd -= free;

                    if (amountToAdd <= 0)
                        return true;
                }
            }

            // 2) Считаем пустые слоты
            for (int i = 0; i < l; i++)
            {
                var slot = slots[i];

                if (slot.IsEmpty)
                {
                    amountToAdd -= maxStack;

                    if (amountToAdd <= 0)
                        return true;
                }
            }

            return false;
        }
        
        public bool CanAddMultiple(IEnumerable<InventorySlotRuntime> slotsToAdd)
        {
            var slots = GetSlots();

            // --- Копируем текущее состояние (только Amount и Item)
            var tempSlots = new List<(object item, int amount, int maxStack, bool isEmpty)>();

            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                {
                    tempSlots.Add((null, 0, 0, true));
                }
                else
                {
                    tempSlots.Add((
                        slot.Item,
                        slot.Amount,
                        slot.Item.Classification.maxStack,
                        false
                    ));
                }
            }

            // --- Пробуем виртуально добавить каждый слот
            foreach (var addSlot in slotsToAdd)
            {
                int amountToAdd = addSlot.Amount;
                var item = addSlot.Item;
                int maxStack = item.Classification.maxStack;

                // 1) Заполняем неполные стаки
                for (int i = 0; i < tempSlots.Count; i++)
                {
                    var s = tempSlots[i];

                    if (!s.isEmpty && s.item == item && s.amount < maxStack)
                    {
                        int free = maxStack - s.amount;
                        int toAdd = Mathf.Min(free, amountToAdd);

                        tempSlots[i] = (s.item, s.amount + toAdd, s.maxStack, false);
                        amountToAdd -= toAdd;

                        if (amountToAdd <= 0)
                            break;
                    }
                }

                // 2) Используем пустые слоты
                for (int i = 0; i < tempSlots.Count && amountToAdd > 0; i++)
                {
                    var s = tempSlots[i];

                    if (s.isEmpty)
                    {
                        int toAdd = Mathf.Min(amountToAdd, maxStack);

                        tempSlots[i] = (item, toAdd, maxStack, false);
                        amountToAdd -= toAdd;
                    }
                }

                // Если после двух фаз что-то осталось — не влезает
                if (amountToAdd > 0)
                    return false;
            }

            return true;
        }
    }
}