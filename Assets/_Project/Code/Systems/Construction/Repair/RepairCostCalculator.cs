using System.Collections.Generic;
using Galactic1.Code.Systems.Economy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction.Repair
{
    /// <summary>
    /// Чистый расчёт стоимости ремонта.
    ///
    /// Не имеет состояния и побочных эффектов — может быть переиспользован
    /// любыми будущими системами (AI-ремонт, дроны, авто-ремонт со временем).
    /// </summary>
    public static class RepairCostCalculator
    {
        public static List<RequirementData> Calculate(
            IReadOnlyList<RequirementData> buildRecipe,
            float currentHP,
            float maxHP,
            IRepairRoundingStrategy rounding)
        {
            var result = new List<RequirementData>();

            if (buildRecipe == null || buildRecipe.Count == 0 || rounding == null)
                return result;

            float missingRatio = GetMissingRatio(currentHP, maxHP);

            if (missingRatio <= 0f)
                return result;

            foreach (var requirement in buildRecipe)
            {
                if (requirement == null || requirement.Item == null)
                    continue;

                float rawAmount = requirement.Amount * missingRatio;
                int roundedAmount = rounding.Round(rawAmount);

                if (roundedAmount <= 0)
                    continue;

                result.Add(new RequirementData
                {
                    Item = requirement.Item,
                    Amount = roundedAmount
                });
            }

            return result;
        }

        /// <summary>
        /// Доля недостающего HP в диапазоне [0..1].
        /// </summary>
        public static float GetMissingRatio(float currentHP, float maxHP)
        {
            if (maxHP <= 0f)
                return 0f;

            float missing = Mathf.Clamp(maxHP - currentHP, 0f, maxHP);
            return missing / maxHP;
        }
    }
}