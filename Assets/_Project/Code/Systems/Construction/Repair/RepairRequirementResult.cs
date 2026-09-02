using System;
using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.Construction.Repair
{
    /// <summary>
    /// Полное состояние ремонта конкретного здания для UI.
    /// UI ничего не считает — только отображает эти поля.
    /// </summary>
    public class RepairRequirementResult
    {
        /// <summary>Может ли объект в принципе чиниться (есть ли у него HP).</summary>
        public bool IsRepairable = true;

        /// <summary>Нужен ли ремонт прямо сейчас (CurrentHP < MaxHP).</summary>
        public bool NeedsRepair;

        /// <summary>Хватает ли ресурсов на весь расчитанный список требований.</summary>
        public bool HasEnoughResources;

        public IReadOnlyList<RepairRequirementEntry> Entries = Array.Empty<RepairRequirementEntry>();

        public static RepairRequirementResult NotRepairable => new()
        {
            IsRepairable = false,
            NeedsRepair = false,
            HasEnoughResources = false
        };
    }
}