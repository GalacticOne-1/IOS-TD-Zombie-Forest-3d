using UnityEngine;
using UnityEditor;

namespace Galactic1
{
    public static class PrefabEditorUtils
    {
        /// <summary>
        /// Deletes the prefab reference (unlink the prefab) from the GameObject in the editor.
        /// This operation only works in Editor mode.
        /// </summary>
        /// <param name="gameObject">The GameObject that is linked to a prefab.</param>
        public static void DeletePrefabInstanceInEditor(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Debug.LogError("GameObject is null.");
                return;
            }

            // Check if the GameObject is a prefab instance
            if (PrefabUtility.GetPrefabAssetType(gameObject) == PrefabAssetType.NotAPrefab)
            {
                Debug.LogWarning("The provided GameObject is not a prefab instance.");
                return;
            }

            Undo.RegisterCompleteObjectUndo(gameObject, "Unlink Prefab");
            Object prefabInstance = PrefabUtility.GetPrefabInstanceHandle(gameObject);
            //GameObject.DestroyImmediate(prefabInstance); // Destroys the prefab instance handle
            GameObject.DestroyImmediate(gameObject); // Destroys the GameObject itself
        }
    }
}