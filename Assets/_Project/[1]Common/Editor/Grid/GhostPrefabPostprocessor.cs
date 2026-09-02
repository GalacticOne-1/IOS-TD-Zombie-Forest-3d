#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction.Editor
{
    /// <summary>
    /// Prefab processing pipeline для ghost rendering.
    ///
    /// Срабатывает при:
    /// - import prefab
    /// - reimport prefab
    /// - save prefab
    ///
    /// Работает только если prefab содержит GhostRenderableTag.
    /// </summary>
    public class GhostPrefabPostprocessor : AssetPostprocessor
    {
        private const string GhostMaterialPath =
            "Assets/_Project/Art/[1]Common/Shader IMPORTANT/Grid/Construction_SC2Ghost.mat";

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var ghostMaterial = AssetDatabase.LoadAssetAtPath<Material>(GhostMaterialPath);

            if (ghostMaterial == null)
            {
                Debug.LogError("Ghost material not found: " + GhostMaterialPath);
                return;
            }

            foreach (var path in importedAssets)
            {
                if (!path.EndsWith(".prefab"))
                    continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                if (prefab.GetComponent<GhostRenderableTag>() == null)
                    continue;

                // #1 materials
                ApplyGhostMaterial(prefab, ghostMaterial);
                
                // #2 disable inner go
                var l = prefab.transform.childCount;
                for (int i = 1; i < l; i++)
                    prefab.GetChild(i).SetActive(false);

                Debug.Log($"Ghost material applied to prefab: {prefab.name}");
            }
        }

        private static void ApplyGhostMaterial(GameObject prefab, Material ghostMaterial)
        {
            var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            var colliders = prefab.GetComponentsInChildren<MeshCollider>();
            var colliders2 = prefab.GetChild(0).GetComponentsInChildren<Collider>();

            foreach (var renderer in renderers)
            {
                var mats = renderer.sharedMaterials;

                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = ghostMaterial;
                }

                renderer.sharedMaterials = mats;
                

                EditorUtility.SetDirty(renderer);
            }

            foreach (var col in colliders)
                col.enabled = false;
            
            foreach (var col in colliders2)
                col.enabled = false;

            EditorUtility.SetDirty(prefab);
        }
    }
}

#endif