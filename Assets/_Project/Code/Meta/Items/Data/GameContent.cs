
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Configs;
using Galactic1.Game.Meta.Economy;
using Galactic1.Game.Meta.Enemy;
using Galactic1.Game.Meta.Items;
using Galactic1.Items;

namespace Galactic1.Code.GameDatabase
{
    /// <summary>
    /// - хранить список всех ItemConfig
    /// - auto-discovery
    /// - validation
    /// - duplicate checks
    /// - build pipelines
    /// </summary>
    public static class GameContent
    {
        public static CurrencyRegistry Currency { get; private set; }
        public static ItemIdRegistry ItemIds { get; private set; }
        public static ItemRegistry Items { get; private set; }
        public static AmmoRegistry Ammo { get; private set; }
        public static WeaponRegistry Weapons { get; private set; }
        public static FacilityRegistry Facilities { get; private set; }
        public static EnemyRegistry Enemies { get; private set; }

        //
        // public static RecipeRegistry Recipes { get; private set; }
        //
        // public static LootTableRegistry LootTables { get; private set; }

        public static void Initialize(IConfigProvider provider)
        {
            // валюта, опыт и пр
            Currency = new CurrencyRegistry(provider.Get<CurrencyDatabase>().Currencies);
            
            // #1 items
            var items = provider.Get<ItemDatabase>().Items;
            
            ItemIds = new ItemIdRegistry(items);
            Items = new ItemRegistry(items);
            Facilities = new FacilityRegistry(items);

            // #2 ammo
            Ammo = new AmmoRegistry(items);
            Weapons = new WeaponRegistry(items);
            
            // #3 creatures
            Enemies = new EnemyRegistry(provider.Get<ZombieVariantDatabase>().All);

        }
        
        
        
        public static bool ResolveItem(string guid, out ItemConfig config)
        {
            config = null;


            if (string.IsNullOrEmpty(guid) || !ItemIds.TryGet(guid, out var itemId))
                return false;

            return Items.TryGet(itemId, out config);
        }

        public static bool ResolveFacility(string guid, out FacilityModule facility)
        {
            facility = null;
            
            if (string.IsNullOrEmpty(guid) || !ItemIds.TryGet(guid, out var itemId))
                return false;

            return Facilities.TryGet(itemId, out facility);
        }
    }
}