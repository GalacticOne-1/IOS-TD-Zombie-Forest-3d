using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Construction.Repair
{
    /// <summary>
    /// Одна строка требования ремонта — DTO для UI.
    /// </summary>
    public class RepairRequirementEntry
    {
        public ItemConfig Item;
        public int Required;
        public int Owned;

        public int Missing => Required - Owned > 0 ? Required - Owned : 0;
        public bool IsEnough => Owned >= Required;
    }
}