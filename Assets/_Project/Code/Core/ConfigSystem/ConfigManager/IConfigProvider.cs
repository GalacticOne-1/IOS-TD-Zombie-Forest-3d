using Galactic1.Code.Gameplay.BaseBuilding;
using UnityEngine;

namespace Galactic1.Configs
{
    public interface IConfigProvider
    {
        ConfigProvider Provider { get; }

        ScriptableObject Get(string key);
        T Get<T>(string key) where T : ScriptableObject;

        /// <summary>
        /// Универсальный геттер<br/>
        /// Название и скрипт должны быть одинаковы !!!<br/>
        /// (PlayerConfig.asset == PlayerConfig.cs)
        /// </summary>
        T Get<T>() where T : ScriptableObject;

        IAPConfigRegistry IAP { get; }
    }
}