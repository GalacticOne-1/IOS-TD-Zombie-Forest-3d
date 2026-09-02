using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Configs
{
    [CreateAssetMenu(fileName = "GameIds", menuName = "Game Configs/IDs/Game Ids")]
    public sealed class GameIds : ScriptableObject
    {
        [Header("Currency")] 
        public CurrencyId Coins;
        public CurrencyId Experience;
        
        
        [Space]
        public LocationId Home;
        
        [Header("Camp")]
        public ItemId Transport;
        public ItemId Tavern;
        public ItemId Garage;
        public ItemId MainContainer;

        [Header("VFX")]
        public VfxId StunVfx;
        public VfxId FacilityExplosionVfx;
    }
}

