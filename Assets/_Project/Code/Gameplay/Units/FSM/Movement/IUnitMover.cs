using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    public interface IUnitMover
    {
        void MoveTo(Vector3 destination);
        void Stop();
        void Die();
        bool HasArrived { get; }
        float RemainingDistance { get; }
    }
}