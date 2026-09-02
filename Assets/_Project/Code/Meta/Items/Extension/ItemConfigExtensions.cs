using System.Collections.Generic;
using System.Linq;

namespace Galactic1.Game.Meta.Items
{
    public static class ItemConfigExtensions
    {
        /// <summary>
        /// Возвращает модуль здания (IFacilityModule) для ItemConfig или null, если отсутствует.
        /// </summary>
        public static FacilityModule GetFacilityModule(this ItemConfig itemConfig)
        {
            return itemConfig.Modules.OfType<FacilityModule>().FirstOrDefault();
        }
        
        /// <summary>
        /// Проверка, является ли ItemConfig зданием
        /// </summary>
        public static bool IsFacility(this ItemConfig itemConfig)
        {
            return itemConfig.Modules.OfType<FacilityModule>().Any();
        }
        
        
        public static IEnumerable<FacilityModule> GetFacilityModules(this ItemConfig itemConfig)
        {
            return itemConfig.Modules.OfType<FacilityModule>();
        }

    }
}