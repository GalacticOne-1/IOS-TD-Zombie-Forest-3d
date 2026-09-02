using System.Collections.Generic;
using Galactic1.Structs.UI;
using UnityEngine;


namespace Galactic1.UI.Core
{
    [CreateAssetMenu(fileName = "UIStyleDatabase", menuName = "Game Configs/Style/UI Style Database")]
    public class UIStyleDatabase : ScriptableObject
    {
        // вынести в отдельный конфиг, как с IntelStyleConfig !!!!
        [field: SerializeField] public InteractionIcons InteractionIcons { get; private set; }
        [field: SerializeField] public InventoryIcons InventoryIcons { get; private set; }
        [field: SerializeField] public CDragonUI DragonUI { get; private set; }

        
        // стили лежат как отдельные конфиги
        private Dictionary<string, IUIStyleConfig> _configs = new();

        
        

        public void Initialize(Dictionary<string, ScriptableObject> rawConfigs)
        {
            if(Application.isPlaying)
            {
                foreach (var config in rawConfigs.Values)
                {
                    if (config is IUIStyleConfig style)
                    {
                        if (!_configs.ContainsKey(style.ConfigId))
                            _configs.Add(style.ConfigId, style);
                        else
                            Debug.LogError($"Duplicate UIStyleConfig id: {style.ConfigId}");
                    }
                }
            }
        }


        /// <summary>
        /// Получение конкретного конфига по id
        /// </summary>
        /// <param name="configId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string configId) where T : ScriptableObject
        {
            if (_configs.TryGetValue(configId, out var config) && config is T typed)
                return typed;

            DLog.Alert($"UI Style '{configId}' not found or wrong type. Expected {typeof(T).Name}", EDlogColor.RED);
            return null;
        }
    }
}