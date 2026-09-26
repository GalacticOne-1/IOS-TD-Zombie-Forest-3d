using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// "Найди originTargetId, возьми count ближайших живых врагов". originTargetId — не враг,
    /// а ориентир (обычно WorldTutorialTargetBehaviour у точки спавна группы). Сам query не
    /// резолвит selection — это делает TutorialEnemyGroupSelectionService, которым владеет
    /// EnemyGroupKilledObjective; этот query для highlight-пайплайна только описывает намерение
    /// (по аналогии с TutorialUnitSearchQuery).
    /// </summary>
    [Serializable]
    public sealed class TutorialEnemySearchQuery : TutorialTargetQuery
    {
        public TutorialTargetId originTargetId;
        [Min(1)] public int count = 1;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (originTargetId == null) { error = "TutorialEnemySearchQuery: originTargetId is empty."; return false; }
            error = null;
            return true;
        }
#endif
    }
}