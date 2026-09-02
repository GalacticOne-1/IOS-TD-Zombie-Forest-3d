namespace Galactic1.Code.Systems.Raid.Enemies
{
    public sealed class TargetingDefinition
    {
        public float LoseTargetRange { get; }
        public float LoseTargetDelay { get; }
        public float MemoryDecayRate { get; }
        public float RetargetCooldown { get; }
        public float ReacquireRadius { get; }
        public float RecentTargetBias { get; }

        public TargetingDefinition(
            float loseTargetRange,
            float loseTargetDelay,
            float memoryDecayRate,
            float retargetCooldown,
            float reacquireRadius,
            float recentTargetBias)
        {
            LoseTargetRange = loseTargetRange;
            LoseTargetDelay = loseTargetDelay;
            MemoryDecayRate = memoryDecayRate;
            RetargetCooldown = retargetCooldown;
            ReacquireRadius = reacquireRadius;
            RecentTargetBias = recentTargetBias;
        }
    }
}