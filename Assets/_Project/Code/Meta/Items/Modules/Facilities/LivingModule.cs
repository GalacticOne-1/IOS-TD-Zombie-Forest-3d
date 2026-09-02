using System;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Provides living space for units.
    /// </summary>
    [Serializable]
    public class LivingModule : FacilityModule
    {
        public int capacity;
        public float comfortBonus;
        public float moraleBonus;
    }
}