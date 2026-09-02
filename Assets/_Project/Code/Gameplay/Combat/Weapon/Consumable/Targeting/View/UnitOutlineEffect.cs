using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    public sealed class UnitOutlineEffect
    {
        private readonly Renderer[] _renderers;
        private static readonly int OutlineEnabled = Shader.PropertyToID("_OutlineEnabled");

        public UnitOutlineEffect(GameObject unitGO)
        {
            _renderers = unitGO.GetComponentsInChildren<Renderer>();
        }

        // подсветка юнита пока не реализована !!!
        public void Show()
        {
            // foreach (var r in _renderers)
            //     r.material.SetFloat(OutlineEnabled, 1f);
        }

        public void Hide()
        {
            // foreach (var r in _renderers)
            //     r.material.SetFloat(OutlineEnabled, 0f);
        }
    }
}