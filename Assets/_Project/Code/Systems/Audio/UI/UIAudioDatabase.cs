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
        private Dictionary<string, IUIAudioConfig> _map = new();

        
        

        public void Initialize(Dictionary<string, ScriptableObject> rawConfigs)
        {
            if(Application.isPlaying)
            {
                foreach (var config in rawConfigs.Values)
                {
                    if (config is IUIAudioConfig audioConfig)
                    {
                        if (!_map.ContainsKey(audioConfig.ConfigId))
                            _map.Add(audioConfig.ConfigId, audioConfig);
                        else
                            Debug.LogError($"Duplicate SimpleAudioConfig id: {audioConfig.ConfigId}");
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