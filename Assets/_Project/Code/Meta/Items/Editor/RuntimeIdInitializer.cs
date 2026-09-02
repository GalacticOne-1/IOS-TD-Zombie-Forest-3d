
using Galactic1.Code.GameDatabase.Registries;
using UnityEditor;
using UnityEngine;

namespace Galactic1.Tools
{
    public static class RuntimeIdInitializer
    {
        [MenuItem("Tools/Database/Initialize RuntimeIds")]
        public static void InitializeAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (obj is RuntimeId id)
                {
                    id.Editor_InitializeIfNeeded();
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("RuntimeIds initialized");
        }
    }
}