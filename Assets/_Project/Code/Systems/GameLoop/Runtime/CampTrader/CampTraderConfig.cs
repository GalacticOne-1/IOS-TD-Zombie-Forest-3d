using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Meta.Configs.Trader
{
    /// <summary>
    /// База предложений трейдера (список конкретных офферов на покупку).
    /// </summary>
    [CreateAssetMenu(
        fileName = "CampTraderConfig",
        menuName = "Game Configs/_AD_/Camp Trader/Camp Trader Config")]
    public sealed class CampTraderConfig : ScriptableObject
    {
        [field: SerializeField] public List<TraderOfferConfig> Offers { get; private set; }
        
        
    }

    /// <summary>
    /// Один оффер трейдера: предмет + стоимость в soft-валюте.
    /// </summary>
    [Serializable]
    public sealed class TraderOfferConfig
    {
        [SerializeField] private ItemId itemId;
        [SerializeField] private int durability;
        [SerializeField] private int amount;
        [SerializeField] private int cost;

        public ItemConfig Item => GameContent.Items.Get(itemId);
        public int Amount => amount;
        public int Durability => (int)(Item.Physical.maxDurability * (durability / 100f));
        public int Cost => cost;
    }
}
