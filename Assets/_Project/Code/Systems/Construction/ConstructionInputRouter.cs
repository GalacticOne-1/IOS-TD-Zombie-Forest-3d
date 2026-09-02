using UnityEngine;
using Galactic1.Code.Gameplay.Construction;
using Galactic1.Code.Systems;
using Galactic1.Code.Utility;

namespace Galactic1.Code.Gameplay
{
    /// <summary>
    /// Input Router для режима строительства.
    /// Читает input и передает события в ConstructionModeController (FSM).
    /// </summary>
    public class ConstructionInputRouter : MonoBehaviour
    {
        [SerializeField] private LayerMask objectLayer;
        [SerializeField] private LayerMask groundLayer;

        private ConstructionModeController _controller;
        private Camera _camera;
        private UIDetector _uiDetector;
        
        

        public void Initialize(
            ConstructionModeController controller,
            Camera camera,
            UIDetector uiDetector)
        {
            _controller = controller;
            _camera = camera;
            _uiDetector = uiDetector;
        }

        void Update()
        {
            if (!_controller || !_controller.IsActive)
                return;

            // tap gesture
            if (GestureSystem.Tap && CanProcessInput())
            {
                ProcessTap();
            }
        }
        
        private bool CanProcessInput()
        {
            return !_uiDetector.IsPointerOverUI &&
                   !_uiDetector.HasUIUnderCursor();
        }

        private void ProcessTap()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            // 1️⃣ Проверяем клик по объекту
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, objectLayer))
            {
                var buildable = hit.collider.GetComponentInParent<BuildableObject>();

                if (buildable != null)
                {
                    _controller.OnObjectClicked(buildable);
                    return;
                }
            }

            // 2️⃣ Проверяем клик по земле
            if (Physics.Raycast(ray, out RaycastHit groundHit, 1000f, groundLayer))
            {
                Vector3 world = groundHit.point;

                var coord = _controller.Placement
                    .ConstructionService
                    .Coordinates
                    .WorldToCell(world);

                _controller.OnCellClicked(coord);
                return;
            }

            // 3️⃣ Клик в пустоту
            _controller.OnEmptyClicked();
        }
    }
}