namespace Galactic1.Systems
{
    using UnityEngine;
    using System.Collections.Generic;

    [System.Serializable]
    public class MusicLayer
    {
        public string name;        // Имя слоя (Base, Combat, Tension)
        public AudioClip clip;     // Аудиофайл
        public float volume = 1f;  // Базовая громкость
        [Range(0, 1)] public float weight = 0f; // Текущий вес (0–1)
    }

    [System.Serializable]
    public class AdaptiveMusicTrack
    {
        public string trackName;            // Имя трека (например BattleTheme)
        public List<MusicLayer> layers;     // Слои трека
    }

}