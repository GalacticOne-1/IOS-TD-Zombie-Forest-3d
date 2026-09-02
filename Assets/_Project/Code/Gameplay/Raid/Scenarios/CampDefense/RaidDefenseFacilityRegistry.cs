using System.Collections.Generic;
using Galactic1.Code.Systems.Raid.Buildings;
using Galactic1.Code.Systems.Runtime.Building;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Runtime-состояние защитных сооружений лагеря во время Camp Defense.
    /// </summary>
    public sealed class RaidDefenseFacilityRegistry
    {
        private readonly List<RaidCombatFacilityRuntime> _facilities;

        public IReadOnlyList<RaidCombatFacilityRuntime> Facilities => _facilities;
        
        
        

        public RaidDefenseFacilityRegistry(List<CombatFacilityRuntime> metaFacilities)
        {
           var factory = new RaidFacilityFactory();

           _facilities = new();
           foreach (var facilityRuntime in metaFacilities)
           {
               var snapshot = factory.Create(facilityRuntime);
               _facilities.Add(new RaidCombatFacilityRuntime(snapshot));
           }
        }

        public RaidCombatFacilityRuntime GetFacility(string facilityId)
        {
            foreach (var facility in _facilities)
            {
                if (facility.Id == facilityId)
                    return facility;
            }

            return null;
        }
        
        
        public RaidCombatFacilityRuntime GetFacility(FacilityType type)
        {
            foreach (var facility in _facilities)
            {
                if (facility.Type == type)
                    return facility;
            }

            return null;
        }
        
        
        

        public int DestroyedCount
        {
            get
            {
                int count = 0;

                foreach (var facility in _facilities)
                {
                    if (facility.Stats.IsDead)
                        count++;
                }

                return count;
            }
        }

        public int AliveCount
        {
            get
            {
                int count = 0;

                foreach (var facility in _facilities)
                {
                    if (!facility.Stats.IsDead)
                        count++;
                }

                return count;
            }
        }

        public bool HasAliveFacilities => AliveCount > 0;

        public void Tick(float dt)
        {
            foreach (var facility in _facilities)
                facility.Tick(dt);
        }
    }
}