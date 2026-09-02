using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    /// <summary>
    /// Управляет shader параметрами ghost объекта.
    /// Используется ConstructionPlacementController.
    /// </summary>
    public class GhostShaderController : MonoBehaviour
    {
        private static readonly int ValidID = Shader.PropertyToID("_Valid");

        private MaterialPropertyBlock _mpb;
        private Renderer[] _renderers;

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _mpb = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Устанавливает состояние ghost (valid / invalid).
        /// </summary>
        public void SetValid(bool valid)
        {
            float v = valid ? 1f : 0f;

            foreach (var r in _renderers)
            {
                r.GetPropertyBlock(_mpb);
                _mpb.SetFloat(ValidID, v);
                r.SetPropertyBlock(_mpb);
            }
        }
    }
}