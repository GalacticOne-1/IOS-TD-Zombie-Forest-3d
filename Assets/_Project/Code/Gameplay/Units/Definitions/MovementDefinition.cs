using System;

namespace Galactic1.Code.Gameplay.Units.Definitions
{
    /// <summary>
    /// Immutable runtime movement payload.
    ///
    /// Используется:
    /// - UnitMover
    /// - ChasingState
    /// - RoamingState
    /// - Navigation systems
    ///
    /// НЕ содержит runtime state.
    /// НЕ зависит от Unity components.
    /// </summary>
    public sealed class MovementDefinition
    {
        public float WalkSpeed { get; }
        public float RunSpeed { get; }

        public float RotationSpeed { get; }

        public float RepathInterval { get; }

        public float StoppingDistance { get; }

        public MovementDefinition(
            float walkSpeed,
            float runSpeed,
            float rotationSpeed,
            float repathInterval,
            float stoppingDistance)
        {
            WalkSpeed = walkSpeed;
            RunSpeed = runSpeed;

            RotationSpeed = rotationSpeed;

            RepathInterval = repathInterval;
            StoppingDistance = stoppingDistance;
        }
    }
}