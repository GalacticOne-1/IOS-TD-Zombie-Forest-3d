using System.Collections;
using System.Collections.Generic;
using Galactic1.Configs;
using Gameplay;
using Gameplay.AbstractFactory;
using UnityEngine;

namespace Galactic1.Structure
{

    public class EnemyPool : MonoBehaviour, IGameService
    {

        [SerializeField] private string[] ConfigsId;
        


        private Dictionary<string, Queue<GameObject>> poolDictionary = new();
        private Dictionary<string, GameObject> loadedPrefabs = new();
        private HashSet<string> loadingPaths = new();
        
        
        
        
        
        public void Launch()
        {
            // var enemyConfigs = ServiceLocator.Current.Get<SettingsProvider>().Lib.EntitiesConfigs.Enemies;
            // var playerUnitConfigs = ServiceLocator.Current.Get<SettingsProvider>().Lib.EntitiesConfigs.PlayerUnits;
            
            
            // для загрузки пула ищем нужные конфиги в библиотеке
            // foreach (var config in ConfigsId)
            // {
            //     CreatePool(FindConfig(config, enemyConfigs, playerUnitConfigs));
            // }
        }

        
        
        
        
        /// <summary>
        /// Для поиска одного конфига
        /// </summary>
        /// <param name="configId"></param>
        /// <returns></returns>
        // EntityConfig FindConfig(
        //     string configId,
        //     List<EnemyConfig> enemyConfigs = null,
        //     List<PlayerUnitConfig> playerUnitConfigs = null)
        // {
        //     if (enemyConfigs == null || enemyConfigs.Count == 0)
        //         enemyConfigs = ServiceLocator.Current.Get<SettingsProvider>().Lib.EntitiesConfigs.Enemies;
        //     
        //     if (playerUnitConfigs == null || playerUnitConfigs.Count == 0)
        //         playerUnitConfigs = ServiceLocator.Current.Get<SettingsProvider>().Lib.EntitiesConfigs.PlayerUnits;
        //     
        //     foreach (var configs in enemyConfigs)
        //         if (configs.ConfigId == configId)
        //             return configs;
        //         
        //     foreach (var configs in playerUnitConfigs)
        //         if (configs.ConfigId == configId)
        //             return configs;
        //         
        //     return null;
        // }
        
        
        
        

        /// <summary>
        /// Асинхронно загружает префаб из Resources, если он ещё не загружен
        /// </summary>
        public void Preload(EntityConfig entityConfig, System.Action onComplete = null)
        {
            if (loadedPrefabs.ContainsKey(entityConfig.ConfigId))
            {
                onComplete?.Invoke();
                return;
            }

            //if (loadingPaths.Contains(entityConfig.ConfigId)) return;

            // StartCoroutine(LoadPrefabAsync(entityConfig.ConfigId, () =>
            // {
                CreatePool(entityConfig);
                onComplete?.Invoke();
            //}));
        }

        private IEnumerator LoadPrefabAsync(string path, System.Action onLoaded)
        {
            loadingPaths.Add(path);

            ResourceRequest request = Resources.LoadAsync<GameObject>(path);
            yield return request;

            if (request.asset == null)
            {
                Debug.LogError($"[ObjectPool] Не удалось загрузить префаб по пути: {path}");
            }
            else
            {
                GameObject prefab = request.asset as GameObject;
                loadedPrefabs[path] = prefab;
            }

            loadingPaths.Remove(path);
            onLoaded?.Invoke();
        }

        private void CreatePool(EntityConfig entityConfig, int expand = 0)
        {
            // if (!loadedPrefabs.ContainsKey(entityConfig.ConfigId))
            // {
            //     Debug.LogError($"[ObjectPool] Префаб не загружен: {entityConfig.ConfigId}");
            //     return;
            // }

            if (!poolDictionary.ContainsKey(entityConfig.ConfigId))
                poolDictionary[entityConfig.ConfigId] = new Queue<GameObject>();

            if (entityConfig is IObjectPoolParam config)
            {
                var coord = Vector2.down * 1000;
                var count = expand > 0 ? expand : config.ObjectPoolParam.InitialSize; 
                
                // for (int i = 0; i < count; i++)
                // {
                //     GameObject obj = EntityFactory.CreateEntity(entityConfig, coord);
                //     obj.SetActive(false);
                //
                //     var pooled = obj.GetComponent<PooledObject>();
                //     if (pooled == null)
                //         pooled = obj.AddComponent<PooledObject>();
                //
                //     
                //     poolDictionary[entityConfig.ConfigId].Enqueue(obj);
                // }
            }
        }

        /// <summary>
        /// Спавн объекта из пула (если пул есть), или загружает и создаёт
        /// </summary>
        public void Spawn(string configId, Vector3 position, Quaternion rotation, System.Action<GameObject> onSpawned)
        {
            // if (!loadedPrefabs.ContainsKey(prefabPath))
            // {
            //     Preload(prefabPath, () =>
            //     {
            //         GameObject obj = InternalSpawn(prefabPath, position, rotation);
            //         onSpawned?.Invoke(obj);
            //     });
            // }
            // else
            {
                //DLog.Alert($"[Pool_OLD] Spawn: {configId}, Pool_OLD size: {poolDictionary[configId].Count}");
                GameObject obj = InternalSpawn(configId, position, rotation);
                onSpawned?.Invoke(obj);
            }
        }

        private GameObject InternalSpawn(string configId, Vector3 position, Quaternion rotation)
        {
            // if (!poolDictionary.ContainsKey(prefabPath))
            // {
            //     CreatePool(prefabPath, initialPoolSize);
            // }
            //
            if (poolDictionary[configId].Count == 0)
            {
                //CreatePool(FindConfig(configId), 5);
                return null;
            }
            
            GameObject obj = poolDictionary[configId].Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        public void ReturnToPool(GameObject obj, string configId)
        {
            obj.SetActive(false);
            
            if (!poolDictionary.ContainsKey(configId))
            {
                Destroy(obj);
                Debug.LogError($"[ObjectPool] Пул не найден для {configId}, объект уничтожен.");
                return;
            }

            //var n = poolDictionary[configId].Count;
            poolDictionary[configId].Enqueue(obj);
            //DLog.Alert($"[Pool_OLD] Returned: {configId}, Pool_OLD size: {n} => {poolDictionary[configId].Count}", EDlogColor.YELLOW);
        }
    }
}