using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Базовый класс authoring-данных одного guidance-условия. По структуре — аналог
    /// TutorialObjectiveDefinition (ScriptableObject + typeId + editor Validate), но
    /// семантически не связан с завершением шага: маппинг Definition → runtime condition —
    /// в TutorialGuidanceFactory (Objectives namespace, по тому же принципу, что
    /// TutorialObjectiveFactory для объективов) — Authoring не знает о Runtime-типах.
    /// </summary>
    public abstract class TutorialGuidanceConditionDefinition : ScriptableObject
    {
        public abstract string ConditionTypeId { get; }

#if UNITY_EDITOR
        /// <summary>Точка расширения для валидации конкретного условия в редакторе.</summary>
        public virtual bool Validate(out string error)
        {
            error = null;
            return true;
        }
#endif
    }
}
