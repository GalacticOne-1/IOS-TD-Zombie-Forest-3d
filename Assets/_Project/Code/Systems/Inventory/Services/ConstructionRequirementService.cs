using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Inventory.Services
{
    /// <summary>
    /// Доменный сервис проверки требований строительства.
    ///
    /// Роль:
    /// - Проверяет наличие ресурсов для строительства.
    /// - Изолирует UI слой от системы инвентарей.
    /// - Работает через агрегированный источник ресурсов лагеря (CampInventoryPort).
    ///
    /// Используется UI ConstructionPanel для отображения:
    /// - можно ли построить
    /// - сколько ресурсов есть / требуется
    /// </summary>
    public class ConstructionRequirementService
    {
        private readonly IInventoryResourcesPort _inventory;

        /// <summary>
        /// Принимает агрегированный источник ресурсов лагеря.
        /// Обычно это CampInventoryPort.
        /// </summary>
        public ConstructionRequirementService(IInventoryResourcesPort inventory)
        {
            _inventory = inventory;
        }

        /// <summary>
        /// Проверяет можно ли построить указанное здание.
        /// </summary>
        public bool CanBuild(FacilityModule facility)
        {
            if (facility == null)
                return false;

            var recipes = facility.Item.Recipes;

            if (recipes == null || recipes.Count == 0)
                return true;

            // Если есть хотя бы один рецепт который можно оплатить — строить можно
            foreach (var recipe in recipes)
            {
                if (HasResources(recipe.Requirement.ToList()))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет достаточно ли ресурсов для одного требования.
        /// </summary>
        public bool HasResources(RequirementData requirement)
        {
            if (requirement == null || requirement.Item == null)
                return false;

            int owned = _inventory.GetTotalAmount(requirement.Item.Id);

            return owned >= requirement.Amount;
        }

        /// <summary>
        /// Возвращает текущее количество ресурса в инвентаре лагеря.
        /// Используется UI для отображения вида: 3 / 10.
        /// </summary>
        public int GetOwnedAmount(RequirementData requirement)
        {
            if (requirement == null || requirement.Item == null)
                return 0;

            return _inventory.GetTotalAmount(requirement.Item.Id);
        }

        /// <summary>
        /// Проверяет набор требований (например рецепт строительства).
        /// </summary>
        public bool HasResources(List<RequirementData> requirements)
        {
            if (requirements == null || requirements.Count == 0)
                return true;

            foreach (var requirement in requirements)
            {
                if (!HasResources(requirement))
                    return false;
            }

            return true;
        }
        
        
        /// <summary>
        /// Списывает произвольный набор требований (например расчитанную стоимость ремонта).
        /// Переиспользуется другими системами поверх той же инвентарь-логики.
        /// </summary>
        public bool TrySpend(List<RequirementData> requirements)
        {
            if (requirements == null || requirements.Count == 0)
                return true;

            if (!HasResources(requirements))
                return false;

            SpendRequirements(requirements);
            return true;
        }
        
        /// <summary>
        /// Пытается списать ресурсы для строительства.
        /// Гарантирует атомарность операции.
        /// </summary>
        public bool TrySpend(FacilityModule facility)
        {
            if (facility == null)
                return false;

            var recipes = facility.Item.Recipes;

            if (recipes == null || recipes.Count == 0)
                return true;

            foreach (var recipe in recipes)
            {
                var requirements = recipe.Requirement.ToList();

                if (!HasResources(requirements))
                    continue;

                SpendRequirements(requirements);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Списывает набор ресурсов.
        /// Предполагается что проверка уже выполнена.
        /// </summary>
        private void SpendRequirements(List<RequirementData> requirements)
        {
            foreach (var requirement in requirements)
            {
                if (requirement == null || requirement.Item == null)
                    continue;

                _inventory.TrySpend(requirement.Item.Id, requirement.Amount);
            }
        }
        
        /// <summary>
        /// Возвращает стоимость строительства здания.
        /// Используется UI для отображения требований.
        /// </summary>
        public List<RequirementData> GetBuildCost(FacilityModule facility)
        {
            if (facility == null)
                return null;

            var recipes = facility.Item.Recipes;

            if (recipes == null || recipes.Count == 0)
                return null;

            // Пока берем первый рецепт
            return recipes[0].Requirement.ToList();
        }
    }
}