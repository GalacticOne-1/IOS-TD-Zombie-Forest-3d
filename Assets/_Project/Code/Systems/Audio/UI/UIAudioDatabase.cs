using System.Collections.Generic;
using Galactic1.Code.Gameplay.Audio;
using UnityEngine;


namespace Galactic1.UI.Core
{
    /*
     *  Общая база для стандартных звуков
     */
    
    [CreateAssetMenu(fileName = "UIAudioDatabase", menuName = "Game Configs/Audio/UI Audio Database")]
    public class UIAudioDatabase : ScriptableObject
    {
        // стили лежат как отдельные конфиги
        private Dictionary<string, SimpleAudioConfig> _map = new();

        
        

        public void Initialize(Dictionary<string, ScriptableObject> rawConfigs)
        {
            if(Application.isPlaying)
            {
                foreach (var config in rawConfigs.Values)
                {
                    if (config is SimpleAudioConfig style)
                    {
                        if (!_map.ContainsKey(style.ConfigId))
                            _map.Add(style.ConfigId, style);
                        else
                            Debug.LogError($"Duplicate SimpleAudioConfig id: {style.ConfigId}");
                    }
                }
            }
        }


        /// <summary>
        /// Получение конкретного конфига по id
        /// </summary>
        public T Get<T>(string configId) where T : ScriptableObject
        {
            if (_map.TryGetValue(configId, out var config) && config is T typed)
                return typed;

            DLog.Alert($"UI Audio '{configId}' not found or wrong type. Expected {typeof(T).Name}", EDlogColor.RED);
            return null;
        }
    }
}