using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Dev.Spawn
{
    /// <summary>
    /// Универсальный спавнер объектов.
    /// Поддерживает список префабов и смещение при спавне.
    /// Можно выполнять действие на спавненном компоненте.
    /// </summary>
    public class ObjectSpawner : MonoBehaviour
    {
        [Header("Spawn BasicSettings")]
        [SerializeField] private List<GameObject> prefabs;   // список префабов
        [SerializeField] private Vector3 startPosition;      // стартовая позиция
        [SerializeField] private Vector3 offsetPerSpawn = new Vector3(1f, 0f, 0f); // смещение после каждого спавна

        private Vector3 nextSpawnPosition;

        private void Awake()
        {
            nextSpawnPosition = startPosition;

            EventBus<SceneReadyEvent>.Register(new EventBinding<SceneReadyEvent>(() =>
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    Spawn(i);
                }
            }));
        }

        /// <summary>
        /// Спавн объекта по индексу в списке префабов.
        /// </summary>
        public GameObject Spawn(int prefabIndex)
        {
            if (prefabs == null || prefabs.Count == 0)
            {
                Debug.LogWarning("No prefabs assigned to ObjectSpawner!");
                return null;
            }

            prefabIndex = Mathf.Clamp(prefabIndex, 0, prefabs.Count - 1);

            GameObject obj = Instantiate(prefabs[prefabIndex], transform);
            obj.transform.position = nextSpawnPosition;
            nextSpawnPosition += offsetPerSpawn;
            return obj;
        }

        /// <summary>
        /// Спавн случайного объекта из списка.
        /// </summary>
        public GameObject SpawnRandom()
        {
            if (prefabs == null || prefabs.Count == 0)
                return null;

            int index = UnityEngine.Random.Range(0, prefabs.Count);
            return Spawn(index);
        }

        /// <summary>
        /// Спавн с вызовом действия на компоненте T.
        /// </summary>
        public GameObject SpawnWithAction<T>(Action<T> action) where T : Component
        {
            GameObject obj = SpawnRandom();
            if (obj != null && action != null)
            {
                T component = obj.GetComponent<T>();
                if (component != null)
                    action(component);
            }
            return obj;
        }

        /// <summary>
        /// Сброс позиции спавна на стартовую.
        /// </summary>
        public void ResetSpawnPosition()
        {
            nextSpawnPosition = startPosition;
        }
    }
}
