using System;
using Galactic1.Code.Gameplay.Combat.Cover;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.Units.Presentation;

namespace Galactic1.Code.Systems.Runtime.Enemy
{
    public sealed class EnemySceneAdapter : ISceneEnemy
    {
        private readonly IEnemyUnitRuntime _runtime;

        public string Id => _runtime.Id;

        public IEnemyUnitRuntime Runtime => _runtime;

        public IUnitRuntimeBase RuntimeBase => _runtime;

        public IUnitStatsScene Stats { get; }

        public float ThreatLevel => _runtime.ThreatLevel;
        public UnitCoverState Cover => UnitCoverState.None_;

        public event Action OnDeath;

        public EnemySceneAdapter(IEnemyUnitRuntime runtime)
        {
            _runtime = runtime;

            Stats = new UnitStatsSceneAdapter(runtime.Stats);

            Stats.OnDeath += HandleDeath;
        }

        private void HandleDeath()
        {
            OnDeath?.Invoke();
        }

        public void Dispose()
        {
            if (Stats != null)
            {
                Stats.OnDeath -= HandleDeath;
                Stats.Dispose();
            }
        }
    }
}