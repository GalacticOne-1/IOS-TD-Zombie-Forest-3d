using System.Collections.Generic;

namespace Galactic1.Code.Systems.Raid.Survivors
{
    /// <summary>
    /// Общий rooster-runtime для группы юнитов, живущих в рамках рейда.
    /// Общая логика для SquadRuntime и CampDefenderRuntime — без дублирования.
    /// </summary>
    public abstract class RaidUnitRosterRuntime
    {
        protected readonly List<RaidUnitRuntime> units;
        public IReadOnlyList<RaidUnitRuntime> Units => units;

        protected RaidUnitRosterRuntime(List<RaidUnitRuntime> units)
        {
            this.units = units;
        }

        public RaidUnitRuntime GetUnit(string unitId)
        {
            foreach (var unit in units)
                if (unit.Id == unitId)
                    return unit;
            return null;
        }

        public int CasualtiesCount
        {
            get
            {
                int count = 0;
                foreach (var unit in units)
                    if (unit.Stats.IsDead)
                        count++;
                return count;
            }
        }

        public bool HasAliveUnits
        {
            get
            {
                foreach (var unit in units)
                    if (!unit.Stats.IsDead)
                        return true;
                return false;
            }
        }

        public virtual void Tick(float dt)
        {
            foreach (var unit in units)
                unit.Tick(dt);
        }
    }
}