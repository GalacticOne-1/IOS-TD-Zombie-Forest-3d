
using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Inventory.Rules;
using Galactic1.Core.Enums;
using Galactic1.Code.UI.Inventory;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Inventory.Services
{
    /// <summary>
    /// ЕДИНАЯ точка доступа к слотам инвентаря.
    /// </summary>
    public sealed class InventoryAccessService
    {
        private readonly InventoryGameplayContextService _contextService;
        private readonly InventoryManagementWindow _window;
        public readonly InventoryRulesService _inventoryRules;
        public readonly EquipmentValidationService _equipmentValidation;
        
        public event Action OnPreviewUpdated; // для обновления модели юнита
        
        

        public InventoryAccessService(InventoryGameplayContextService contextService, InventoryManagementWindow window)
        {
            _contextService = contextService;
            _window = window;
            _inventoryRules = new InventoryRulesService();
            _equipmentValidation = new EquipmentValidationService();
        }

        
        
        
        
        
        public void NotifyChanged(IInventorySource source)
        {
            PreviewUpdated();
            source?.NotifyChanged();
        }

        public void PreviewUpdated() => OnPreviewUpdated?.Invoke();

        /// <summary>
        /// Возвращает слоты источника.
        /// </summary>
        public IReadOnlyList<InventorySlotRuntime> GetSlots(IInventorySource source)
            => source.GetSlots();
        public InventorySlotRuntime GetSlot(IInventorySource source, int index)
            => source.GetSlot(index);

        public void SetSlot(IInventorySource source, int index, InventorySlotRuntime slot)
            => source.SetSlot(index, slot);
        
        
        public bool HasItems(IInventorySource source) 
            => GetSlots(source).Any(slot => !slot.IsEmpty);

        public EquipSlotType GetEquipSlotType(IInventorySource source, int slotIndex)
            => source.GetEquipmentSlotType(slotIndex);

        public IReadOnlyDictionary<int, EquipmentSlotType> GetEquipmentSlots(IInventorySource source)
            => source.EquipmentSlots;

        public int? FindSlotIndex(IInventorySource source, EquipmentSlotType requiresType)
            => source.FindSlotIndex(requiresType);

        public bool TryGetSlotType(IInventorySource source, int slotIndex, out EquipmentSlotType slotType)
        {
            slotType = source.GetSlotType(slotIndex) ?? EquipmentSlotType.None;
            return slotType != EquipmentSlotType.None;
        }
        
        
        
        /// <summary>
        /// Проверяет, есть ли место хотя бы для ОДНОЙ единицы предмета.
        /// Возвращает слот и сколько можно положить в него (минимум 1).
        /// </summary>
        public (int slotIndex, int canAccept)? HasFreeSpaceFor(IInventorySource source, ItemConfig item)
        {
            int maxStack = item.Classification.maxStack;
            var slots = GetSlots(source);

            // 1) Ищем неполные стаки (можно положить >=1)
            int i = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty &&
                    slot.Item == item &&
                    slot.Amount < maxStack)
                {
                    int freeSpace = maxStack - slot.Amount;
                    if (freeSpace > 0)
                        return (i, freeSpace);
                }
                i++;
            }

            // 2) Ищем пустые слоты
            i = 0;
            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                {
                    // можно положить весь стак, но нам важно >=1
                    return (i, maxStack);
                }
                i++;
            }

            return null;
        }



        /// <summary>
        /// Добавить предмет в контейнер (поведение может отличаться)
        /// </summary>
        public AddItemResult TryAdd(IInventorySource source, InventorySlotRuntime slotRuntime)
        {
            var slots = GetSlots(source);
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
                    source.SetSlot(i, slot);
                    
                    slotRuntime.Amount -= toAdd;

                    if (slotRuntime.Amount <= 0)
                    {
                        NotifyChanged(source);
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
                    source.SetSlot(i, slot);

                    slotRuntime.Amount -= toAdd;

                    if (slotRuntime.Amount <= 0)
                    {
                        NotifyChanged(source);
                        return new AddItemResult(initialAmount, 0);
                    }
                }
            }

            // ❗ Если дошли сюда — amount > 0 → часть не вошла
            NotifyChanged(source);

            return new AddItemResult(initialAmount - slotRuntime.Amount, slotRuntime.Amount);
        }

        /// <summary>
        /// Удалить предмет
        /// </summary>
        public void RemoveItem(IInventorySource source, ItemConfig item, int amount)
        {
            var slots = GetSlots(source);
            var l = slots.Count;

            for (int i = 0; i < l; i++)
            {
                if (slots[i].Item == item)
                {
                    int remove = Mathf.Min(amount, slots[i].Amount);
                    slots[i].Amount -= remove;
                    amount -= remove;
                    if (slots[i].Amount <= 0)
                        //slots[i].Clear();
                        source.ClearSlot(i);
                    if (amount <= 0)
                        break;
                }
            }

            NotifyChanged(source);
        }


        /// <summary>
        /// Перемещение предмета между источниками.
        /// </summary>
        public bool MoveItem(
            IInventorySource fromSource,
            int fromIndex,
            IInventorySource toSource,
            int toIndex)
        {
            
             ServiceLocator.Current.Get<InventoryManagementWindow>().transferSystem.MoveSlot(
                fromSource,
                fromIndex,
                toSource,
                toIndex);
             
             return true;
        }

        // public void SetSlot(
        //     IInventorySource source,
        //     int index,
        //     InventorySlotProxy slotProxy)
        // {
        //     if (source is InventoryProxySourceAdapter adapter)
        //         adapter.InventoryProxy.SetSlot(index, slotProxy);
        // }

        
    }
}