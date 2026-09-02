using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    public readonly struct CombatTraceEvent : IEvent
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;
        public readonly bool Hit;
        public readonly Vector3 EndPoint;

        public CombatTraceEvent(
            Vector3 origin,
            Vector3 direction,
            bool hit,
            Vector3 endPoint)
        {
            Origin = origin;
            Direction = direction;
            Hit = hit;
            EndPoint = endPoint;
        }
    }
}