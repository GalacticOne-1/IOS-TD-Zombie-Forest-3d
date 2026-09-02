using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    [Serializable]
    public sealed class WaveDefinition
    {
        public string WaveId;

        public WaveCompletionMode CompletionMode = WaveCompletionMode.AllEnemiesDead;

        [Tooltip("Используется только при CompletionMode == TimerOnly.")]
        public float DelayBeforeNextWave = 60f;

        public List<WaveSpawnInstruction> Instructions = new();
    }
}