using System;
using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.Runtime
{
    public class RecruitOfferFactory
    {
        public RecruitOfferData CreateCommon(
            UnitArchetypeConfig archetype,
            UnitIdentity identity,
            RecruitEquipmentLoadout equipment)
        {
            return new RecruitOfferData()
            {
                Id = Guid.NewGuid().ToString("N"),
                //archetype,
                Identity = identity,
                Category = (byte)RecruitCategory.Common,
                Level = 0,
                PurchaseType = (byte)PurchaseType.Free,
                PremiumCost = 0,
                Equipment = equipment,
                // skill
            };
        }

        public RecruitOfferData CreateExperienced(
            UnitArchetypeConfig archetype,
            UnitIdentity identity,
            int level,
            PurchaseType purchaseType,
            int premiumCost,
            RecruitEquipmentLoadout equipment)
        {
            return new RecruitOfferData()
            {
                Id = Guid.NewGuid().ToString("N"),
                //archetype,
                Identity = identity,
                Category = (byte)RecruitCategory.Experienced,
                Level = level,
                PurchaseType = (byte)purchaseType,
                PremiumCost = premiumCost,
                Equipment = equipment,
                // skill
            };
        }

        public RecruitOfferData CreateSpecialist(
            UnitArchetypeConfig archetype,
            UnitIdentity identity,
            int level,
            PurchaseType purchaseType,
            int premiumCost,
            RecruitEquipmentLoadout equipment,
            IReadOnlyList<SkillConfig> skills)
        {
            return new RecruitOfferData()
            {
                Id = Guid.NewGuid().ToString("N"),
                //archetype,
                Identity = identity,
                Category = (byte)RecruitCategory.Specialist,
                Level = level,
                PurchaseType = (byte)purchaseType,
                PremiumCost = premiumCost,
                Equipment = equipment,
                // skill
            };
        }
    }
}