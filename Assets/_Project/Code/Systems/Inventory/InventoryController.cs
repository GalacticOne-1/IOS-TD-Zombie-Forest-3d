using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.Weapons.Services;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// Контроллер логики инвентаря.
    /// Управляет действиями над слотами (удаление, разделение, сортировка, перемещение).
    /// Вызывается из UI, но не содержит визуальной логики.
    /// </summary>
    public class InventoryController
    {
        private readonly InventoryManagementWindow invWindow;
        private readonly InventoryTransferSystem transferSystem;

        public readonly InventoryGameplayContextService ContextService;
        public readonly GameLoopContext GameLoopContext;
        public readonly InventoryAccessService AccessService;
        public readonly WeaponReloadService WeaponReloadService;


        public IInventorySource LeftSource => transferSystem.LeftSource;
        public IInventorySource RightSource => transferSystem.RightSource;


        public InventoryController(
            InventoryManagementWindow invWindow, 
            InventoryTransferSystem transferSystem,
            InventoryAccessService accessService,
            InventoryGameplayContextService contextService, 
            GameLoopContext gameLoopContext)
        {
            this.invWindow = invWindow;
            this.transferSystem = transferSystem;
            ContextService = contextService;
            GameLoopContext = gameLoopContext;
            AccessService = accessService;
            
            WeaponReloadService = new WeaponReloadService(this, GameContent.Ammo);
        }

        // -------------------------------
        // 🔹 УДАЛЕНИЕ ПРЕДМЕТА
        // -------------------------------
        public void RemoveItem(InventoryView view)
        {
            if (view == null || view.selectedSlot == null)
            {
                Debug.Log("❌ Нет выбранного слота для удаления");
                return;
            }

            var source = view._source;
            //var slot = _accessService.GetSlot(source, view.selectedSlot.SlotIndex);
            var slot = source.GetSlot(view.selectedSlot.SlotIndex);

            if (slot.IsEmpty)
            {
                view.ClearSelection();
                return;
            }

            // 🧱 Проверка — если удаляем из экипировки
            if (AccessService._inventoryRules.IsEquipmentSource(source))
            {
                // var config = slot.Item.Value?.Config;
                // if (config != null && config.EquipSlotType == ItemEquipSlotType.Bag)
                // {
                //     var leftInventory = LeftSource.Inventory as CharacterInventoryData;
                //     if (!equipInv.CanRemoveBag(leftInventory, view.selectedSlot.SlotIndex))
                //     {
                //         Debug.LogWarning("⚠️ Сначала очистите сумку перед снятием!");
                //         return;
                //     }
                //
                //     // Удаляем сумку из ячейки
                //     slot.Clear();
                //
                //     // 🔹 После удаления сумки нужно обновить доступные слоты игрока
                //     equipInv.HandleBagChange(view.selectedSlot.SlotIndex,
                //         new InventorySlot(config.Item, 1, config.Item.Config.Durability), // старый слот (для события)
                //         new InventorySlot(null, 0, 0), // новый слот
                //         leftInventory);
                //
                //     leftInventory.OnChanged?.Invoke();
                //      _accessService.NotifyChanged(targetSource);
                // }
                
                // *** остальные предметы из снаряги
                //else
                //{
                    // Удаляем полностью предмет из ячейки
                    source.ClearSlot(view.selectedSlot.SlotIndex);
                    source.EquipmentListener.Unequip(view.selectedSlot.SlotIndex);
                //}
            }
            
            // *** левый инвентарь
            else
            {
                // Удаляем полностью предмет из ячейки
                //slot.Clear();
                source.ClearSlot(view.selectedSlot.SlotIndex);
            }


            // Обновляем UI
            //inventory.OnChanged?.Invoke();
            AccessService.NotifyChanged(source);

            // Сбрасываем выделение
            view.ClearSelection();
        }


        // -------------------------------
        // 🔹 РАЗДЕЛЕНИЕ СТЕКА
        // -------------------------------
        public void SplitStack(InventoryView view, bool takeOne = false)
        {
            if (view == null || view.selectedSlot == null)
            {
                Debug.Log("❌ Нет выбранного слота для разделения");
                return;
            }

            var invWindow = view.Window;
            if (invWindow == null)
                return;

            var controller = invWindow.controller;

            var fromIndex = view.selectedSlot.SlotIndex;
            var fromSource = view._source;
            var fromSlot = AccessService.GetSlot(fromSource, fromIndex);

            // Предмет должен быть делимым
            if (fromSlot.IsEmpty || fromSlot.Amount < 2)
            {
                view.ClearSelection();
                return;
            }

            // Определяем — экипировка ли это
            // bool isEquipment = inventory is CharacterEquipmentInventoryData;
            //
            // // Куда складываем вторую половину
            // InventoryDataBase targetInventory = inventory;
            //
            // // Если экипировка → целиться в левый (обычный) инвентарь
            // if (isEquipment)
            // {
            //     targetInventory = controller.LeftSource.Inventory;
            // }
            
            var targetSource = fromSource.Type is InventorySourceType.UnitEquipment or InventorySourceType.TransportEquipment
                ? LeftSource
                : fromSource;

            var targetSlots = AccessService.GetSlots(targetSource);

            // Ищем пустой слот в целевом инвентаре
            // var emptyIndex = targetInventory.InventoryProxy.Slots
            //     .Select((slot, index) => new { slot, index })
            //     .FirstOrDefault(x => x.slot.IsEmpty)?.index ?? -1;
            var emptyIndex = targetSlots
                .Select((slot, index) => new { slot, index })
                .FirstOrDefault(x => x.slot.IsEmpty)?.index ?? -1;

            if (emptyIndex == -1)
            {
                Debug.Log("❌ Нет свободного слота для разделения!");
                view.ClearSelection();
                return;
            }

            // Делим
            int amount = takeOne ? 1 : fromSlot.Amount / 2;
            fromSlot.Amount -= amount;

            // Клонируем новую половину
            // targetInventory.InventoryProxy.SetSlot(emptyIndex,
            //     new InventorySlotProxy(new InventorySlotData(fromSlot.Item.Value, "", amount, fromSlot.Durability.Value)));
            AccessService.SetSlot(targetSource, emptyIndex,
                new InventorySlotRuntime(fromSlot.Item, amount, fromSlot.Durability, fromSlot.AmmoInMagazine));


            // Обновляем инвентари
            // inventory.OnChanged?.Invoke();
            // if (targetInventory != inventory)
            //     targetInventory.OnChanged?.Invoke();
            AccessService.NotifyChanged(targetSource);

            // Сбрасываем выделение
            //ui.ClearSelection();
            view.selectedSlot.SetHighlight(!fromSlot.IsEmpty);
            invWindow.UpdateButtons();
        }



        // -------------------------------
        // 🔹 СОРТИРОВКА
        // -------------------------------
        // что бы сортировка работала нужно явно выбрать предмет в нужном инвентаре !!!
        public void SortInventory(InventoryView view)
        {
            if (view == null || view._source == null) return;
            view.ClearSelection();

            var invList = AccessService.GetSlots(view._source);

            // 1️⃣ Собираем одинаковые предметы в стеки
            for (int i = 0; i < invList.Count; i++)
            {
                var slot = invList[i];
                if (slot.IsEmpty) continue;

                for (int j = i + 1; j < invList.Count; j++)
                {
                    var other = invList[j];
                    if (other.IsEmpty) continue;
                    if (other.Item != slot.Item) continue;

                    int total = slot.Amount + other.Amount;
                    int max = slot.Item.Classification.maxStack;

                    if (total <= max)
                    {
                        slot.Amount = total;
                        //other.Clear();
                        view._source.ClearSlot(j);
                    }
                    else
                    {
                        slot.Amount = max;
                        other.Amount = total - max;
                    }
                }
            }

            // 2️⃣ Сортируем по категории и приоритету
            // Создаем обычный список
            var sorted = invList.ToList();
            sorted.Sort((a, b) =>
            {
                if (a.IsEmpty && b.IsEmpty) return 0;
                if (a.IsEmpty) return 1;
                if (b.IsEmpty) return -1;

                int catCompare = a.Item.Classification.sortCategory.CompareTo(b.Item.Classification.sortCategory);
                if (catCompare != 0) return catCompare;

                return a.Item.Classification.sortPriority.CompareTo(b.Item.Classification.sortPriority);
            });
            
            // Переставляем элементы inplace
            for (int i = 0; i < sorted.Count; i++)
                //invList[i] = sorted[i];  // вызовет ObserveReplace, подписка сохранится
                AccessService.SetSlot(view._source, i, sorted[i]);

            // 3️⃣ Компактируем одинаковые предметы подряд
            var compacted = new List<InventorySlotRuntime>();
            var used = new HashSet<InventorySlotRuntime>();

            foreach (var slot in invList)
            {
                if (slot.IsEmpty || used.Contains(slot)) continue;

                compacted.Add(slot);
                used.Add(slot);

                foreach (var other in invList)
                {
                    if (other == slot || other.IsEmpty) continue;
                    if (other.Item != slot.Item) continue;

                    compacted.Add(other);
                    used.Add(other);
                }
            }

            // 4️⃣ Добавляем пустые слоты в конец
            int emptyCount = invList.Count - compacted.Count;
            compacted.AddRange(invList.Where(s => s.IsEmpty).Take(emptyCount));

            // 5️⃣ Переставляем элементы inplace
            for (int i = 0; i < compacted.Count; i++)
                //invList[i] = compacted[i];
                AccessService.SetSlot(view._source, i, compacted[i]);

            AccessService.NotifyChanged(view._source);
        }



        // -------------------------------
        // 🔹 ПЕРЕНОС ПРЕДМЕТА МЕЖДУ ИНВЕНТАРЯМИ
        // -------------------------------
        public void MoveItem(IInventorySource from, int fromIndex, IInventorySource to, int toIndex)
        {
            var result = AccessService.MoveItem(from, fromIndex, to, toIndex);
            invWindow.UpdateButtons();
        }

        public void HandleDoubleClick(IInventorySource source, int fromIndex)
        {
            transferSystem.HandleDoubleClick(source, fromIndex);
            invWindow.UpdateButtons();
        }
        
        
        public void HandleEquip(IInventorySource source, int index)
        {
            var slots = AccessService.GetSlots(source);
            var item = slots[index].Item;
            if (item == null) 
                return;
            
            var equipType = item.GetEquipSlot();
            if (equipType == EquipSlotType.None)
                return;

            var equipSource = RightSource; // правая сторона — экипировка
            int? targetSlot = InventoryRules.FindMatchingEquipmentSlot(
                equipSource,
                equipType,
                AccessService,
                item);

            if (!targetSlot.HasValue)
            {
                DLog.Alert("Нет подходящего слота!", EDlogColor.RED);
                return;
            }

            AccessService.MoveItem(source, index, equipSource, targetSlot.Value);
            invWindow.UpdateButtons();
        }


        public void MoveAllLeftToRight()
        {
            if (LeftSource == null || RightSource == null) return;

            SmartMoveAllItems(LeftSource, RightSource);
            invWindow.UpdateButtons();
        }

        public void MoveAllRightToLeft()
        {
            if (LeftSource == null || RightSource == null) return;

            SmartMoveAllItems(RightSource, LeftSource);
            invWindow.UpdateButtons();
        }

        // Общий метод для переноса всех предметов
        private void SmartMoveAllItems(IInventorySource fromSource, IInventorySource toSource)
        {
            invWindow.ClearAllSelections();
        
            // 1️⃣ Пробегаем все слоты исходного инвентаря
            for (int i = 0; i < fromSource.GetSlots().Count; i++)
            {
                var slot = fromSource.GetSlot(i);
                if (slot.IsEmpty) continue;
        
                // 2️⃣ Сначала пытаемся добавить предмет в существующие стеки
                int remaining = slot.Amount;
                for (int j = 0; j < toSource.GetSlots().Count; j++)
                {
                    var targetSlot = toSource.GetSlot(j);
                    if (targetSlot.IsEmpty) continue;
                    if (targetSlot.Item != slot.Item) continue;
        
                    int maxStack = slot.Item.Classification.maxStack;
                    int canAdd = maxStack - targetSlot.Amount;
                    if (canAdd <= 0) continue;
        
                    int toTransfer = Mathf.Min(remaining, canAdd);
                    targetSlot.Amount += toTransfer;
                    remaining -= toTransfer;
        
                    if (remaining <= 0) break;
                }
        
                // 3️⃣ Если что-то осталось — ищем пустой слот
                while (remaining > 0)
                {
                    //int emptyIndex = toInv.InventoryProxy.Inventory.FindIndex(s => s.IsEmpty);
                    var emptyIndex = toSource.GetSlots()
                        .Select((slot, index) => new { slot, index })
                        .FirstOrDefault(x => x.slot.IsEmpty)?.index ?? -1;
                    if (emptyIndex == -1) break; // больше нет места
        
                    int toTransfer = Mathf.Min(remaining, slot.Item.Classification.maxStack);
                    toSource.SetSlot(emptyIndex,
                        new InventorySlotRuntime(slot.Item,  toTransfer, slot.Durability, slot.AmmoInMagazine));
                    remaining -= toTransfer;
                }
        
                // 4️⃣ Очищаем исходный слот
                if (remaining <= 0)
                    //slot.Clear();
                fromSource.ClearSlot(i);
            }
        
            // 5️⃣ Обновляем UI
            AccessService.NotifyChanged(fromSource);
            AccessService.NotifyChanged(toSource);
        }

        

    }

}