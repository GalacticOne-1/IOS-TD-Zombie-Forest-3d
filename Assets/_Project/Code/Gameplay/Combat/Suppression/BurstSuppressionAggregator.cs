using System.Collections.Generic;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Systems.Raid;

namespace Galactic1.Code.Gameplay.Combat.Suppression
{
    /// <summary>
    /// Aggregates suppression across a burst and raises GameplaySuppressionEvent per unit.
    ///
    /// Phase 3 addition: raises GameplaySuppressionEvent after flushing,
    /// so AI and morale systems can react without polling stats.
    ///
    /// Replaces Phase 2 version.
    /// </summary>
    public sealed class BurstSuppressionAggregator
    {
        private readonly Dictionary<IUnitSceneContext, float> _buffer = new();

        public void Add(IUnitSceneContext target, float amount)
        {
            if (_buffer.TryGetValue(target, out float existing))
                _buffer[target] = existing + amount;
            else
                _buffer[target] = amount;
        }

        /// <summary>
        /// Applies suppression and raises GameplaySuppressionEvent per unit.
        /// Call once after the damage loop, before batch.Dispose().
        /// </summary>
        public void Flush(SuppressionSystem suppression)
        {
            foreach (var pair in _buffer)
            {
                suppression.Apply(pair.Key, pair.Value);

                EventBus<CombatSuppressionEvent>.Raise(
                    new CombatSuppressionEvent(pair.Key, pair.Value));
            }

            _buffer.Clear();
        }
    }
}