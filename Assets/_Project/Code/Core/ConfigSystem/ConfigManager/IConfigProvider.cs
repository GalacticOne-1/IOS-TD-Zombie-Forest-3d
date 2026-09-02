using System.Collections;
using Galactic1.Code.Gameplay.BaseBuilding;
using UnityEngine;

namespace Galactic1.Configs
{
    public interface IConfigProvider
    {
        ConfigProvider Provider { get; }
        IEnumerator LoadAllConfigs();

        ScriptableObject Get(string key);
        T Get<T>(string key) where T : ScriptableObject;
        
        /// <summary>
        /// Универсальный геттер<br/>
        /// Название и скрипт должны быть одинаковы !!!<br/>
        /// (PlayerConfig.asset == PlayerConfig.cs)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>() where T : ScriptableObject;
        
        
        
        IAPConfigRegistry IAP { get; }
        StructureConfigRegistry Structures { get; }
        LocationsStateConfigRegistry LocationsState { get; }
    }
}