using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    /// <summary>Упорядоченный список волн для одной локации Camp Defense.</summary>
    [CreateAssetMenu(
        fileName = "WaveConfig",
        menuName = "Game Configs/Enemy/Wave Config")]
    public sealed class WaveConfig : ScriptableObject
    {
        public List<WaveDefinition> Waves = new();
    }
}