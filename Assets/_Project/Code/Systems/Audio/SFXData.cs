
namespace Galactic1.Systems
{
    using UnityEngine;

    [System.Serializable]
    public class SFXData
    {
        public string name; // Имя эффекта (совпадает с clip)
        public AudioClip clip; // Аудиофайл
        [Range(0f, 1f)] public float volume = 1f; // Базовая громкость
        public int maxInstances = 3; // Максимальное количество одновременных копий
    }

}