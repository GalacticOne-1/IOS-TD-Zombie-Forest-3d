using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Systems.CampDefense.Penalty
{
    /// <summary>
    /// Отвечает только за вычисление штрафа. Ничего не изменяет.
    /// Возвращает immutable CampDefensePenaltyResult.
    ///
    /// Фильтрация предметов (что можно/нельзя забирать) происходит здесь,
    /// а не в Applier и не в UI.
    /// </summary>
    public sealed class CampDefensePenaltyCalculator : ICampDefensePenaltyCalculator
    {
        private readonly CampDefensePenaltyConfig _config;

        public CampDefensePenaltyCalculator(CampDefensePenaltyConfig config)
        {
            _config = config;
        }

        public CampDefensePenaltyResult Calculate(GameLoopContext context)
        {
            var totals = CollectItemTotals(context.CampRuntime);

            var items = new List<CampDefensePenaltyItem>();

            foreach (var total in totals.Values)
            {
                var amountToSteal = CalculateAmountToSteal(total.Amount);
                if (amountToSteal <= 0)
                    continue;

                items.Add(new CampDefensePenaltyItem(total.Item, amountToSteal));
            }

            return items.Count > 0
                ? new CampDefensePenaltyResult(items)
                : CampDefensePenaltyResult.Empty;
        }

        /// <summary>
        /// Один предмет может лежать в нескольких слотах одного склада
        /// (например Iron: 15 в одном слоте + 15 в другом).
        /// Защитные пороги (MinStackToSteal / MinimumUnitsLeft / MinimumPercentLeft)
        /// должны считаться от суммарного объёма предмета на складе,
        /// а не от отдельного слота — иначе защита срабатывает даже когда
        /// суммарно предмета более чем достаточно.
        /// </summary>
        private Dictionary<RuntimeId, (ItemConfig Item, int Amount)> CollectItemTotals(CampRuntime campRuntime)
        {
            var totals = new Dictionary<RuntimeId, (ItemConfig Item, int Amount)>();

            foreach (var storage in GetPlayerStorages(campRuntime))
            {
                foreach (var slot in storage.GetSlots())
                {
                    if (slot == null || slot.IsEmpty)
                        continue;

                    if (!IsStealable(slot.Item))
                        continue;

                    var itemId = slot.Item.Id;

                    totals[itemId] = totals.TryGetValue(itemId, out var existing)
                        ? (existing.Item, existing.Amount + slot.Amount)
                        : (slot.Item, slot.Amount);
                }
            }

            return totals;
        }

        /// <summary>
        /// Шаг 1: получить все склады игрока.
        /// Soft Launch: CampRuntime.GetInventory сейчас схлопывает любую категорию
        /// в один Regular-инвентарь (см. комментарий внутри CampRuntime).
        /// Когда появятся раздельные склады (Ammo/Food/Warehouse/Medical) —
        /// этот метод расширяется списком StorageType, логика Calculator не меняется.
        /// </summary>
        private IReadOnlyList<IInventorySource> GetPlayerStorages(CampRuntime campRuntime)
        {
            var storage = campRuntime.GetInventory(StorageType.Regular);

            return storage != null
                ? new[] { storage }
                : Array.Empty<IInventorySource>();
        }

        /// <summary>
        /// Шаг 3 (фильтр): можно ли вообще забирать этот предмет.
        /// Soft Launch: только Raw Resources / Craft Materials / Food / Water / Ammo —
        /// все они классифицированы как ItemLabel.Resource.
        /// Weapons/Armor/Quest/Currency/Buildings/Units сюда не попадают.
        /// </summary>
        private bool IsStealable(ItemConfig item)
        {
            if (item == null)
                return false;

            return item.Classification.itemLabel == ItemLabel.Resource;
        }

        /// <summary>
        /// Шаги 3–4: посчитать объём потери и ограничить его так,
        /// чтобы штраф оставался "мягким".
        /// </summary>
        private int CalculateAmountToSteal(int currentAmount)
        {
            // если ресурса мало — не трогаем стак вообще
            if (currentAmount < _config.MinStackToSteal)
                return 0;

            var target = Mathf.RoundToInt(currentAmount * _config.PenaltyPercent);

            // абсолютный потолок по проценту
            var maxByCap = Mathf.RoundToInt(currentAmount * _config.MaximumPercent);
            target = Mathf.Min(target, maxByCap);

            // не может остаться меньше MinimumPercentLeft
            var minLeftByPercent = Mathf.CeilToInt(currentAmount * _config.MinimumPercentLeft);
            var maxStealByPercentFloor = currentAmount - minLeftByPercent;

            // не может остаться меньше MinimumUnitsLeft
            var maxStealByUnitsFloor = currentAmount - _config.MinimumUnitsLeft;

            target = Mathf.Min(target, maxStealByPercentFloor, maxStealByUnitsFloor);

            return Mathf.Max(0, target);
        }
    }
}