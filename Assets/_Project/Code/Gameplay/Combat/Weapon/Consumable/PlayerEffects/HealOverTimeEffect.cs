using Galactic1.Code.Gameplay.Units.Abstractions;

namespace Galactic1.Code.Gameplay.Effect
{
    public sealed class HealOverTimeEffect : IActiveEffect
    {
        private readonly IUnitStatsRuntime _stats;
        private readonly float _healPerTick;
        private readonly float _tickInterval;
        private readonly float _duration;

        private float _elapsed;
        private float _tickTimer;

        public bool IsFinished => _elapsed >= _duration;

        public HealOverTimeEffect(
            IUnitStatsRuntime stats,
            float totalHeal,
            float duration,
            float tickInterval,
            bool divideAmount)
        {
            _stats = stats;
            _duration = duration;
            _tickInterval = tickInterval;
            _healPerTick = divideAmount
                ? totalHeal / (duration / tickInterval)
                : totalHeal;
        }

        public void Tick(float dt)
        {
            if (IsFinished) return;

            _elapsed += dt;
            _tickTimer += dt;

            while (_tickTimer >= _tickInterval)
            {
                _tickTimer -= _tickInterval;
                _stats.ModifyStat(StatId.Health, _healPerTick);
            }
        }
    }

}