using System;
using Galactic1.Code.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Movement
{
    public readonly struct MoveRequest
    {
        public readonly Vector3 Destination;
        public readonly WorldInputDispatcher.MoveMode Mode;
        public readonly Action<NavigationMoveResult> Callback;

        public MoveRequest(
            Vector3 destination,
            WorldInputDispatcher.MoveMode mode,
            Action<NavigationMoveResult> callback = null)
        {
            Destination = destination;
            Mode = mode;
            Callback = callback;
        }
    }
}