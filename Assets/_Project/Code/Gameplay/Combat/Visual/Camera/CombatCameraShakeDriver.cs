using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Unity bridge for runtime camera shake.
    ///
    /// RESPONSIBILITY:
    /// Applies runtime offsets to camera rig.
    /// </summary>
    public sealed class CombatCameraShakeDriver : MonoBehaviour, IGameService
    {
        [SerializeField] private Transform cameraRoot;

        private CombatCameraShakeService _shake;

        private Vector3 _basePosition;

        private Quaternion _baseRotation;

        public void Construct(CombatCameraShakeService shake)
        {
            _shake = shake;
        }

        private void Awake()
        {
            _basePosition =
                cameraRoot.localPosition;

            _baseRotation =
                cameraRoot.localRotation;
        }

        private void LateUpdate()
        {
            if (_shake == null)
                return;

            _shake.Tick(Time.deltaTime);

            cameraRoot.localPosition =
                _basePosition +
                _shake.GetPositionOffset();

            cameraRoot.localRotation =
                _baseRotation *
                _shake.GetRotationOffset();
        }
    }
}