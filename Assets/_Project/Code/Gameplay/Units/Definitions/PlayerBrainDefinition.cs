namespace Galactic1.Code.Gameplay.Units.Definitions
{
    /// <summary>
    /// Immutable brain behaviour settings для игрока.
    /// Строится из PlayerBrainConfig (SO) в PlayerRuntimeFactory.
    /// </summary>
    public sealed class PlayerBrainDefinition
    {
        public float AutoEngageRange { get; }
        public float AutoCoverRange { get; }
        public float ReEngageDelay { get; }

        public PlayerBrainDefinition(
            float autoEngageRange,
            float autoCoverRange,
            float reEngageDelay)
        {
            AutoEngageRange = autoEngageRange;
            AutoCoverRange = autoCoverRange;
            ReEngageDelay = reEngageDelay;
        }
    }
}