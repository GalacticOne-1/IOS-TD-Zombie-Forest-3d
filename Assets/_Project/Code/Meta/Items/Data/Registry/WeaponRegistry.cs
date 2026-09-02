using System.Collections.Generic;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Runtime lookup registry for items.
    /// </summary>
    public sealed class WeaponRegistry : RegistryBase<RuntimeId, ItemConfig>
    {
        public WeaponRegistry(IReadOnlyList<ItemConfig> items)
        {
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                if (!item.HasModule<WeaponModule>())
                    continue;

                if (map.ContainsKey(item.Id))
                {
                    DLog.Alert(
                        $"[WeaponRegistry] Duplicate weapon id '{item.Id.DebugKey}'",
                        EDlogColor.RED);

                    continue;
                }

                map.Add(item.Id, item);
            }

        }


        /// <summary>
        /// Собирает все оружие использующее тип боеприпаса
        /// </summary>
        /// <param name="ammoId"></param>
        /// <returns></returns>
        public IReadOnlyList<ItemConfig> FindAllWeaponsUsingAmmo(RuntimeId ammoId)
        {
            var result = new List<ItemConfig>();

            foreach (var item in map.Values)
            {
                var weapon = item.GetModule<WeaponModule>();

                if (weapon.SupportedAmmo.Id == ammoId)
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}