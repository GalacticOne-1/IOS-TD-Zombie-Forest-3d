
using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Code.UI.Inventory;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using UnityEngine;

namespace Galactic1.Code.Inventory.Sources
{
    /// <summary>
    /// Адаптер, позволяющий использовать существующий InventoryProxy
    /// как Inventory Source без изменения его логики.
    /// </summary>
    public sealed class InventoryProxySourceAdapter : IInventorySource, IInventoryResourcesPort
    {
        public string SourceId { get; }
        public object Owner { get; }
        public InventorySourceType Type { get; }
        public bool IsReadOnly { get; }
        
        public event Action OnChanged;
        public event Action OnChangedPersistent;
        
        
        private readonly InventoryProxy InventoryProxy;
        private readonly List<InventorySlotRuntime> runtimeSlots;
        
        public InventoryDataBase InventoryData { get; }
        public IEquipmentStateListener EquipmentListener { get; }

        public IReadOnlyDictionary<int, EquipmentSlotType> EquipmentSlots => InventoryData.EquipmentSlots;



        public InventoryProxySourceAdapter(
            string sourceId,
            object owner,
            InventoryDataBase inventoryData,
            InventoryProxy proxy,
            InventorySourceType type,
            IEquipmentStateListener listener,
            bool readOnly = false)
        {
            SourceId = sourceId;
            InventoryData = inventoryData;
            InventoryProxy = proxy;
            EquipmentListener = listener;
            Type = type;
            Owner = owner;
            IsReadOnly = readOnly;

            InventoryData.Initialize();
            
            InventoryProxy.OnChanged += () =>
            {
                OnChanged?.Invoke();   // 🔹 пробрасываем наружу
                OnChangedPersistent?.Invoke();
            };

            // === первая инициализация прокси слотов
            // нужно для юнитов, техники и всего остального где инвентарь не зависит от сцены
            // (здания создают прокси через BaseProxy.GetOrCreateInventory !!!)
            if (InventoryProxy.Slots.Count == 0)
            {
                //Debug.LogError($"Source {sourceId}");
                var baseCapacity = InventoryData.BaseCapacity; // можно передавать в конструкторе
                for (int i = 0; i < baseCapacity; i++)
                    InventoryProxy.Slots.Add(new InventorySlotProxy(
                        new InventorySlotData(
                            null,
                            "",
                            0,
                            0,
                            0)));
            }

            runtimeSlots = InventoryRuntimeBuilder.BuildFromProxy(InventoryProxy, inventoryData);
        }
        
        
        
        

        // ================= SLOT API =================

        public void Dispose()
        {
            OnChanged = null;
        }

        public IReadOnlyList<InventorySlotRuntime> GetSlots() => runtimeSlots;

        public InventorySlotRuntime GetSlot(int index) => runtimeSlots[index];

        public void SetSlot(int index, InventorySlotRuntime slot)
        {
            runtimeSlots[index] = slot;

            // ⬇ МГНОВЕННЫЙ SYNC С PROXY
            var p = InventoryProxy.Slots[index];
            p.Item.Value = slot.Item;
            p.Amount.Value = slot.Amount;
            p.Durability.Value = slot.Durability;
            p.AmmoInMagazine.Value = slot.AmmoInMagazine;

            OnChanged?.Invoke();
            OnChangedPersistent?.Invoke();
            
#if UNITY_EDITOR
            DLog.Alert($"Set slot => InventoryProxySourceAdapter [{p.Item.Value}]", 
                EDlogColor.YELLOW, 
                AppConstants.show_log_core);
#endif
        }
        
        public void ClearSlot(int index) 
            => SetSlot(index, new InventorySlotRuntime(null, 0, 0, 0));
        
        
        // ================= META =================
        public void NotifyChanged() => InventoryProxy.NotifyChanged();
        
        public bool HasItems() => InventoryProxy.Slots.Any(slot => !slot.IsEmpty);
        
        public int? FindSlotIndex(EquipmentSlotType requiresType)
            => InventoryData.FindSlotIndex(requiresType);

        public EquipmentSlotType? GetSlotType(int slotIndex)
            => InventoryData.GetSlotType(slotIndex);

        public EquipSlotType GetEquipmentSlotType(int slotIndex)
            => InventoryData.GetEquipmentSlotType(slotIndex);

        // юзать для сравнения предметов
        private static bool IsSameItem(ItemConfig a, ItemConfig b)
            => a != null && b != null && a.Id == b.Id;

        public void SetCapacity(int capacity)
        {
            InventoryProxy.SetCapacity(capacity);
            
            var proxySlots = InventoryProxy.Slots;

            // добавить runtime слоты
            while (runtimeSlots.Count < proxySlots.Count)
            {
                var proxy = proxySlots[runtimeSlots.Count];

                runtimeSlots.Add(new InventorySlotRuntime(
                    proxy.Item.Value,
                    proxy.Amount.Value,
                    proxy.Durability.Value,
                    proxy.AmmoInMagazine.Value));
            }

            // удалить runtime слоты
            while (runtimeSlots.Count > proxySlots.Count)
            {
                runtimeSlots.RemoveAt(runtimeSlots.Count - 1);
            }
            
            OnChanged?.Invoke();
            OnChangedPersistent?.Invoke();
        }
        
        
        
        
        
        // =========================================================
        // QUERY
        // =========================================================
        
        /// <summary>
        /// Быстрая проверка — есть ли непустые слоты за пределами newCapacity.
        /// Не создаёт список, используется для валидации перед сменой вместимости.
        /// </summary>
        public bool HasOverflow(int newCapacity)
        {
            for (int i = newCapacity; i < runtimeSlots.Count; i++)
            {
                if (!runtimeSlots[i].IsEmpty)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Возвращает предметы из слотов за пределами newCapacity.
        /// Используется перед уменьшением вместимости чтобы не потерять предметы.
        /// </summary>
        public List<InventorySlotRuntime> GetOverflowItems(int newCapacity)
        {
            var overflow = new List<InventorySlotRuntime>();

            for (int i = newCapacity; i < runtimeSlots.Count; i++)
            {
                var slot = runtimeSlots[i];
                if (!slot.IsEmpty)
                    overflow.Add(slot);
            }

            return overflow;
        }

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
                if (IsSameItem(slot.Item, slotRuntime.Item) && slot.Amount < maxStack)
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

                if (IsSameItem(slot.Item, slotRuntime.Item) && slot.Amount < maxStack)
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
            var tempSlots = new List<(ItemConfig item, int amount, int maxStack, bool isEmpty)>();

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