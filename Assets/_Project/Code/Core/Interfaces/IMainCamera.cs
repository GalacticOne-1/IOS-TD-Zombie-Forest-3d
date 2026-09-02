using Galactic1.Code.Cameras.Configs;
using UnityEngine;

namespace Galactic1.Code.Cameras
{
    public interface IMainCamera : IGameService
    {
        Camera Camera { get; }

        void Activate();
        void OnLevelLoaded(
            CameraConfig cameraConfig,
            Vector3 startPosition,
            Vector3 newMinBounds,
            Vector3 newMaxBounds,
            float? startZoom = null);


        void FocusOnPosition(Vector3 target, float duration = .2f);
    }
}