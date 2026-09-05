
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace Galactic1.Configs
{
    public static class ConfigManifestGenerator
    {
        private const string ResourcesRoot = "Assets/Resources/Configs";
        private const string ManifestPath = "Assets/Resources/Configs/configs_manifest.json";

        // 🔥 список исключений (можно расширять)
        private static readonly string[] IgnorePatterns =
        {
            "Test", // все файлы, где есть "Test"
            "Old", // старые версии
            "Backup", // бэкапы
            "Example", // примерные конфиги
            "configs_manifest.json", // сам манифест
            "Fight"
            //"_ApplicationConfigs",
            //"_GameConfigs"
        };
        
        static string[] ignoreFolders = new[]
        {
            "Assets/Resources/Configs/_Ignore_",
            "Assets/Resources/Configs/Gameplay/Locations/Location",
            "Assets/Resources/Configs/Gameplay/Locations/Loot",
            "Assets/Resources/Configs/Gameplay/Locations/PlayerSpawnPresets",
            "Assets/Resources/Configs/Gameplay/Inventory/Crafting",
            "Assets/Resources/Configs/Gameplay/Inventory/Items",
            "Assets/Resources/Configs/Gameplay/Recruitment/Units",
            "Assets/Resources/Configs/Gameplay/Enemies/Variants",
            "Assets/Resources/Configs/Tutorial/Campaigns",
        };

        
        
        
        
        [MenuItem("Tools/Configs/Generate Manifest")]
        public static void Generate()
        {
            if (!Directory.Exists(ResourcesRoot))
            {
                Debug.LogWarning($"❌ Folder not found: {ResourcesRoot}");
                return;
            }

            List<ConfigEntry> entries = new List<ConfigEntry>();

            // ищем все ScriptableObject внутри папки Configs
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { ResourcesRoot });
            
            // для игнорирования папок
            var results = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => ignoreFolders.All(ignore => !IsInFolder(path, ignore)))
                .Where(path => !IsInsideIdsFolder(path)) // 👈 исключаем всё, что находится внутри папок _Ids
                .ToArray();
            
            
            foreach (string guid in results)
            {
                string assetPath = guid;//AssetDatabase.GUIDToAssetPath(guid); // нужно если юзать guids вместо results
                string fileName = Path.GetFileName(assetPath);

                // ⚡ проверка на игнор
                if (ShouldIgnore(fileName))
                {
                    Debug.Log($"⏭ Ignored: {fileName}");
                    continue;
                }

                // путь внутри Resources (без расширения и без Assets/Resources/)
                string resourcesPath = Path.ChangeExtension(assetPath, null)
                    .Replace("Assets/Resources/", "");

                entries.Add(new ConfigEntry
                {
                    key = Path.GetFileNameWithoutExtension(assetPath),
                    path = resourcesPath
                });
            }

            ConfigsManifest manifest = new ConfigsManifest { files = entries.ToArray() };

            string json = JsonUtility.ToJson(manifest, true);
            File.WriteAllText(ManifestPath, json);

            AssetDatabase.Refresh();

            Debug.Log($"✅ Config manifest generated at {ManifestPath}, {entries.Count} configs found");
        }

        private static bool ShouldIgnore(string fileName)
        {
            return IgnorePatterns.Any(pattern => fileName.Contains(pattern));
        }
        
        private static bool IsInFolder(string assetPath, string folderPath)
        {
            // нормализуем — убираем trailing slash если есть
            folderPath = folderPath.TrimEnd('/');
    
            return assetPath.StartsWith(folderPath + "/");
        }
        
        // 👈 Метод для проверки, находится ли файл внутри папки _Ids на любом уровне вложенности
        private static bool IsInsideIdsFolder(string assetPath)
        {
            // Нормализуем слеши на случай разных ОС
            string normalizedPath = assetPath.Replace('\\', '/');
            
            // Разбиваем путь на папки
            string[] folders = normalizedPath.Split('/');
            
            // Проверяем, есть ли среди родительских папок папка с именем "_Ids"
            // (исключаем последний элемент, так как это имя самого файла)
            for (int i = 0; i < folders.Length - 1; i++)
            {
                if (folders[i] == "_Ids")
                {
                    return int.TryParse(folders[i], out _) == false; // просто возвращаем true, если имя совпало
                }
            }
            
            return false;
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

}