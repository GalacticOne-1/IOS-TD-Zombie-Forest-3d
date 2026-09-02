using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Meta.Configs.Recruitment
{
    /// <summary>
    /// Стартовая экипировка рекрута.
    /// Чистая runtime-модель.
    /// </summary>
    [System.Serializable]
    public sealed class RecruitEquipmentLoadout
    {
        
        public struct RecruitEquipmentLoadoutBox
        {
            public string Id;
            public int Durability;
            public int AmmoInMagazine;
        }
        
        public RecruitEquipmentLoadoutBox WeaponItem { get; }
        
        public IReadOnlyList<RecruitEquipmentLoadoutBox> ArmorItem { get; }
        

        public RecruitEquipmentLoadout(
            RecruitEquipmentLoadoutBox weaponItem,
            IReadOnlyList<RecruitEquipmentLoadoutBox> armorItem)
        {
            WeaponItem = weaponItem;
            ArmorItem = armorItem;
        }
    }

    
}