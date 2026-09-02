using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Configs.Galactic1.Code.GameDatabase
{
    /// <summary>
    /// Глобальный typed access к часто используемым RuntimeId.
    ///
    /// Используется только как read-only facade поверх GameIds config.
    /// Не хранит gameplay state.
    /// </summary>
    public static class GameIdProvider
    {
        private static GameIds _config;

        public static void Initialize(GameIds config)
        {
            _config = config;
        }
        
        
        
        
        // =========================================================
        // CURENCY
        // =========================================================

        public static CurrencyId Coins => _config.Coins;
        public static CurrencyId Experience => _config.Experience;
        
        
        public static LocationId Home => _config.Home;

        // =========================================================
        // FACILITIES
        // =========================================================

        public static ItemId Transport => _config.Transport;
        public static ItemId Tavern => _config.Tavern;
        public static ItemId Garage => _config.Garage;
        public static ItemId MainContainer => _config.MainContainer;

        // =========================================================
        // VFX
        // =========================================================

        public static VfxId StunVfx => _config.StunVfx;
        public static VfxId FacilityExplosionVfx => _config.FacilityExplosionVfx;
    }
}