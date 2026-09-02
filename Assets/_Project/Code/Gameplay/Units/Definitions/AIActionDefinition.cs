using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Immutable runtime utility settings for one AI action.
    ///
    /// Строится из ActionWeightEntry (authoring) в EnemyAIDefinitionBuilder.
    /// Используется Brain в think-loop для применения weights и enabled-флага.
    /// </summary>
    public sealed class AIActionDefinition
    {
        public AIActionType Type { get; }
        public float Weight { get; }
        public bool Enabled { get; }

        public AIActionDefinition(AIActionType type, float weight, bool enabled)
        {
            Type = type;
            Weight = weight;
            Enabled = enabled;
        }
    }
}