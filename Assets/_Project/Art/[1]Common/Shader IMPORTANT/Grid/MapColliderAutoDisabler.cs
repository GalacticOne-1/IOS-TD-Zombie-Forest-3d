
using UnityEngine;

#if UNITY_EDITOR
namespace Galactic1.Code.Gameplay.Construction.Editor
{
    [ExecuteAlways]
    public class MapColliderAutoDisabler : MonoBehaviour
    {
        private void OnValidate()
        {
            var colliders = GetComponentsInChildren<MeshCollider>();
            foreach (var col in colliders)
            {
                if (!col.CompareTag("SelectionCollider"))
                    col.enabled = false;
            }

            DLog.Alert($"Map location object updated: {name}");
        }
    }
}
#endif