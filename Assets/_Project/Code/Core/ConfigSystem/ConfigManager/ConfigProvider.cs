using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Gameplay.Combat.Visual;
using Galactic1.Items;
using Galactic1.UI.Core;
using UnityEditor;
using UnityEngine;


namespace Galactic1.Configs
{
    public class ConfigProvider : IGameService, IConfigProvider
    {
        public ConfigProvider Provider => this;

        //private List<ScriptableObject> _allConfigs = new();
        private Dictionary<string, ScriptableObject> allConfigs = new();

        
        
        
        // доступ к конфигам
        public IAPConfigRegistry IAP { get; private set; }
        
        
        public CombatSurfaceFXDatabase CombatSurfaceFX { get; private set; }
        public CombatTracerDatabase CombatTracers { get; private set; }
        
        // ====== ^^ NEW ^^ ======
        
        
        public LocationsStateConfigRegistry LocationsState { get; private set; }                // ??     
        public StructureConfigRegistry Structures { get; private set; }                         // old
        // OLD




        public ConfigProvider()
        {
            LoadFromManifest();

            IAP = new IAPConfigRegistry(allConfigs);
            Get<UIStyleDatabase>().Initialize(allConfigs);
            CombatSurfaceFX = new CombatSurfaceFXDatabase(allConfigs);
            CombatTracers = new CombatTracerDatabase(Get<ItemDatabase>().Items);
            
            // ======================================================================================================
            
            LocationsState = new LocationsStateConfigRegistry(allConfigs);
            Structures = new StructureConfigRegistry(allConfigs);
            


            //ServiceLocator.Current.Get<CoroutineController>()
            //.StartCoroutine(ConfigLoader.LoadAllConfigs(localJSON, OnConfigsLoaded));
        }


        // загружает конфиги по списку путей
        public void LoadFromManifest()
        {
            // манифест для локальных конфигов Tools->Configs->Generate Manifest
            TextAsset manifestAsset = Resources.Load<TextAsset>("Configs/configs_manifest");
            if (manifestAsset == null)
            {
                Debug.LogError("❌ No configs_manifest.json found!");
                return;
            }

            var manifest = JsonUtility.FromJson<ConfigsManifest>(manifestAsset.text);

            foreach (var entry in manifest.files)
            {
                var config = Resources.Load<ScriptableObject>(entry.path);
                if (config != null)
                {
                    DLog.Alert($"✅ Loaded config: {entry.key} from {entry.path}", EDlogColor.YELLOW);
                    string key = GetKeyForConfig(config);   // напр. "DailyRewardConfig"
                    if (string.IsNullOrEmpty(key))
                    {
                        Debug.LogError($"Config '{config.name}' produced an empty key — skipped.");
                        continue;
                    }
                    
                    key = NormalizeId(key);

                    if (!allConfigs.ContainsKey(key))
                    {
                        allConfigs.Add(key, config);
                        //_allConfigs.Add(config);
                    }
                    else
                    {
                        // Логируем, откуда этот ассет (в редакторе можно показать путь)
#if UNITY_EDITOR
                        string existingPath = GetAssetPath(allConfigs[key]);
                        string newPath = GetAssetPath(config);
                        Debug.LogError($"Duplicate config key '{key}' — existing: {existingPath}, new: {newPath}. Skipping new one.");
#else
                Debug.LogWarning($"Duplicate config key '{key}' for asset '{config.name}'. Skipping duplicate.");
#endif
                    }
                }
                else
                    Debug.LogError($"⚠ Failed to load config: {entry.key} ({entry.path})");
            }
            
            DLog.Alert($"Loaded {allConfigs.Count} unique configs.");
        }

        private void LoadBuiltInConfigs()
        {
            var configs = Resources.LoadAll<ScriptableObject>("Configs/Gameplay");

            foreach (var cfg in configs)
            {
                string key = cfg.GetType().Name; // напр. "DailyRewardConfig"
                if (!allConfigs.ContainsKey(key))
                    allConfigs.Add(key, cfg);
            }

            Debug.Log($"✅ Found {allConfigs.Count} built-in configs in Resources.");
        }



        /// <summary>
        /// For loading all configs for game
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadAllConfigs() => ConfigLoader.LoadAllConfigs(OnConfigsLoaded);

        private void OnConfigsLoaded(Dictionary<string, string> jsonConfigs)
        {
            foreach (var jsonConfig in jsonConfigs)
            {
                string key = jsonConfig.Key; // напр. "DailyRewardConfig" из манифеста
                string json = jsonConfig.Value;


                // устанавливать из манифеста, какие json для массива конфигов
                // ! что бы json загрузался он должен быть в манифесте! 
                switch (key)
                {
                    case "builds_config":
                        //UpdateConfigsFromJson<BuildConfig, BuildConfig.Wrapper>(json, Builds._configs);
                        break;
                    
                    case "equipments_config":
                        //UpdateConfigsFromJson<CombatEquipmentConfig, CombatEquipmentConfig.Wrapper>(json, Equipments._configs);
                        break;

                    case "weapons_config":
                        //UpdateConfigsFromJson<WeaponConfig, WeaponConfig.Wrapper>(json, Weapon._configs);
                        break;

                    default:
                    {
                        // регистронезависимый поиск
                        var so = allConfigs
                            .FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase))
                            .Value;

                        if (so != null)
                        {
                            var method = so.GetType().GetMethod("UpdateFromJson");
                            if (method != null)
                            {
                                method.Invoke(so, new object[] { json });
                                Debug.Log($"🔄 Updated {key} from JSON");
                            }
                            else
                            {
                                DLog.Alert($"⚠ {key} has no UpdateFromJson method.", EDlogColor.RED);
                            }
                        }
                        else
                        {
                            DLog.Alert($"⚠ Config {key} not found in Resources.", EDlogColor.RED);
                        }

                    }
                        break;
                }
            }
        
        }

        public void UpdateConfigsFromJson<T, TData>(string json, Dictionary<string, T> configs) 
            where T : ScriptableObject
            where TData : class
        {
            if (configs == null || configs.Count == 0)
            {
                DLog.Alert($"⚠ No configs provided for type {typeof(T).Name}", EDlogColor.RED);
                return;
            }

            if (string.IsNullOrEmpty(json))
            {
                DLog.Alert($"⚠ Empty JSON for type {typeof(T).Name}", EDlogColor.RED);
                return;
            }

            try
            {
                // 1️⃣ Оборачиваем массив в объект, чтобы JsonUtility мог его распарсить
                var wrappedJson = $"{{\"items\":{json}}}";

                var wrapper = JsonUtility.FromJson<Wrapper<TData>>(wrappedJson);
                if (wrapper?.items == null || wrapper.items.Length == 0)
                {
                    DLog.Alert($"⚠ No items found in JSON for {typeof(T).Name}", EDlogColor.RED);
                    return;
                }

                // 3️⃣ Обновляем каждый конфиг
                foreach (var item in wrapper.items)
                {
                    if (item == null) continue;
                    
                    // Получаем id через рефлексию
                    var idField = typeof(TData).GetField("id");
                    var idValue = idField?.GetValue(item)?.ToString();

                    if (string.IsNullOrEmpty(idValue)) continue;

                    // Находим нужный ScriptableObject по id
                    var so = configs.FirstOrDefault(c => c.Key == idValue).Value;
                    if (so == null) continue;

                    var method = so.GetType().GetMethod("UpdateFromJson");
                    if (method != null)
                    {
                        // Выбираем правильный TData для этого типа
                        var dataType = ConfigDataTypeResolver.Resolve(so.GetType());
                        if (dataType == null)
                        {
                            Debug.LogError($"❌ No TData type found for {so.GetType().Name}");
                            continue;
                        }

                        // Создаём generic-метод и вызываем его
                        var genericMethod = method.MakeGenericMethod(dataType);
                        genericMethod.Invoke(so, new object[] { JsonUtility.ToJson(item) });

                        DLog.Alert($"🔄 Updated {idValue} using {dataType.Name}");
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Failed to update {typeof(T).Name} from json: {ex.Message}");
            }
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }





        // Попытка получить ключ в порядке приоритета:
        private string GetKeyForConfig(ScriptableObject cfg)
        {
            // 2) fallback — имя ассета (cfg.name)
            return cfg.name;
        }

        /// <summary>
        /// Приводим id к snake_case:
        /// - нижний регистр
        /// - пробелы/дефисы → _
        /// - убираем спецсимволы
        /// </summary>
        private static string NormalizeId(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            string result = input.Trim();

            // 1. Преобразуем PascalCase / camelCase в snake_case
            // Добавляем _ перед заглавной буквой, если перед ней буква
            result = System.Text.RegularExpressions.Regex.Replace(
                result,
                @"(?<=[a-z0-9])([A-Z])",
                "_$1"
            );

            // 2. Пробелы и дефисы → _
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[\s\-]+", "_");

            // 3. Убираем все символы кроме a-z, 0-9 и _
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[^\w_]", "");

            // 4. Приводим к нижнему регистру
            result = result.ToLowerInvariant();

            return result;
        }
        
        
#if UNITY_EDITOR
        private string GetAssetPath(UnityEngine.Object obj)
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
        
        /// Пример геттеров по типу
        /// <br/>(только для уникальных)
        public T Get<T>(string key) where T : ScriptableObject
        {
            var so = Get(key);
            return so as T;
        }

        public T Get<T>() where T : ScriptableObject
        {
            string key = typeof(T).Name;
            key = NormalizeId(key);
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
        public string key; // имя конфига (например DailyRewardConfig)
        public string path; // путь для Resources.Load (например "Configs/Gameplay/DailyRewardConfig")
    }

}