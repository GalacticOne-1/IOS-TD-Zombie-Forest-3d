namespace Galactic1.Code.Gameplay.Units.Definitions
{
    public sealed class PerceptionDefinition
    {
        public float DetectionRadius { get; }
        public float UpdateInterval { get; }
        public float HearingRadius { get; }
        public float HearingSensitivity { get; }
        public float ViewAngle { get; set; } = 360;

        public PerceptionDefinition(
            float detectionRadius,
            float updateInterval,
            float hearingRadius,
            float hearingSensitivity)
        {
            DetectionRadius = detectionRadius;
            UpdateInterval = updateInterval;
            HearingRadius = hearingRadius;
            HearingSensitivity = hearingSensitivity;
        }
    }
}