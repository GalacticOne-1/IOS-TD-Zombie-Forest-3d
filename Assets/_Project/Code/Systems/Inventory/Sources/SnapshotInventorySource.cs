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
    /// Универсальный источник инвентаря работающий поверх InventorySnapshot.
    /// Используется для рейдов, буферов, симуляций.
    /// </summary>
    public class SnapshotInventorySource : IInventorySource, IInventoryResourcesPort
    {
        public string SourceId { get; }
        public object Owner { get; }
        public InventorySourceType Type { get; }
        public bool IsReadOnly { get; }

        public event Action OnChanged;
        public event Action OnChangedPersistent;

        protected readonly InventorySnapshot snapshot;

        public InventoryDataBase InventoryData { get; }
        public IEquipmentStateListener EquipmentListener { get; }

        public IReadOnlyDictionary<int, EquipmentSlotType> EquipmentSlots => InventoryData.EquipmentSlots;

        public SnapshotInventorySource(
            string sourceId,
            object owner,
            InventorySnapshot snapshot,
            InventoryDataBase inventoryData,
            InventorySourceType type,
            IEquipmentStateListener listener = null)
        {
            SourceId = sourceId;
            Owner = owner;
            this.snapshot = snapshot;
            InventoryData = inventoryData;
            Type = type;
            EquipmentListener = listener;

            InventoryData.Initialize();
        }

        // ================= SLOT API =================

        public void Dispose()
        {
            OnChanged = null;
        }
        public IReadOnlyList<InventorySlotRuntime> GetSlots() => snapshot.Slots;

        public InventorySlotRuntime GetSlot(int index) => snapshot.Slots[index];

        public void SetSlot(int index, InventorySlotRuntime slot)
        {
            snapshot.Slots[index] = slot;
            NotifyChanged();
        }

        public void ClearSlot(int index)
            => SetSlot(index, new InventorySlotRuntime(null, 0, 0, 0));

        // ================= META =================

        public void NotifyChanged() 
        {
            OnChanged?.Invoke();
            OnChangedPersistent?.Invoke();
        }

        public bool HasItems() => snapshot.Slots.Any(s => !s.IsEmpty);

        public int? FindSlotIndex(EquipmentSlotType requiresType)
            => InventoryData.FindSlotIndex(requiresType);

        public EquipmentSlotType? GetSlotType(int slotIndex)
            => InventoryData.GetSlotType(slotIndex);

        public EquipSlotType GetEquipmentSlotType(int slotIndex)
            => InventoryData.GetEquipmentSlotType(slotIndex);

        // ================= QUERY =================

        public bool HasOverflow(int newCapacity) => false;

        public List<InventorySlotRuntime> GetOverflowItems(int newCapacity) => new();

        public int GetTotalAmount(RuntimeId itemId)
        {
            int total = 0;

            foreach (var slot in snapshot.Slots)
            {
                if (!slot.IsEmpty && slot.Item.Id == itemId)
                    total += slot.Amount;
            }

            return total;
        }

        // ================= CONSUME =================

        public bool TrySpend(RuntimeId itemId, int amount)
        {
            if (amount <= 0)
                return true;

            int available = GetTotalAmount(itemId);
            if (available < amount)
                return false;

            int remaining = amount;

            for (int i = 0; i < snapshot.Slots.Count && remaining > 0; i++)
            {
                var slot = snapshot.Slots[i];

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

            return true;
        }

        // ================= ADD =================

        public AddItemResult TryAdd(InventorySlotRuntime slotRuntime)
        {
            var slots = snapshot.Slots;
            int initialAmount = slotRuntime.Amount;
            int maxStack = slotRuntime.Item.Classification.maxStack;

            // fill stacks
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];

                if (slot.Item == slotRuntime.Item && slot.Amount < maxStack)
                {
                    int free = maxStack - slot.Amount;
                    int toAdd = Mathf.Min(free, slotRuntime.Amount);

                    slot.Amount += toAdd;
                    SetSlot(i, slot);

                    slotRuntime.Amount -= toAdd;

                    if (slotRuntime.Amount <= 0)
                        return new AddItemResult(initialAmount, 0);
                }
            }

            // empty slots
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];

                if (slot.IsEmpty)
                {
                    int toAdd = Mathf.Min(slotRuntime.Amount, maxStack);

                    slot.Item = slotRuntime.Item;
                    slot.Durability = slotRuntime.Durability;
                    slot.Amount = toAdd;

                    SetSlot(i, slot);

                    slotRuntime.Amount -= toAdd;

                    if (slotRuntime.Amount <= 0)
                        return new AddItemResult(initialAmount, 0);
                }
            }

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