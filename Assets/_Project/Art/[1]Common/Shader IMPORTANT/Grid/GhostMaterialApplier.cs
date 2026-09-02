#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Editor utility.
    ///
    /// Назначает ghost материал всем MeshRenderer внутри prefab.
    /// Используется для подготовки building prefab
    /// к режиму ghost placement.
    ///
    /// Работает только в редакторе.
    /// </summary>
    [ExecuteInEditMode]
    public class GhostMaterialApplier : MonoBehaviour
    {
        [Header("Ghost Material")]
        [SerializeField] private Material ghostMaterial;

        /// <summary>
        /// Назначить материал всем MeshRenderer
        /// Правая кнопка по компоненту и в меню будет Apply Ghost Material
        /// </summary>
        [ContextMenu("Apply Ghost Material")]
        public void ApplyGhostMaterial()
        {
            if (ghostMaterial == null)
            {
                Debug.LogError("Ghost material not assigned");
                return;
            }

            var renderers = GetComponentsInChildren<MeshRenderer>(true);

            foreach (var r in renderers)
            {
                Undo.RecordObject(r, "Apply Ghost Material");

                var materials = new Material[r.sharedMaterials.Length];

                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = ghostMaterial;
                }

                r.sharedMaterials = materials;

                EditorUtility.SetDirty(r);
            }

            Debug.Log($"Ghost material applied to {renderers.Length} renderers.");
        }

        /// <summary>
        /// Восстановить оригинальные материалы
        /// (если нужно вернуть prefab).
        /// </summary>
        [ContextMenu("Clear Ghost Material")]
        public void ClearGhostMaterial()
        {
            var renderers = GetComponentsInChildren<MeshRenderer>(true);

            foreach (var r in renderers)
            {
                Undo.RecordObject(r, "Clear Ghost Material");

                var materials = new Material[r.sharedMaterials.Length];

                r.sharedMaterials = materials;

                EditorUtility.SetDirty(r);
            }

            Debug.Log("Ghost materials cleared.");
        }
    }
}

#endif