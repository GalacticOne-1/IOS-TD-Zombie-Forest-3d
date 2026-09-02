using System.Collections.Generic;
using System.Linq;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.UI.Inventory
{
    public static class InventoryAutoEquipService
    {
        public static (bool fromEquipment, int? index) FindReplacement(
            IInventoryContainer inventory,
            IInventoryContainer equipment,
            ItemConfig brokenItem,
            bool isTool)
        {
            var equipSlots = equipment.Inventory;
            var invSlots = inventory.Inventory;

            List<(bool fromEquip, int index, ItemConfig item)> candidates = new();

            // 1) Собираем кандидаты из обоих контейнеров
            CollectCandidates(equipSlots, true, brokenItem, candidates, isTool);
            CollectCandidates(invSlots, false, brokenItem, candidates, isTool);

            if (candidates.Count == 0)
                return (false, null);

            // 2) Выбираем лучший
            var best = ChooseBestLDOE(candidates);

            return (best.fromEquip, best.index);
        }

        // =================================================================================
        // КРИТЕРИИ СОВМЕСТИМОСТИ — как в LDOE
        // =================================================================================

        private static void CollectCandidates(
            BaseInventoryData inventory,
            bool fromEquip,
            ItemConfig broken,
            List<(bool, int, ItemConfig)> result,
            bool requiresTool)
        {
            var l = inventory.InventoryProxy.Slots.Count;
            for (int i = 0; i < l; i++)
            {
                var proxy = inventory.InventoryProxy.Slots[i];
                if (proxy.IsEmpty)
                    continue;

                var item = proxy.Item.Value;
                if (item == null)
                    continue;

                // #1 ОРУЖИЕ → подходит все что может наносить урон (как в LDoE)
                if (!requiresTool && broken.HasModule<WeaponModule>() && item.HasModule<WeaponModule>())
                {
                    result.Add((fromEquip, i, item));
                    continue;
                }

                // #2 ИНСТРУМЕНТЫ → только одинаковый ToolClass
                // if (requiresTool && broken.IsTool() && item.IsTool())
                // {
                //     if (item.EquipClass == broken.EquipClass)
                //         result.Add((fromEquip, i, item));
                //
                //     continue;
                // }

                // #3 БРОНЯ → только одинаковый слот (helmet/torso/legs/boots/shield)
                if (broken.HasModule<EquipmentModule>()  && broken.GetEquipSlot() == item.GetEquipSlot())
                {
                    //if (item.GetEquipSlot() == broken.GetEquipSlot())
                        result.Add((fromEquip, i, item));

                    continue;
                }

                // #4 Одежда / спец-слоты → как в LDOE по slotType
                // if (item.Config.EquipSlotType == broken.Config.EquipSlotType)
                // {
                //     result.Add((fromEquip, i, item));
                //     continue;
                // }
            }
        }

        // =================================================================================
        // ВЫБОР ЛУЧШЕГО — как LDOE выбирает "next best item"
        // =================================================================================

        private static (bool fromEquip, int index, ItemConfig item) ChooseBestLDOE(
            List<(bool fromEquip, int index, ItemConfig item)> list)
        {
            return list
                .OrderByDescending(c => c.item.Classification.rarity) // 1) редкость
                .ThenByDescending(c => GetPowerRating(c.item)) // 2) power rating
                .ThenByDescending(c => GetArmor(c.item)) // 3) броня, если есть
                .ThenByDescending(c => c.item.Physical.maxDurability) // 4) прочность
                .ThenBy(c => c.index) // 5) стабильность
                .First();
        }

        // =================================================================================
        // Расчёт "ценности" предмета — как в LDOE (DPS + скорость)
        // =================================================================================

        private static float GetPowerRating(ItemConfig item)
            => item.HasModule<WeaponModule>() ? item.Weapon.DPS : 0;

        static float GetArmor(ItemConfig item)
            => item.Equipment.BaseStats().TryGetStat(StatId.Armor, out var value) ? value : 0;



        public static void MoveToEquipment(
            IInventoryContainer fromContainer,
            int fromIndex,
            IInventoryContainer toContainer,
            int toIndex)
        {
            // слот откуда переносим предмет
            var fromSlot = fromContainer.Inventory.InventoryProxy.Slots[fromIndex];

            // Простое перемещение
            toContainer.Inventory.InventoryProxy.SetSlot(toIndex, new InventorySlotProxy(
                new InventorySlotData(
                    fromSlot.Item.Value, 
                    "",
                    fromSlot.Amount.Value,
                    fromSlot.Durability.Value,
                    fromSlot.AmmoInMagazine.Value)));

            fromSlot.Clear();
        }
    }

}