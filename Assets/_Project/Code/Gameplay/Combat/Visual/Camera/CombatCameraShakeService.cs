
using Galactic1.Code.Cameras;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    public sealed class CombatCameraShakeService
    {
        private readonly CombatCameraShakeConfig _config;
        // =====================================================
        // STATE
        // =====================================================

        private readonly Camera _camera;
        private readonly CameraController _cameraController;

        private float _trauma;

        private float _suppression;

        private float _budgetUsed;

        private float _lowFrequency;

        private float _highFrequency;

        private Vector3 _positionOffset;

        private Vector3 _rotationOffset;

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public CombatCameraShakeService(
            Camera camera,
            CameraController cameraController,
            CombatCameraShakeConfig config)
        {
            _camera = camera;
            _cameraController = cameraController;
            _config = config;
        }

        // =====================================================
        // PUBLIC
        // =====================================================

        /// <summary>
        /// Adds explosion trauma.
        /// </summary>
        public void AddExplosionShake(
            Vector3 explosionPosition,
            float radius,
            float intensity,
            float lowFrequency,
            float highFrequency)
        {
            if (_budgetUsed >= _config.shakeBudget)
                return;

            Vector3 viewport = _camera.WorldToViewportPoint(explosionPosition);

            // BEHIND CAMERA
            if (viewport.z < 0f)
                return;

            // =================================================
            // DISTANCE ATTENUATION
            // =================================================

            Vector3 cameraPos = _cameraController.FocusPosition;

            cameraPos.y = explosionPosition.y;

            float distance = Vector3.Distance(cameraPos, explosionPosition);

            if (distance > radius)
                return;

            float distance01 = 1f - Mathf.Clamp01(distance / radius);

            // =================================================
            // SCREEN EDGE ATTENUATION
            // =================================================

            float centerDistance = Vector2.Distance(
                new Vector2(
                    viewport.x,
                    viewport.y),
                new Vector2(
                    0.5f,
                    0.5f));

            float edgeFactor = 1f - Mathf.Clamp01(centerDistance / 0.75f);

            // =================================================
            // FINAL INTENSITY
            // =================================================

            float finalIntensity =
                intensity *
                distance01 *
                edgeFactor;

            if (finalIntensity <= 0.001f)
                return;

            // =================================================
            // BUDGET
            // =================================================

            float allowed = Mathf.Min(finalIntensity, _config.shakeBudget - _budgetUsed);

            _budgetUsed += allowed;

            // =================================================
            // TRAUMA
            // =================================================

            _trauma = Mathf.Clamp01(_trauma + allowed);

            _lowFrequency = Mathf.Max(_lowFrequency, lowFrequency);

            _highFrequency = Mathf.Max(_highFrequency, highFrequency);
        }

        /// <summary>
        /// Adds suppression camera jitter.
        /// </summary>
        public void AddSuppressionShake(
            float intensity)
        {
            _suppression =
                Mathf.Clamp01(
                    _suppression + intensity);
        }

        /// <summary>
        /// Runtime update.
        /// </summary>
        public void Tick(float dt)
        {
            // =================================================
            // DECAY
            // =================================================

            _trauma =
                Mathf.MoveTowards(
                    _trauma,
                    0f,
                    _config.traumaDecay * dt);

            _suppression =
                Mathf.MoveTowards(
                    _suppression,
                    0f,
                    2.5f * dt);

            _budgetUsed = 0f;

            // =================================================
            // SHAKE POWER
            // =================================================

            float shake =
                _trauma * _trauma;

            // =================================================
            // LOW FREQUENCY
            // =================================================

            float lowX =
                Mathf.PerlinNoise(
                    Time.time * _lowFrequency,
                    0f) - 0.5f;

            float lowY =
                Mathf.PerlinNoise(
                    0f,
                    Time.time * _lowFrequency) - 0.5f;

            // =================================================
            // HIGH FREQUENCY
            // =================================================

            float highX =
                Mathf.PerlinNoise(
                    Time.time * _highFrequency,
                    10f) - 0.5f;

            float highY =
                Mathf.PerlinNoise(
                    10f,
                    Time.time * _highFrequency) - 0.5f;

            // =================================================
            // FINAL NOISE
            // =================================================

            float finalX =
                lowX * 0.7f +
                highX * 0.3f;

            float finalY =
                lowY * 0.7f +
                highY * 0.3f;

            // =================================================
            // POSITION SHAKE
            // =================================================

            _positionOffset =
                new Vector3(
                    finalX,
                    finalY,
                    0f) *
                (shake * _config.maxPositionOffset);

            // =================================================
            // ROTATION SHAKE
            // =================================================

            _rotationOffset =
                new Vector3(
                    finalY,
                    finalX,
                    finalX) *
                (shake * _config.maxRotation);

            // =================================================
            // SUPPRESSION JITTER
            // =================================================

            if (_suppression > 0f)
            {
                float sx =
                    Random.Range(-1f, 1f);

                float sy =
                    Random.Range(-1f, 1f);

                _positionOffset +=
                    new Vector3(
                        sx,
                        sy,
                        0f) *
                    (_suppression *
                     _config.suppressionMax);
            }
        }

        // =====================================================
        // GETTERS
        // =====================================================

        public Vector3 GetPositionOffset()
        {
            return _positionOffset;
        }

        public Quaternion GetRotationOffset()
        {
            return Quaternion.Euler(
                _rotationOffset);
        }
    }
}