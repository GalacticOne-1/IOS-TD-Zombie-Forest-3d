using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Marker component.
    ///
    /// Помечает prefab как поддерживающий
    /// ghost rendering в системе строительства.
    ///
    /// Используется Editor pipeline:
    /// GhostPrefabPostprocessor.
    ///
    /// Не содержит логики.
    /// </summary>
    public class GhostRenderableTag : MonoBehaviour
    {
    }
}