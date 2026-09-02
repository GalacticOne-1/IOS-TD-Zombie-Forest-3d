using UnityEngine;
using System.Collections.Generic;

namespace Galactic1.Systems
{
    
    /// <summary>
    /// Пул AudioSource для многократного использования SFX
    /// </summary>
    public class AudioSourcePool : MonoBehaviour
    {
        public AudioSource prefab; // Префаб AudioSource
        public int poolSize = 10; // Количество источников в пуле
        private List<AudioSource> pool = new List<AudioSource>();

        private void Awake()
        {
            // Создаем пул заранее
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource obj = Instantiate(prefab, transform);
                obj.gameObject.SetActive(false);
                pool.Add(obj);
            }
        }

        /// <summary>
        /// Берет свободный AudioSource из пула
        /// </summary>
        public AudioSource GetAudioSource()
        {
            foreach (var source in pool)
            {
                if (!source.gameObject.activeInHierarchy)
                {
                    source.gameObject.SetActive(true);
                    return source;
                }
            }

            // Если все заняты — создаем новый
            AudioSource obj = Instantiate(prefab, transform);
            pool.Add(obj);
            return obj;
        }

        /// <summary>
        /// Возвращает AudioSource обратно в пул
        /// </summary>
        public void ReleaseAudioSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
        }
    }

}