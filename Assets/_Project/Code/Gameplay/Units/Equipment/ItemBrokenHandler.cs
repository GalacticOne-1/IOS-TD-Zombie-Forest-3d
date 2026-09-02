using System;
using Galactic1.Code.UI.World;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Localisation;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Equipment
{
    /// <summary>
    /// Обрабатывает поломку предмета.
    /// Работает для UnitRuntime и RaidUnitRuntime одинаково.
    /// Не знает про сцену — получает позицию через делегат.
    /// </summary>
    public sealed class ItemBrokenHandler
    {
        private readonly EquipmentRuntimeService _equipment;
        private readonly Func<GameObject> _getInstance; // ← позиция из сцены

        public ItemBrokenHandler(
            EquipmentRuntimeService equipment,
            Func<GameObject> getInstance)
        {
            _equipment = equipment;
            _getInstance = getInstance;

            _equipment.OnItemBroken += Handle;
        }

        private void Handle(ItemConfig item)
        {
            var instance = _getInstance();
            if (instance == null) 
                return;
            
            var text = item.Classification.category switch
            {
                ItemCategory.Weapon => ServiceLocator.Current.Get<LocalisationService>().Data.weapon_broken,
                ItemCategory.Armor => ServiceLocator.Current.Get<LocalisationService>().Data.armor_broken,
            };

            ServiceLocator.Current.Get<WorldToastSystem>().ShowStatus(instance.transform.position, text);
        }

        public void Dispose() => _equipment.OnItemBroken -= Handle;
    }
}