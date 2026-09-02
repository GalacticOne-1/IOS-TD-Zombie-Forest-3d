using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Noise
{
    /// <summary>
    /// Game service. Принимает NoiseEvent и оповещает слушателей в радиусе.
    ///
    /// НЕ знает: AI, aggro, damage, team, faction.
    /// Только: spatial query + broadcast.
    ///
    /// Использование:
    ///   var noise = ServiceLocator.Current.Get<NoiseSystem>();
    ///   noise.Emit(new NoiseEvent(pos, radius, NoiseType.Gunshot));
    /// </summary>
    public sealed class NoiseSystem : IGameService
    {
        private readonly List<INoiseListener> _listeners = new(64);
        
        public event Action<NoiseEvent> OnNoiseEmitted;

        // ── Registration ──────────────────────────────────────────────────

        public void Register(INoiseListener listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void Unregister(INoiseListener listener)
        {
            _listeners.Remove(listener);
        }

        // ── Emit ──────────────────────────────────────────────────────────

        /// <summary>
        /// Рассылает NoiseEvent всем слушателям в радиусе evt.Radius.
        /// O(n) по количеству зарегистрированных слушателей.
        /// Для большого мира — заменить на spatial grid.
        /// </summary>
        public void Emit(NoiseEvent evt)
        {
            float radiusSq = evt.Radius * evt.Radius;

            foreach (var listener in _listeners)
            {
                float distSq = (listener.Position - evt.Position).sqrMagnitude;
                if (distSq <= radiusSq)
                    listener.OnNoiseHeard(evt);
            }
            
            OnNoiseEmitted?.Invoke(evt);
        }
    }
}