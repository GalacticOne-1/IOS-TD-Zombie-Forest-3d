
using Galactic1.Code.Systems.Raid;

namespace Galactic1.Code.Systems.Runtime.Enemy
{
    public interface ISceneEnemy : IUnitSceneContext
    {
        IEnemyUnitRuntime Runtime { get; }
        
        float ThreatLevel { get; }

    }
}