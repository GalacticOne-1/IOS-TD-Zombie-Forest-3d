using System.Collections.Generic;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Core.Enums;
using Galactic1.Meta.Configs.Recruitment;
using R3;

namespace Galactic1.Game.Buildings.Proxy
{
    /// <summary>
    /// Proxy-обёртка над RecruitOfferData.
    /// Является реактивным представлением предложения найма.
    /// Не содержит бизнес-логики.
    /// </summary>
    public sealed class RecruitOfferProxy
    {
        public RecruitOfferData Origin { get; }

        public string Id => Origin.Id;
        public int Level => Origin.Level;
        
        //public UnitArchetypeConfig Archetype => Origin.a
        public UnitIdentity Identity => Origin.Identity;
        
        public RecruitCategory Category => (RecruitCategory)Origin.Category;

        public PurchaseType PurchaseType => (PurchaseType)Origin.PurchaseType;
        public int PremiumCost => Origin.PremiumCost;

        public RecruitEquipmentLoadout Equipment => Origin.Equipment;

        private readonly List<SkillConfig> _skills;
        public IReadOnlyList<SkillConfig> Skills => _skills;

        public RecruitOfferProxy(RecruitOfferData origin)
        {
            Origin = origin;
        }

    }
}