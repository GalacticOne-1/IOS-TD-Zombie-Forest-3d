using System.Collections.Generic;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Temporary selection of units entering a specific raid.
    /// Exists only before mission start.
    /// </summary>
    public sealed class TacticalSelectionSystem
    {
        private GameLoopContext context;
        private int maxRaidSize = 6;

        //public IReadOnlyList<UnitRuntime> SelectedUnits => context.TacticalSelectedUnits;

        public TacticalSelectionSystem(GameLoopContext ctx)
        {
            context = ctx;
        }

        //public void Select(UnitRuntime unit) { ... }
        //public void Deselect(UnitRuntime unit) { ... }
    }
}