
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{

    public class InventoryTransferSystem
    {
        public InventoryManagementWindow invWindow;
        private readonly InventoryAccessService _access;
        
        public IInventorySource LeftSource { get; private set; }
        public IInventorySource RightSource { get; private set; }


        public InventoryTransferSystem(
            InventoryManagementWindow invWindow,
            InventoryAccessService access)
        {
            this.invWindow = invWindow;
            _access = access;
        }




        public void OpenTransfer(IInventorySource leftSource, IInventorySource rightSource)
        {
            LeftSource = leftSource;
            RightSource = rightSource;
        }

        public void MoveSlot(
            IInventorySource fromSource,
            int fromIndex,
            IInventorySource toSource,
            int toIndex)
        {
            InventoryDataBase fromInventory = fromSource.InventoryData;
            InventoryDataBase toInventory = toSource.InventoryData;
            var fromSlot = fromSource.GetSlot(fromIndex);
            var toSlot = toSource.GetSlot(toIndex);

            if (fromSlot.IsEmpty) 
                return;

            bool _fromEquipment = _access._inventoryRules.IsEquipmentSource(fromSource);
            bool _toEquipment = _access._inventoryRules.IsEquipmentSource(toSource);
            
            InventoryDataBase fromEquip = fromInventory;
            InventoryDataBase toEquip = toInventory;

            EquipmentSlotType slotType = EquipmentSlotType.None; // default
            EquipmentSlotType fromType = EquipmentSlotType.None;


            // Если переносим из экипировки, получаем слот и тип
            if (_fromEquipment)
            {
                //fromEquip = fe;
                //fromEquip.slotTypes.TryGetValue(fromIndex, out fromType);
                _access.TryGetSlotType(fromSource, fromIndex, out fromType);
            }

            // 🎯 Проверка допустимости
            if (_toEquipment)
            {
                //toEquip = te;
                // if (!toEquip.slotTypes.TryGetValue(toIndex, out slotType) ||
                //     !IsItemAllowedForCharacter(fromSlot.Item.Value, slotType))
                if (!_access.TryGetSlotType(toSource, toIndex, out slotType) ||
                    !_access._equipmentValidation.CanEquip(toSource, slotType, fromSlot.Item))
                {
                    DLog.Alert($"❌ {fromSlot.Item.name} нельзя поместить в слот {slotType}", EDlogColor.ORANGE);
                    return;
                }

                // Доп проверка для быстрых слотов
                if (fromType == EquipmentSlotType.WeaponMain &&
                    (slotType == EquipmentSlotType.QuickSlot1 || slotType == EquipmentSlotType.QuickSlot2))
                {
                    if (!toSlot.IsEmpty && toSlot.Item.GetEquipSlot() != EquipSlotType.Weapon)
                    {
                        DLog.Alert("❌ В быстрый слот можно положить только оружие или пустой слот!", EDlogColor.ORANGE);
                        return;
                    }
                }
            }
            else if (_fromEquipment && !toSlot.IsEmpty) // equipment => inventory
            {
                if (!_access.TryGetSlotType(fromSource, fromIndex, out slotType) ||
                    !_access._equipmentValidation.CanEquip(fromSource, slotType, toSlot.Item))
                {
                    DLog.Alert($"❌ {toSlot.Item.name} нельзя поместить в слот {slotType}", EDlogColor.ORANGE);
                    return;
                }
            }
            
            if (_fromEquipment && _toEquipment)
            {
                // Если в целевом слоте лежит предмет – проверяем можно ли его положить в "fromSlotType"
                // !fromEquip.slotTypes.TryGetValue(fromIndex, out slotType) 
                if (!toSource.GetSlot(toIndex).IsEmpty && 
                    (!_access.TryGetSlotType(fromSource, fromIndex, out slotType) ||
                    !_access._equipmentValidation.CanEquip(
                        toSource, 
                        slotType, 
                        toSource.GetSlot(toIndex).Item)))
                {
                    DLog.Alert($"❌ {toSource.GetSlot(toIndex).Item.name} нельзя поместить в слот {slotType}", EDlogColor.ORANGE);
                    return;
                }
            }


            // if (fromEquip != toEquip &&
            //     ((fromEquip != null && InventoryRules.IsBagSlot(fromType)) ||
            //      (toEquip != null && InventoryRules.IsBagSlot(slotType))))
            // {
            //     var inventory = left.Inventory as CharacterInventoryData;
            //
            //     // Проверка снятия сумки
            //     if (fromEquip != null && InventoryRules.IsBagSlot(fromType))
            //     {
            //         var oldSlot = new InventorySlot(fromSlot.Item.Value, fromSlot.Amount.Value,
            //             fromSlot.Durability.Value);
            //         var newSlot = new InventorySlot(null, 0, 0); // сумка снимается
            //         if (!fromEquip.CanChangeBag(toIndex, toInventory, fromIndex, oldSlot, newSlot, inventory))
            //         {
            //             DLog.Alert("Нельзя снять сумку — отключаемые слоты заняты!", EDlogColor.ORANGE);
            //             return;
            //         }
            //     }
            //
            //     // Проверка надевания сумки
            //     if (toEquip != null && InventoryRules.IsBagSlot(slotType))
            //     {
            //         var oldSlot = new InventorySlot(toEquip.InventoryProxy.Slots[toIndex].Item.Value,
            //             toEquip.InventoryProxy.Slots[toIndex].Amount.Value,
            //             toEquip.InventoryProxy.Slots[toIndex].Durability.Value);
            //         var newSlot = new InventorySlot(fromSlot.Item.Value, fromSlot.Amount.Value,
            //             fromSlot.Durability.Value);
            //         if (!toEquip.CanChangeBag(fromIndex, toInventory, toIndex, oldSlot, newSlot, inventory))
            //         {
            //             DLog.Alert("Нельзя надеть сумку — отключаемые слоты заняты!", EDlogColor.ORANGE);
            //             return;
            //         }
            //     }
            // }


            // 🎒 Перед переносом сохраняем состояние обоих слотов
            // InventorySlot oldFrom =
            //     new InventorySlot(fromSlot.Item, fromSlot.Amount, fromSlot.Durability);
            // var slot = toSource.GetSlot(toIndex);
            // InventorySlot oldTo = new InventorySlot(
            //     slot.Item,
            //     slot.Amount,
            //     slot.Durability);

            // =============================
            // 🧩 Выполняем сам перенос
            // =============================
            TransferSlot(fromSource, fromIndex, toSource, toIndex);

            // =============================
            // ⚙️ Теперь анализируем изменения после переноса
            // =============================

            // 🎒 Если добавили или заменили сумку в экипировке
            // if (toEquip != null &&
            //     toEquip.slotTypes.TryGetValue(toIndex, out var toSlotType) && InventoryRules.IsBagSlot(toSlotType))
            // {
            //     var newSlot = toEquip.InventoryProxy.Slots[toIndex];
            //     toEquip.HandleBagChange(toIndex, oldTo, new InventorySlot(newSlot.Item.Value, newSlot.Amount.Value,
            //             newSlot.Durability.Value),
            //         left.Inventory as CharacterInventoryData);
            // }
            //
            // // 🧳 Если сняли сумку из экипировки
            // if (fromEquip != null &&
            //     fromEquip.slotTypes.TryGetValue(fromIndex, out var fromSlotType) &&
            //     InventoryRules.IsBagSlot(fromSlotType))
            // {
            //     var newSlot = fromEquip.InventoryProxy.Slots[fromIndex];
            //     fromEquip.HandleBagChange(fromIndex, oldFrom, new InventorySlot(newSlot.Item.Value,
            //             newSlot.Amount.Value,
            //             newSlot.Durability.Value),
            //         left.Inventory as CharacterInventoryData);
            // }
        }

        /// <summary>
        /// Перенос оружия из быстрого слота в основной
        /// </summary>
        /// <param name="fromContainer"></param>
        /// <param name="fromIndex"></param>
        /// <param name="toContainer"></param>
        /// <param name="toIndex"></param>
        public void SwitchWeapon(
            IInventorySource fromSource,
            int fromIndex,
            IInventorySource toSource,
            int toIndex)
        {
            //var fromInventory = fromContainer.Inventory;
            //var toInventory = toContainer.Inventory;

            // =============================
            // 🧩 Выполняем сам перенос
            // =============================
            TransferSlot(fromSource, fromIndex, toSource, toIndex);
        }


        private void TransferSlot(
            IInventorySource fromSource,
            int fromIndex,
            IInventorySource toSource,
            int toIndex)
        {
            var fromSlot = fromSource.GetSlot(fromIndex);
            var toSlot = toSource.GetSlot(toIndex);

            bool fromIsEquip = _access._inventoryRules.IsEquipmentSource(fromSource);
            bool toIsEquip = _access._inventoryRules.IsEquipmentSource(toSource);

            // var equipmentContainer =
            //     fromInventory is PlayerEquipmentInventoryData || toInventory is PlayerEquipmentInventoryData
            //         ? ServiceLocator.Current.Get<PlayerRepository>().GetController.EquipmentContainer
            //         : ServiceLocator.Current.Get<DragonRepository>().GetController.EquipmentContainer;

            // ---------------------------------------------------------
            // 1️⃣ Определяем, был ли предмет экипирован в from
            // ---------------------------------------------------------
            bool wasEquipped = false;
            EquipmentSlotType fromSlotType = EquipmentSlotType.None;

            if (fromIsEquip)
            {
                // var eqInv = (CharacterEquipmentInventoryData)fromInventory;
                // if (eqInv.slotTypes.TryGetValue(fromIndex, out fromSlotType) &&
                //     !IsQuickSlot(fromSlotType) &&
                //     !fromSlot.IsEmpty)
                // {
                //     wasEquipped = true;
                // }
                if (_access.TryGetSlotType(fromSource, fromIndex, out fromSlotType) &&
                    !IsQuickSlot(fromSlotType) &&
                    !fromSlot.IsEmpty)
                {
                    wasEquipped = true;
                }
            }

            // ---------------------------------------------------------
            // 2️⃣ Выполняем перенос/обмен слотов
            // ---------------------------------------------------------
            if (toSlot.IsEmpty)
            {
                // Простое перемещение
                toSource.SetSlot(
                    toIndex, 
                    new InventorySlotRuntime(fromSlot.Item,  fromSlot.Amount, fromSlot.Durability, fromSlot.AmmoInMagazine));

                //fromSlot.Clear();
                fromSource.ClearSlot(fromIndex);
            }
            else if (fromSlot.Item == toSlot.Item && fromSlot.Item.Classification.maxStack > 1)
            {
                // Stack
                int total = fromSlot.Amount + toSlot.Amount;
                int max = fromSlot.Item.Classification.maxStack;

                toSlot.Amount = Mathf.Min(max, total);
                fromSlot.Amount = Mathf.Max(0, total - max);

                if (fromSlot.Amount == 0)
                    //fromSlot.Clear();
                    fromSource.ClearSlot(fromIndex);

                //toSource.InventoryProxy.Slots[toIndex] = toSlot;
                //fromSource.InventoryProxy.Slots[fromIndex] = fromSlot;
                toSource.SetSlot(toIndex, toSlot);
                fromSource.SetSlot(fromIndex, fromSlot);
            }
            else
            {
                // Swap
                // (fromSource.InventoryProxy.Slots[fromIndex], toSource.InventoryProxy.Slots[toIndex]) =
                //     (toSource.InventoryProxy.Slots[toIndex], fromSource.InventoryProxy.Slots[fromIndex]);

                var a = fromSource.GetSlot(fromIndex).Clone();
                var b = toSource.GetSlot(toIndex).Clone();

                fromSource.SetSlot(fromIndex, b);
                toSource.SetSlot(toIndex, a);
            }

            // ---------------------------------------------------------
            // 3️⃣ Снимаем предмет из from (если он был экипирован)
            // ---------------------------------------------------------
            if (wasEquipped)
            {
                //equipmentContainer.Unequip(fromIndex);
                fromSource.EquipmentListener.Unequip(fromIndex);
            }

            // ---------------------------------------------------------
            // 4️⃣ Надеваем предмет в to (если это слот экипировки)
            // ---------------------------------------------------------
            if (toIsEquip)
            {
                // var eqInv = (CharacterEquipmentInventoryData)toInventory;
                //
                // if (eqInv.slotTypes.TryGetValue(toIndex, out var toSlotType) &&
                //     !IsQuickSlot(toSlotType))
                // {
                //     var newSlot = toSource.InventoryProxy.Slots[toIndex];
                //
                //     if (!newSlot.IsEmpty)
                //         //equipmentContainer.Equip(toIndex);
                // }
                if (_access.TryGetSlotType(toSource, toIndex, out var toSlotType) &&
                    !IsQuickSlot(toSlotType))
                {
                    var newSlot = toSource.GetSlot(toIndex);

                    if (!newSlot.IsEmpty)
                    {
                        //equipmentContainer.Equip(toIndex);
                        toSource.EquipmentListener.Equip(toIndex);
                    }
                }
            }

            // ---------------------------------------------------------
            // 5️⃣ Финальные события
            // ---------------------------------------------------------
            //fromSource.OnChanged?.Invoke();
            //toSource.OnChanged?.Invoke();
            _access.NotifyChanged(fromSource);
            _access.NotifyChanged(toSource);
        }


        public void HandleDoubleClick(IInventorySource fromSource, int fromIndex)
        {
            var fromInventory = fromSource.InventoryData;
            var fromSlot = fromSource.GetSlot(fromIndex);
            if (fromSlot.IsEmpty || RightSource == null) 
                return;

            var item = fromSlot.Item;
            var equipType = item.GetEquipSlot();

            bool isLeftInventory = fromSource == LeftSource;


            // 1️⃣ Инвентарь ↔ Инвентарь (обе стороны)
            if (RightSource != null)
            {
                // 1️⃣ От игрока в контейнер (ящик / дракон)
                if (isLeftInventory)
                {
                    // if (right.Inventory is HomeInventoryData or OuterInventoryData ||
                    //     right.Inventory is DragonInventoryData)
                    // {
                    //     AutoMoveToFirstFreeSlot(fromSource, fromIndex, right.Inventory);
                    //     return;
                    // }
                    if (!_access._inventoryRules.IsEquipmentSource(RightSource))
                    {
                        AutoFullMmove(fromSource, fromIndex, RightSource);
                        return;
                    }
                }

                // 2️⃣ Из контейнера (ящик / дракон) к игроку
                if (!isLeftInventory)
                {
                    // if (fromInventory is HomeInventoryData or OuterInventoryData || fromInventory is DragonInventoryData)
                    // {
                    //     AutoMoveToFirstFreeSlot(fromSource, fromIndex, LeftSource.InventoryData);
                    //     return;
                    // }
                    if (!_access._inventoryRules.IsEquipmentSource(fromSource))
                    {
                        AutoFullMmove(fromSource, fromIndex, LeftSource);
                        return;
                    }
                }
            }

            // 2️⃣ Инвентарь → экипировка
            // if (isLeftInventory && right.Inventory is CharacterEquipmentInventoryData equipData)
            // {
            //     int? equipSlotIndex = InventoryRules.FindMatchingEquipmentSlot(equipData, equipType, item);
            //     if (equipSlotIndex.HasValue)
            //     {
            //         MoveSlot(left, fromIndex, right, equipSlotIndex.Value);
            //         return;
            //     }
            // }
            if (isLeftInventory && _access._inventoryRules.IsEquipmentSource(RightSource))
            {
                int? equipSlotIndex = InventoryRules.FindMatchingEquipmentSlot(RightSource, equipType, _access, item);
                if (equipSlotIndex.HasValue)
                {
                    MoveSlot(LeftSource, fromIndex, RightSource, equipSlotIndex.Value);
                    return;
                }
            }

            // 3️⃣ Экипировка → инвентарь
            //if (!isLeftInventory && fromInventory is CharacterEquipmentInventoryData fromEquip)
            if (!isLeftInventory && _access._inventoryRules.IsEquipmentSource(fromSource))
            {
                //var playerInv = left.Inventory as CharacterInventoryData; // as PlayerInventoryData;
                //fromEquip.slotTypes.TryGetValue(fromIndex, out var fromType);

                // 🎒 Проверка если снимаем сумку
                // if (InventoryRules.IsBagSlot(fromType))
                // {
                //     var oldSlot = new InventorySlot(fromEquip.InventoryProxy.Slots[fromIndex].Item.Value,
                //         fromEquip.InventoryProxy.Slots[fromIndex].Amount.Value,
                //         fromEquip.InventoryProxy.Slots[fromIndex].Durability.Value);
                //     var newSlot = new InventorySlot(null, 0, 0); // снимаем сумку
                //
                //     // Проверяем, можно ли снять
                //     if (!fromEquip.CanChangeBag(fromIndex, fromInventory, fromIndex, oldSlot, newSlot, playerInv))
                //     {
                //         DLog.Alert("Нельзя снять сумку — отключаемые слоты заняты!", EDlogColor.ORANGE);
                //         return;
                //     }
                //
                //     // ⚡ Сначала уменьшаем размер инвентаря (чтобы UI отреагировал)
                //     fromEquip.HandleBagChange(fromIndex, oldSlot, newSlot, playerInv);
                //
                //     // ✅ Потом переносим сумку в первый свободный слот
                //     AutoMoveToFirstFreeSlot(fromSource, fromIndex, playerInv);
                //     return;
                // }

                // 🧤 Обычное снятие предмета
                //AutoMoveToFirstFreeSlot(fromSource, fromIndex, left.Inventory as CharacterInventoryData);
                AutoFullMmove(fromSource, fromIndex, LeftSource);
            }
        }



        /// <summary>
        /// Проверяем подходит ли предмет юниту/технике
        /// </summary>
        /// <param name="item"></param>
        /// <param name="slotType"></param>
        /// <returns></returns>
        private bool IsItemAllowedForCharacter(ItemConfig item, EquipmentSlotType slotType) => true;
            // => item != null &&
            //    //slotToEquipType.TryGetValue(slotType, out var type) && item.config.equipType == type &&
            //    InventoryRules.IsEquipTypeAllowedForSlot(item.Config.EquipSlotType, slotType) &&
            //    (!item.ForVehicle && right.Inventory is PlayerEquipmentInventoryData ||
            //     item.ForVehicle && right.Inventory is DragonEquipmentInventoryData);


        private void AutoFullMmove(
            IInventorySource fromSource,
            int fromIndex,
            IInventorySource toSource)
        {
            var fromSlot = fromSource.GetSlot(fromIndex);

            if (fromSlot.IsEmpty)
                return;

            var item = fromSlot.Item;

            // Пока есть что переносить
            while (!fromSource.GetSlot(fromIndex).IsEmpty)
            {
                bool movedSomething = false;

                // 1️⃣ Сначала дополняем существующие стаки
                for (int i = 0; i < toSource.GetSlots().Count; i++)
                {
                    var toSlot = toSource.GetSlot(i);

                    if (!toSlot.IsEmpty &&
                        toSlot.Item == item &&
                        toSlot.Amount < item.Classification.maxStack)
                    {
                        TransferSlot(fromSource, fromIndex, toSource, i);
                        movedSomething = true;
                        break;
                    }
                }

                if (movedSomething)
                    continue;

                // 2️⃣ Потом ищем пустой слот
                for (int i = 0; i < toSource.GetSlots().Count; i++)
                {
                    var toSlot = toSource.GetSlot(i);

                    if (toSlot.IsEmpty)
                    {
                        TransferSlot(fromSource, fromIndex, toSource, i);
                        movedSomething = true;
                        break;
                    }
                }

                // 3️⃣ Если больше некуда класть — выходим
                if (!movedSomething)
                {
                    DLog.Alert("❌ Нет свободных слотов!", EDlogColor.ORANGE);
                    return;
                }
            }
        }


            private bool IsQuickSlot(EquipmentSlotType type)
        {
            return type == EquipmentSlotType.QuickSlot1 ||
                   type == EquipmentSlotType.QuickSlot2;
        }


    }

}