using UnityEngine;

namespace Galactic1.Gameplay.Locations.Authoring
{
    /// <summary>
    /// Отрисовывает Gizmos.DrawCube для дочерних объектов заданного root'а.
    /// Размер куба берётся из Renderer.bounds / Collider.bounds
    /// (либо lossyScale, если ни компонента нет).
    ///
    /// Никакой логики генерации — чисто визуализация в редакторе,
    /// как и OnDrawGizmosSelected в LocationGeometryDefinition.
    /// </summary>
    public sealed class POIBoundsGizmoDrawer : MonoBehaviour
    {
        // ===================================================================
        // SOURCE
        // ===================================================================
        [Header("=== SOURCE ===")]
        [Tooltip("Родитель, чьи дочерние объекты нужно отрисовать. " +
                 "Если не задан — используется собственный transform.")]
        [SerializeField]
        private Transform childrenRoot;


        // ===================================================================
        // APPEARANCE
        // ===================================================================
        [Header("=== APPEARANCE ===")]
        [SerializeField]
        private Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.35f);

        [Tooltip("Дополнительно рисовать wire-контур поверх заливки.")]
        [SerializeField]
        private bool drawWireOutline = true;


        // ===================================================================
        // GIZMOS
        // ===================================================================
// #if UNITY_EDITOR
//         private void OnDrawGizmosSelected()
//         {
//             var root = childrenRoot != null ? childrenRoot : transform;
//             if (root.childCount == 0)
//                 return;
//
//             DrawChildren(root);
//         }
//
//         private void DrawChildren(Transform root)
//         {
//             foreach (Transform child in root)
//             {
//                 DrawCubeForChild(child);
//             }
//         }
//
//         private void DrawCubeForChild(Transform child)
//         {
//             Vector3 center = child.position;
//             Vector3 size = child.GetComponent<GameplayZone>().Size;
//             size.z = size.y;
//             size.y = 1;
//
//             Gizmos.matrix = Matrix4x4.identity;
//
//             Gizmos.color = gizmoColor;
//             Gizmos.DrawCube(center, size);
//
//             if (drawWireOutline)
//             {
//                 Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
//                 Gizmos.DrawWireCube(center, size);
//             }
//         }
// #endif
    }
}