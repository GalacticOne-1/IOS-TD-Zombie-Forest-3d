using System.Collections.Generic;
using Galactic1.Code.Gameplay.Combat.Visual;
using Galactic1.Items;
using Galactic1.UI.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galactic1.Configs
{
    public class ConfigProvider : IGameService, IConfigProvider
    {
        // локальный манифест со списком ScriptableObject (Resources/Configs/configs_manifest.json)
        // генерируется через Tools->Configs->Generate Manifest
        private const string ManifestResourcePath = "Configs/configs_manifest";

        public ConfigProvider Provider => this;

        private readonly Dictionary<string, ScriptableObject> allConfigs = new();

        // доступ к конфигам
        public IAPConfigRegistry IAP { get; private set; }
        public CombatSurfaceFXDatabase CombatSurfaceFX { get; private set; }
        public CombatTracerDatabase CombatTracers { get; private set; }

        public ConfigProvider()
        {
            // 1. сначала регистрируем все ассеты из манифеста
            LoadFromManifest();

            // 2. потом инициализируем реестры и базы, которые читают allConfigs
            IAP = new IAPConfigRegistry(allConfigs);
            Get<UIStyleDatabase>().Initialize(allConfigs);
            Get<UIAudioDatabase>().Initialize(allConfigs);
            CombatSurfaceFX = new CombatSurfaceFXDatabase(allConfigs);
            CombatTracers = new CombatTracerDatabase(Get<ItemDatabase>().Items);
        }

        /// <summary>
        /// Загружает ScriptableObject-конфиги по путям из Resources/Configs/configs_manifest
        /// </summary>
        private void LoadFromManifest()
        {
            TextAsset manifestAsset = Resources.Load<TextAsset>(ManifestResourcePath);
            if (manifestAsset == null)
            {
                Debug.LogError($"❌ No {ManifestResourcePath} found in Resources!");
                return;
            }

            var manifest = JsonUtility.FromJson<ConfigsManifest>(manifestAsset.text);
            if (manifest?.files == null)
            {
                Debug.LogError($"❌ {ManifestResourcePath} is empty or invalid!");
                return;
            }

            foreach (var entry in manifest.files)
            {
                var config = Resources.Load<ScriptableObject>(entry.path);
                if (config == null)
                {
                    Debug.LogError($"⚠ Failed to load config: {entry.key} ({entry.path})");
                    continue;
                }

                string key = NormalizeId(GetKeyForConfig(config));
                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError($"Config '{config.name}' produced an empty key — skipped.");
                    continue;
                }

                if (allConfigs.TryAdd(key, config))
                    continue;

#if UNITY_EDITOR
                Debug.LogError($"Duplicate config key '{key}' — existing: {GetAssetPath(allConfigs[key])}, " +
                               $"new: {GetAssetPath(config)}. Skipping new one.");
#else
                Debug.LogWarning($"Duplicate config key '{key}' for asset '{config.name}'. Skipping duplicate.");
#endif
            }

            DLog.Alert($"Loaded {allConfigs.Count} unique configs.");
        }

        // Ключ конфига — имя ассета
        private string GetKeyForConfig(ScriptableObject cfg) => cfg.name;

        /// <summary>
        /// Приводим id к snake_case:
        /// - PascalCase/camelCase → snake_case
        /// - пробелы/дефисы → _
        /// - убираем спецсимволы
        /// - нижний регистр
        /// </summary>
        private static string NormalizeId(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            string result = input.Trim();

            result = System.Text.RegularExpressions.Regex.Replace(result, @"(?<=[a-z0-9])([A-Z])", "_$1");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[\s\-]+", "_");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[^\w_]", "");

            return result.ToLowerInvariant();
        }

#if UNITY_EDITOR
        private string GetAssetPath(Object obj)
        {
            if (obj == null) return "(null)";
            return AssetDatabase.GetAssetPath(obj);
        }
#endif

        #region GET CONFIG

        /// Универсальный геттер по нормализованному ключу
        public ScriptableObject Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            key = NormalizeId(key);
            allConfigs.TryGetValue(key, out var so);
            return so;
        }

        /// (только для уникальных)
        public T Get<T>(string key) where T : ScriptableObject
        {
            return Get(key) as T;
        }

        public T Get<T>() where T : ScriptableObject
        {
            string key = NormalizeId(typeof(T).Name);
            if (allConfigs.TryGetValue(key, out var cfg))
                return cfg as T;

            Debug.LogError($"❌ Config {key} not found!");
            return null;
        }

        #endregion
    }

    [System.Serializable]
    public class ConfigsManifest
    {
        public ConfigEntry[] files;
    }

    [System.Serializable]
    public class ConfigEntry
    {
        public string key;  // имя конфига (например DailyRewardConfig)
        public string path; // путь для Resources.Load (например "Configs/Gameplay/DailyRewardConfig")
    }
}