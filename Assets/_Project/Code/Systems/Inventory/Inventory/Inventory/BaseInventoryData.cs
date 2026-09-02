using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;
using Galactic1.Items;
using Object = UnityEngine.Object;

namespace Galactic1.Code.UI.Inventory
{
    public abstract class BaseInventoryData : ScriptableObject
    {
        public int baseCapacity = 10;

        //public List<InventorySlot> slots = new();
        public InventoryProxy InventoryProxy; // ссылка на runtime
        public Action OnChanged;

        /// <summary>
        /// Инициализация контейнера (создание слотов)
        /// </summary>
        public abstract void Initialize(Object data = null);

        public bool HaveItems() => InventoryProxy.Slots.Any(slot => !slot.IsEmpty);
        
        public virtual int? FindSlotIndex(EquipmentSlotType requiresType) => null;
        public virtual EquipmentSlotType GetSlotType(int slotIndex) => EquipmentSlotType.None;
        public virtual EquipSlotType GetEquipmentSlotType(int slotIndex) => EquipSlotType.None;







        /// <summary>
        /// Проверяет, есть ли место хотя бы для ОДНОЙ единицы предмета.
        /// Возвращает слот и сколько можно положить в него (минимум 1).
        /// </summary>
        public (int slotIndex, int canAccept)? HasFreeSpaceFor(ItemConfig item)
        {
            int maxStack = item.Classification.maxStack;

            // 1) Ищем неполные стаки (можно положить >=1)
            for (int i = 0; i < InventoryProxy.Slots.Count; i++)
            {
                var slot = InventoryProxy.Slots[i];

                if (!slot.IsEmpty &&
                    slot.Item.Value == item &&
                    slot.Amount.Value < maxStack)
                {
                    int freeSpace = maxStack - slot.Amount.Value;
                    if (freeSpace > 0)
                        return (i, freeSpace);
                }
            }

            // 2) Ищем пустые слоты
            for (int i = 0; i < InventoryProxy.Slots.Count; i++)
            {
                var slot = InventoryProxy.Slots[i];

                if (slot.IsEmpty)
                {
                    // можно положить весь стак, но нам важно >=1
                    return (i, maxStack);
                }
            }

            return null;
        }



        /// <summary>
        /// Добавить предмет в контейнер (поведение может отличаться)
        /// </summary>
        public virtual AddItemResult TryAdd(ItemConfig item, int amount)
        {
            int initialAmount = amount;
            int maxStack = item.Classification.maxStack;

            // 1) Заполняем неполные стаки
            foreach (var slot in InventoryProxy.Slots)
            {
                if (slot.Item.Value == item && slot.Amount.Value < maxStack)
                {
                    int free = maxStack - slot.Amount.Value;
                    int toAdd = Mathf.Min(free, amount);

                    slot.Amount.Value += toAdd;
                    amount -= toAdd;

                    if (amount <= 0)
                    {
                        OnChanged?.Invoke();
                        return new AddItemResult(initialAmount, 0);
                    }
                }
            }

            // 2) Используем пустые слоты
            foreach (var slot in InventoryProxy.Slots)
            {
                if (slot.IsEmpty)
                {
                    slot.Item.Value = item;
                    slot.Durability.Value = item.Physical.maxDurability;

                    int toAdd = Mathf.Min(amount, maxStack);
                    slot.Amount.Value = toAdd;

                    amount -= toAdd;

                    if (amount <= 0)
                    {
                        OnChanged?.Invoke();
                        return new AddItemResult(initialAmount, 0);
                    }
                }
            }

            // ❗ Если дошли сюда — amount > 0 → часть не вошла
            OnChanged?.Invoke();

            int added = initialAmount - amount;
            return new AddItemResult(added, amount);
        }


        /// <summary>
        /// Удалить предмет
        /// </summary>
        public virtual void RemoveItem(ItemConfig item, int amount)
        {
            foreach (var slot in InventoryProxy.Slots)
            {
                if (slot.Item.Value == item)
                {
                    int remove = Mathf.Min(amount, slot.Amount.Value);
                    slot.Amount.Value -= remove;
                    amount -= remove;
                    if (slot.Amount.Value <= 0)
                        slot.Clear();
                    if (amount <= 0)
                        break;
                }
            }

            OnChanged?.Invoke();
        }
    }
}