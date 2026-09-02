#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction.Editor
{
    public class FacilityPrefabPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {

            foreach (var path in importedAssets)
            {
                if (!path.EndsWith(".prefab"))
                    continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                if (prefab.GetComponent<FacilityRenderableTag>() == null)
                    continue;

                var colliders = prefab.GetComponentsInChildren<MeshCollider>();
                var colliders2 = prefab.GetChild(0).GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                    col.enabled = false;
                foreach (var col in colliders2)
                    col.enabled = false;

                EditorUtility.SetDirty(prefab);

                Debug.Log($"Ghost material applied to prefab: {prefab.name}");
            }
        }

    }
}

#endif