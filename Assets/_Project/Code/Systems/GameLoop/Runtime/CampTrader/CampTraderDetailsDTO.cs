using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Game.UI.Buildings.DTO
{
    /// <summary>
    /// Детали трейдера (магазин).
    /// </summary>
    public sealed class CampTraderDetailsDTO : IFacilityDetailsDTO
    {
        public FacilityType Type => FacilityType.CampTrader;

        public IReadOnlyList<CampTraderOfferDTO> Offers { get; }

        public CampTraderDetailsDTO(IReadOnlyList<CampTraderOfferDTO> offers)
        {
            Offers = offers;
        }
    }

    public sealed class CampTraderOfferDTO
    {
        public string Id;

        public string ItemNameLid;
        public Sprite Icon;
        public ItemRarity Rarity;

        public bool UsesDurability;
        public int Durability;
        public float Durability01;

        public int Amount;

        public int Cost;

        /// <summary>
        /// Только для подсказки (тултип) !!!
        /// </summary>
        public ItemConfig Item;
    }
}
