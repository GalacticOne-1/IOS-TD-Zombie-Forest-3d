using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Game.UI.Production.DTO
{
    /// <summary>
    /// DTO для отображения одного требуемого ресурса рецепта.
    /// UI-слой. Не содержит ссылок на Config или Runtime.
    /// </summary>
    public readonly struct RecipeRequirementDto
    {
        /// <summary>ID ресурса</summary>
        public readonly RuntimeId ItemId;

        public readonly ItemConfig Item;

        /// <summary>Иконка ресурса</summary>
        public readonly Sprite Icon;

        /// <summary>Сколько требуется</summary>
        public readonly int RequiredAmount;

        /// <summary>Сколько есть у игрока</summary>
        public readonly int OwnedAmount;

        /// <summary>Достаточно ли ресурса</summary>
        public readonly bool IsEnough;

        public RecipeRequirementDto(
            RuntimeId itemId,
            ItemConfig item,
            Sprite icon,
            int requiredAmount,
            int ownedAmount,
            bool isEnough)
        {
            ItemId = itemId;
            Item = item;
            Icon = icon;
            RequiredAmount = requiredAmount;
            OwnedAmount = ownedAmount;
            IsEnough = isEnough;
        }
    }
}