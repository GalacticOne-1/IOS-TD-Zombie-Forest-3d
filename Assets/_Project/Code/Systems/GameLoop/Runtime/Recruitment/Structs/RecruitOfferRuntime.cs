using System;
using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Runtime-предложение найма.
    /// Не является юнитом до момента найма.
    /// </summary>
    public sealed class RecruitOfferRuntime
    {
        public string Id { get; }
        public UnitArchetypeConfig Archetype { get; }
        public UnitIdentity Identity { get; }
        public int Level { get; }
        public RecruitCategory Category { get; }

        
        public PurchaseType PurchaseType { get; }
        public int PremiumCost { get; }

        
        public RecruitEquipmentLoadout Equipment { get; }
        public IReadOnlyList<SkillConfig> Skills { get; }
        
        

        private RecruitOfferRuntime(
            string id,
            UnitArchetypeConfig archetype,
            UnitIdentity identity,
            RecruitCategory category,
            int level,
            PurchaseType purchaseType,
            int premiumCost,
            RecruitEquipmentLoadout equipment,
            IReadOnlyList<SkillConfig> skills)
        {
            Id = id;
            Archetype = archetype;
            Identity = identity;
            Level = level;
            Category = category;
            PurchaseType = purchaseType;
            PremiumCost = premiumCost;
            Equipment = equipment;
            Skills = skills ?? Array.Empty<SkillConfig>();
        }

        public static RecruitOfferRuntime CreateCommon(
            UnitArchetypeConfig archetype,
            UnitIdentity identity,
            RecruitEquipmentLoadout equipment)
        {
            return new RecruitOfferRuntime(
                Guid.NewGuid().ToString("N"),
                archetype,
                identity,
                RecruitCategory.Common,
                0,
                PurchaseType.Free,
                0,
                equipment,
                null);
        }

        public static RecruitOfferRuntime CreateExperienced(
            UnitArchetypeConfig archetype,
            UnitIdentity identity,
            int level,
            PurchaseType purchaseType,
            int premiumCost,
            RecruitEquipmentLoadout equipment)
        {
            return new RecruitOfferRuntime(
                Guid.NewGuid().ToString("N"),
                archetype,
                identity,
                RecruitCategory.Experienced,
                level,
                purchaseType,
                premiumCost,
                equipment,
                null);
        }

        public static RecruitOfferRuntime CreateSpecialist(
            UnitArchetypeConfig archetype,
            UnitIdentity identity,
            int level,
            PurchaseType purchaseType,
            int premiumCost,
            RecruitEquipmentLoadout equipment,
            IReadOnlyList<SkillConfig> skills)
        {
            return new RecruitOfferRuntime(
                Guid.NewGuid().ToString("N"),
                archetype,
                identity,
                RecruitCategory.Specialist,
                level,
                purchaseType,
                premiumCost,
                equipment,
                skills);
        }
    }
}