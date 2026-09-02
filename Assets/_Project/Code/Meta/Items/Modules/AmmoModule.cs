using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Items;
using Galactic1.Core.Enums;
using Galactic1.Game.UI.Stats.DTO;
using Galactic1.UI;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Defines ammo item.
    /// </summary>
    [System.Serializable]
    public class AmmoModule : ItemModule, ILinkedItemsProvider
    {
        [SerializeField] private AmmoDefinition ammoType;
        //[SerializeField] private int amount;
        
        [Tooltip("(0 = обычные, 1 = бронебойные, и т.д.)")]
        [SerializeField] private int priority;

        public AmmoDefinition AmmoType => ammoType;
        //public int Amount => amount;

        public int Priority => priority;

        
        
        public override void CollectDescriptors(List<DescriptorDisplayEntry> list)
        {
            // list.Add(new(DescriptorId.AmmoType, 
            //     "Ammo", ValueType.Enum));
        }
        
        
        public override void BuildTooltip(ref TooltipItemDto data)
        {
            data.descriptors = new[]
            {
                new DescriptorDisplayEntry()
                {
                    DescriptorId = DescriptorId.AmmoType,
                    RawValue = "Ammo",
                    ValueType = ValueType.String
                }
            };
        }
        
        
        
        
        public (StatId, List<RuntimeId>) LinkedItems()
        {
            // используемые боприпасы
            var ammoIds = GameContent.Weapons.
                FindAllWeaponsUsingAmmo(ammoType.Id)
                .Select(ammo => ammo.Id)
                .ToList();

            return (StatId.LinkedWeapons, ammoIds);
        }
    }
}