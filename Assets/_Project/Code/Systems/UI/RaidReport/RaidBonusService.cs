using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.UI.RaidReport
{
    public class RaidBonusService : IGameService
    {
        private readonly IInventoryResourcesPort _transportInventory;

        public RaidBonusService(IInventoryResourcesPort transportInventory)
        {
            _transportInventory = transportInventory;
        }

        public RaidBonusEligibility CheckEligibility(List<RaidLootResult> loot)
        {
            // #1 лута нет
            if(loot.Count == 0)
                return RaidBonusEligibility.Ineligible(IneligibleReason.NoLoot);
            
            // #2 Нет ресурсов для бонуса
            bool bonusAvail = true;
            if (loot.All(i => i.Item.Classification.itemLabel != ItemLabel.Resource))
                bonusAvail = false;

            // проверяем финальный лут для добавления в транспорт
            var slots = BuildSlots(loot);
            if (!_transportInventory.CanAddMultiple(slots))
                return RaidBonusEligibility.Ineligible(IneligibleReason.TransportFull);
            
            // лут входит с ресурсами после бонуса, либо только снаряга
            return RaidBonusEligibility.Eligible(bonusAvail);
        }

        // bonus только к ресурсам, в существующих слотах
        public (List<RaidLootResult>, int onlyBonus) ApplyBonus(List<RaidLootResult> loot, float multiplier)
        {
            var result= loot.Select(item =>
            {
                if (item.Item.Classification.itemLabel != ItemLabel.Resource)
                    return item;

                var bonused = item;
                bonused.TotalAmount = Mathf.CeilToInt(item.Amount * multiplier);
                bonused.BonusAmount = bonused.TotalAmount - item.Amount;
                return bonused;
            }).ToList();

            return (result, result.Sum(item => item.BonusAmount));
        }

        // Конвертируем RaidLootResult → InventorySlotRuntime
        // Та же логика что в UniversalProductionSceneAdapter.CancelOrder()
        private static List<InventorySlotRuntime> BuildSlots(List<RaidLootResult> loot)
        {
            return loot.Select(l => new InventorySlotRuntime(
                l.Item,
                l.Amount,
                l.Durability,
                l.AmmoInMagazine
            )).ToList();
        }
    }
}