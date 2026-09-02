using Galactic1.Code.Gameplay.Combat.Events;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat
{
    /// <summary>
    /// Visual-only combat debug renderer.
    /// Draws actual shot trajectories.
    /// </summary>
    public sealed class CombatDebugDrawer
    {
        private readonly float _duration;

        private readonly EventBinding<CombatTraceEvent> _traceBinding;

        public CombatDebugDrawer(float duration = 3f)
        {
            _duration = duration;

            _traceBinding = new EventBinding<CombatTraceEvent>(OnTrace);

            EventBus<CombatTraceEvent>.Register(_traceBinding);
        }

        private void OnTrace(CombatTraceEvent e)
        {
            Debug.DrawLine(
                e.Origin,
                e.EndPoint,
                e.Hit ? Color.green : Color.yellow,
                _duration);
        }

        public void Dispose()
        {
            EventBus<CombatTraceEvent>.Deregister(_traceBinding);
        }
    }
}