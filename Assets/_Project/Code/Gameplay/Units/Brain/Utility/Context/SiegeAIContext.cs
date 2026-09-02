using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Siege-версия AIContext. Perception-поля (VisibleTarget и т.д.) заполняются
    /// SiegeAIContextBuilder через TargetingUtility.FindNearestHostilePlayer —
    /// только игрок, здания сюда никогда не попадают.
    ///
    /// ТРЕБУЕТ: AIContext больше не sealed (см. Modified/AIContext.cs).
    /// </summary>
    public  class SiegeAIContext : AIContext
    {
        public ITargetInfo Headquarters;
        public SiegeObjective CurrentObjective;
        public ITargetInfo BlockingWall;
        public bool PathBlocked;
        public bool HasReachablePath;
        public float ObjectiveDistance;
        public Vector3 HeadquartersAttackPosition;
    }
}
