using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    public interface ITargetInfo
    {
        string TargetId { get; }
        IUnitSceneContext Unit { get; }
        Vector3 Position { get; }
        Vector3 AimPoint { get; }
        bool IsDead { get; }
        Vector3 GetClosestPoint(Vector3 fromPosition);
    }
}