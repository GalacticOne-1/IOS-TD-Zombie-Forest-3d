using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Tools
{
    public static class RuntimeIdDatabaseScanner
    {
        public static List<RuntimeIdEntry> Scan(out Dictionary<string, List<RuntimeIdEntry>> grouped)
        {
            grouped = new Dictionary<string, List<RuntimeIdEntry>>();

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (obj is not RuntimeId runtime)
                    continue;

                var entry = new RuntimeIdEntry
                {
                    RuntimeId = runtime,
                    Guid = runtime.Guid,
                    Path = path,
                    IsDuplicate = false
                };

                if (!grouped.TryGetValue(entry.Guid, out var list))
                {
                    list = new List<RuntimeIdEntry>();
                    grouped[entry.Guid] = list;
                }

                list.Add(entry);
            }

            // mark duplicates
            foreach (var kv in grouped)
            {
                if (kv.Value.Count > 1)
                {
                    foreach (var e in kv.Value)
                        e.IsDuplicate = true;
                }
            }

            return grouped.Values.SelectMany(x => x).ToList();
        }
    }

    public class RuntimeIdEntry
    {
        public RuntimeId RuntimeId;
        public string Guid;
        public string Path;
        public bool IsDuplicate;
    }
}