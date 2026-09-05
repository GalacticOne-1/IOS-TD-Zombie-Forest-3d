using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Game.UI.Buildings.DTO
{
    public sealed class TavernOfferDTO
    {
        public string Id;
        public string Name;
        public string ArchetypeId;
        public int Level;

        public Sprite Portrait;

        public GearSlotDTO Weapon;
        public IReadOnlyList<GearSlotDTO> Gear;
        public IReadOnlyList<StatDTO> Stats;

        public bool CanHire;
        public PurchaseType PurchaseType;
        public int PremiumCost;
    }
    
    public sealed class GearSlotDTO
    {
        public bool Disable; // true - слот пустой
        
        public Sprite Icon;
        public int DurabilityPrcnt;
        public int Durability;
        public float Durability01;
        public ItemRarity Rarity;

        /// <summary>
        /// Только для подсказки !!!
        /// </summary>
        public ItemConfig Item;
    }

    public sealed class StatDTO
    {
        public string Name;
        public int Value;
    }
}