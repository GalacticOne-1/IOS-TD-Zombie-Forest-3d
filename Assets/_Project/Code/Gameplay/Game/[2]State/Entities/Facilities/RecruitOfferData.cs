
using Galactic1.Code.Systems.Runtime;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Game.Buildings.Proxy
{
    [System.Serializable]
    public class RecruitOfferData
    {
        public string Id;
        //public UnitArchetypeConfig Archetype;
        public UnitIdentity Identity;
        public int Level;
        public byte Category;


        public byte PurchaseType;
        public int PremiumCost;


        public RecruitEquipmentLoadout Equipment;
        //public IReadOnlyList<SkillConfig> Skills;
    }
}