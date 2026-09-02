
    using Galactic1.Code.Inventory.Abstractions;
    using Galactic1.Code.Inventory.Services;
    using Galactic1.Configs;
    using Galactic1.Core.Enums;
    using Galactic1.Game.Meta.Items;
    using Galactic1.Items;

    namespace Galactic1.Code.UI.Inventory
    {
        public static class InventoryRules
        {

            public static bool IsBagSlot(EquipmentSlotType slotType)
                => slotType == EquipmentSlotType.Bag1 ||
                   slotType == EquipmentSlotType.Bag2 ||
                   slotType == EquipmentSlotType.Bag3 ||
                   slotType == EquipmentSlotType.Bag4;


            public static bool IsEquipTypeAllowedForSlot(EquipSlotType equipType, EquipmentSlotType slotType)
            {
                return slotType switch
                {
                    EquipmentSlotType.WeaponMain or 
                        EquipmentSlotType.WeaponSecondary => equipType == EquipSlotType.Weapon,
                    EquipmentSlotType.QuickSlot1 or 
                        EquipmentSlotType.QuickSlot2 => // добавь оставшиеся слоты QuickSlot3 , QuickSlot4 если используется    
                        equipType == EquipSlotType.Usable,
                    //EquipmentSlotType.Shield => equipType == ItemEquipSlotType.Shield,
                    // EquipmentSlotType.Bag1
                    //     or EquipmentSlotType.Bag2
                    //     or EquipmentSlotType.Bag3
                    //     or EquipmentSlotType.Bag4 => equipType == ItemEquipSlotType.Bag,
                    EquipmentSlotType.Head => equipType == EquipSlotType.Head,
                    EquipmentSlotType.Body => equipType == EquipSlotType.Torso,
                    EquipmentSlotType.Pants => equipType == EquipSlotType.Pants,
                    EquipmentSlotType.Legs => equipType == EquipSlotType.Boots,
                    _ => false
                };
            }



            public static int? FindMatchingEquipmentSlot(
                IInventorySource source,
                EquipSlotType equipType,
                InventoryAccessService accessService,
                ItemConfig item = null)
            {
                // ⚔️ Для оружия используем отдельную расширенную логику
                if (equipType == EquipSlotType.Weapon)
                    return FindWeaponMainSlot(source, accessService);

                //if (equipData is PlayerEquipmentInventoryData && equipType == ItemEquipSlotType.Bag)
                    //return FindBestBagSlot(source, item);

                int? free = null;
                int? firstOccupied = null;
                var equipmentSlots = accessService.GetEquipmentSlots(source);

                foreach (var esl in equipmentSlots)
                {
                    int index = esl.Key;
                    EquipmentSlotType slotType = esl.Value;

                    if (!IsEquipTypeAllowedForSlot(equipType, slotType))
                        continue;

                    var slot = source.GetSlot(index);


                    // 1️⃣ Если слот занят — проверяем на стакание расходников
                    if (equipType == EquipSlotType.Usable &&
                        slot.Item != null &&
                        slot.Item == item &&
                        slot.Amount < slot.Item.Classification.maxStack)
                    {
                        return index; // 👍 Можно стекаться — выбираем этот слот
                    }

                    // 2️⃣ Пустой
                    if (slot.IsEmpty)
                    {
                        free = index;
                        break;
                    }

                    // 🎯 3. Если нет пустых — запоминаем первый занятый
                    firstOccupied = index;
                }

                // Если все подходящие слоты заняты — используем первый найденный
                return free ?? firstOccupied;
            }

            public static int? FindWeaponMainSlot(IInventorySource source, InventoryAccessService accessService)
            {
                var equipmentSlots = accessService.GetEquipmentSlots(source);
                foreach (var kvp in equipmentSlots)
                {
                    if (kvp.Value == EquipmentSlotType.WeaponMain)
                        return kvp.Key;
                }

                // Если вдруг нет такого слота — ошибка в данных
                return null;
            }


            // public static int? FindBestWeaponSlot(CharacterEquipmentInventoryData equipData, ItemBase newWeapon)
            // {
            //     int? emptySlot = null;
            //     int? weakerSlot = null;
            //     int? firstSlot = null;
            //
            //     float newPower = GetWeaponPower(newWeapon);
            //
            //     foreach (var kvp in equipData.slotTypes)
            //     {
            //         int index = kvp.Key;
            //         EquipmentSlotType slotType = kvp.Value;
            //
            //         if (slotType != EquipmentSlotType.WeaponMain && 
            //             slotType != EquipmentSlotType.QuickSlot1 && 
            //             slotType != EquipmentSlotType.QuickSlot2)
            //             continue;
            //
            //         var slot = equipData.InventoryProxy.Slots[index];
            //
            //         // Запоминаем первый слот для замены, если оба сильные
            //         firstSlot ??= index;
            //
            //         // 1️⃣ Пустой слот — приоритет №1
            //         if (slot.IsEmpty)
            //         {
            //             emptySlot ??= index;
            //             continue;
            //         }
            //
            //         // 2️⃣ Проверяем силу оружия
            //         float currentPower = GetWeaponPower(slot.Item.Value);
            //         if (currentPower < newPower)
            //         {
            //             weakerSlot = index; // слабее — можно заменить
            //         }
            //     }
            //
            //     // ⚙️ Приоритет:
            //     // 1️⃣ Пустой слот
            //     // 2️⃣ Слабое оружие
            //     // 3️⃣ Если оба сильные — заменяем первый слот
            //     return emptySlot ?? weakerSlot ?? firstSlot;
            // }


            // public static float GetWeaponPower(ItemConfig weapon)
            // {
            //     if (weapon == null) return 0f;
            //
            //     if (weapon.Config is WeaponConfig wcfg)
            //         return wcfg.WeaponData.damage * wcfg.WeaponData.fireRate; // или любой другой критерий
            //
            //     return 0f;
            // }


            // public static int? FindBestBagSlot(CharacterEquipmentInventoryData equipData, ItemBase newBag)
            // {
            //     int? emptySlot = null;
            //     int? smallerSlot = null;
            //     int? firstSlot = null;
            //
            //     int newCapacity = GetBagCapacity(newBag);
            //
            //     foreach (var kvp in equipData.slotTypes)
            //     {
            //         int index = kvp.Key;
            //         EquipmentSlotType slotType = kvp.Value;
            //
            //         if (slotType != EquipmentSlotType.Bag1 &&
            //             slotType != EquipmentSlotType.Bag2 &&
            //             slotType != EquipmentSlotType.Bag3 &&
            //             slotType != EquipmentSlotType.Bag4)
            //             continue;
            //
            //         var slot = equipData.InventoryProxy.Slots[index];
            //
            //         firstSlot ??= index; // первый слот на замену, если обе сумки больше
            //
            //         // 1️⃣ Пустой слот → приоритет
            //         if (slot.IsEmpty)
            //         {
            //             emptySlot ??= index;
            //             continue;
            //         }
            //
            //         // 2️⃣ Меньшая вместимость → приоритет для замены
            //         int currentCapacity = GetBagCapacity(slot.Item.Value);
            //         if (currentCapacity < newCapacity)
            //         {
            //             smallerSlot = index;
            //         }
            //     }
            //
            //     // Приоритет:
            //     // 1️⃣ Пустой слот
            //     // 2️⃣ Менее вместительный
            //     // 3️⃣ Если обе сумки больше — заменяем первый слот
            //     return emptySlot ?? smallerSlot ?? firstSlot;
            // }

            // public static int GetBagCapacity(ItemConfig bag)
            // {
            //     if (bag == null) return 0;
            //     if (bag.Config is BagConfig bcfg)
            //         return bcfg.Capacity;
            //     return 0;
            // }
        }
    }