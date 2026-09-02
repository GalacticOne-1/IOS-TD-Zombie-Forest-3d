using System.Collections.Generic;
using Galactic1.Configs;
using Galactic1.AbstractFactory;
using UnityEngine;


namespace Galactic1.Dev
{
    public class DevSpawner : MonoBehaviour
    {
        [Tooltip("Список всех префабов, которые можно спавнить")]
        public List<string> entityConfigs = new();

        public void Spawn(string configId, Vector3 position)
        {
            var id = entityConfigs.Find(id => id == configId);
            if (id != null)
            {
                // var config = ServiceLocator.Current.Get<ConfigManager>().Enemies.Get(id);
                // var entity = EntityFactory.CreateEntity(config, position).GetComponent<_Entity>();
                // entity.Entity_Activate();
            }
            else
                Debug.LogWarning($"Entity config '{configId}' not found!");
        }
    }

}