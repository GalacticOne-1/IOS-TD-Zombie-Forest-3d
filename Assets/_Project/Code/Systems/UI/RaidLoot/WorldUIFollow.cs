using UnityEngine;

namespace Galactic1.Code.UI.Common
{
    public sealed class WorldUIFollow : MonoBehaviour
    {
        private Vector3 _target;
        private Camera _camera;
        private RectTransform _rect;

        
        
        private void Awake()
        {
            _rect = transform as RectTransform;
        }

        public void Attach(Vector3 target, Camera camera)
        {
            _target = target;
            _camera = camera;
        }

        public void Detach()
        {
            
        }

        private void LateUpdate()
        {
            _rect.position = _camera.WorldToScreenPoint(_target);
        }
    }
}