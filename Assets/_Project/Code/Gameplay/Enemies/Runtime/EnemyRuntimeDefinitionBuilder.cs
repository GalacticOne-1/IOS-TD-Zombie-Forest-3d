
using Galactic1.Code.Gameplay.Enemies.Modifiers;
using Galactic1.Code.Gameplay.Enemies.Stats;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Enemies;

namespace Galactic1.Code.Gameplay.Enemies.Definitions
{
    public sealed class EnemyRuntimeDefinitionBuilder
    {
        public EnemyRuntimeDefinition Build(
            RaidRuntime raidRuntime,
            EnemyArchetypeDefinition archetype,
            EnemyStatMutationContext mutationContext)
        {
            var snapshot = new EnemyStatsSnapshot(mutationContext.Stats);
            var movement = ApplyMovementOverride(archetype.Movement, mutationContext.Movement);

            return new EnemyRuntimeDefinition(
                archetype.EnemyId,
                archetype.DisplayName,
                mutationContext.Presentation,
                archetype.AI,
                movement,
                archetype.Perception,
                archetype.Targeting,
                archetype.Combat,
                archetype.Melee,
                archetype.Pack,
                snapshot,
                raidRuntime.Scenario.AIProfile,
                archetype.Presentation.AudioConfig,
                0f,
                string.Empty,
                mutationContext.IsElite,
                mutationContext.Presentation?.GameplayPrefabId ?? string.Empty);
        }

        private static MovementDefinition ApplyMovementOverride(
            MovementDefinition baseMovement,
            MovementOverride movOverride)
        {
            if (movOverride == null) return baseMovement;

            float walk = baseMovement.WalkSpeed;
            float run = baseMovement.RunSpeed;

            if (movOverride.WalkSpeedMultiplier.HasValue)
                walk *= movOverride.WalkSpeedMultiplier.Value;
            if (movOverride.RunSpeedMultiplier.HasValue)
                run *= movOverride.RunSpeedMultiplier.Value;

            return new MovementDefinition(
                walk, run,
                baseMovement.RotationSpeed,
                baseMovement.RepathInterval,
                baseMovement.StoppingDistance);
        }
    }
}