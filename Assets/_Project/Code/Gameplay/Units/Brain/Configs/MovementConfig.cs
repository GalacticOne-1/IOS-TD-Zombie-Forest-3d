using UnityEngine;

namespace Galactic1.Game.Meta.Enemy
{
    [CreateAssetMenu(fileName = "MovementConfig", menuName = "Game Configs/AI/MovementConfig")]
    public sealed class MovementConfig : ScriptableObject
    {
        public float WalkSpeed;
        public float RunSpeed;

        public float RotationSpeed;

        public float RepathInterval;

        public float StoppingDistance;
    }
}