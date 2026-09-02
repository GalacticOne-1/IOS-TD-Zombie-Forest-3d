
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Inventory.Abstractions;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Infrastructure
{
    /// <summary>
    /// Адаптер между системой оружия и инвентарём транспорта.
    /// Ищет патроны по AmmoDefinition.AmmoId, совпадающему с AmmoModule.AmmoType.AmmoId у предметов.
    /// </summary>
    public sealed class InventoryAmmoAdapter : IAmmoInventory
    {
        private readonly IInventoryResourcesPort _port;
        private readonly AmmoRegistry _ammoRegistry;

        public InventoryAmmoAdapter(IInventoryResourcesPort port, AmmoRegistry ammoRegistry)
        {
            _port = port;
            _ammoRegistry = ammoRegistry;
        }

        // public int PeekAmmo(RuntimeId ammoId)
        //     => _port.GetTotalAmount(ammoId);
        //
        // public int TakeAmmo(RuntimeId ammoId, int amount)
        // {
        //     int available = _port.GetTotalAmount(ammoId);
        //     int toTake = System.Math.Min(available, amount);
        //     if (toTake > 0) _port.TrySpend(ammoId, toTake);
        //     return toTake;
        // }
        
        // Суммарный счёт всех патронов нужного калибра
        public int PeekAmmo(RuntimeId caliberId)
        {
            var configs = _ammoRegistry.GetByCaliber(caliberId);
            int total = 0;
            for (int i = 0; i < configs.Count; i++)
                total += _port.GetTotalAmount(configs[i].Id);
            return total;
        }

        public int TakeAmmo(RuntimeId caliberId, int amount)
        {
            // === получаем список разных видов одного калибра
            var configs = _ammoRegistry.GetByCaliber(caliberId);

            // todo
            // сейчас разные патроны идут как одинаковые
            // т.е разные типы не реализованы (бронебойные, скоростыне и пр)
            
            int remaining = amount;
            for (int i = 0; i < configs.Count && remaining > 0; i++) 
            {
                int available = _port.GetTotalAmount(configs[i].Id);
                int toTake = Mathf.Min(available, remaining);
                if (toTake > 0)
                {
                    _port.TrySpend(configs[i].Id, toTake);
                    remaining -= toTake;
                }
            }

            return amount - remaining;
        }
    }
}