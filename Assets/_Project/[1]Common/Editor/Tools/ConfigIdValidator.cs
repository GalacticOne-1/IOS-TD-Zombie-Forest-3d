
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Galactic1.Configs;


namespace Galactic1.Tools
{
    public static class ConfigIdValidator
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            // Автоматическая проверка при изменениях в проекте
            EditorApplication.projectChanged += AutoFixIds;
        }

        /// <summary>
        /// Меню для ручного запуска проверки
        /// </summary>
        [MenuItem("Tools/Configs/Validate Config IDs")]
        public static void RunValidationNow()
        {
            AutoFixIds();
            Debug.Log("[ConfigIdValidator] Manual validation finished.");
        }

        /// <summary>
        /// Основной метод проверки и нормализации id
        /// </summary>
        private static void AutoFixIds()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            HashSet<string> ids = new HashSet<string>();
            bool changed = false;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so == null) continue;

                var type = so.GetType();
                var field = type.GetProperty("Id");
                if (field == null || field.PropertyType != typeof(string)) continue;

                string currentId = field.GetValue(so) as string;

                // Если пусто → генерируем из имени ассета
                if (string.IsNullOrEmpty(currentId))
                {
                    currentId = NormalizeId(so.name);
                    field.SetValue(so, currentId);
                    EditorUtility.SetDirty(so);
                    changed = true;
                    Debug.Log($"[ConfigIdValidator] Assigned id '{currentId}' to {path}");
                }
                else
                {
                    string normalized = NormalizeId(currentId);
                    if (normalized != currentId)
                    {
                        field.SetValue(so, normalized);
                        EditorUtility.SetDirty(so);
                        changed = true;
                        Debug.Log($"[ConfigIdValidator] Normalized id '{currentId}' → '{normalized}' in {path}");
                    }
                }

                // Проверка на дубликаты
                if (!ids.Add(currentId))
                {
                    Debug.LogWarning($"[ConfigIdValidator] Duplicate id '{currentId}' found in {path}");
                }
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
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
            result = Regex.Replace(result, @"[\s\-]+", "_");

            // 3. Убираем все символы кроме a-z, 0-9 и _
            result = Regex.Replace(result, @"[^\w_]", "");

            // 4. Приводим к нижнему регистру
            result = result.ToLowerInvariant();

            return result;
        }
    }


}