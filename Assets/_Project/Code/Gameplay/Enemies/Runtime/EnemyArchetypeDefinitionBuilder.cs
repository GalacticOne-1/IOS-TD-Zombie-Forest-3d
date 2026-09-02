
using Galactic1.Code.Gameplay.Enemies.Definitions;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Game.Meta.Enemy;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Factories
{
    public sealed class EnemyArchetypeDefinitionBuilder
    {
        private readonly EnemyPresentationDefinitionBuilder _presentationBuilder = new();

        public EnemyArchetypeDefinition Build(EnemyArchetypeConfig config)
        {
            return new EnemyArchetypeDefinition(
                config.Id,
                config.DisplayName,
                _presentationBuilder.Build(config.Presentation),
                EnemyAIDefinitionBuilder.Build(config.AI),
                BuildMovement(config.Movement),
                BuildTargeting(config.Targeting),
                BuildPerception(config.Perception),
                BuildPack(config.Pack),
                BuildCombat(config.Combat),
                BuildMelee(config.Combat),
                config.Stats.BaseStats.Health,
                config.Stats.BaseStats.Armor,
                config.Stats.BaseStats.Poise,
                config.Stats.BaseStats.StunResistance);
        }

        private static MovementDefinition BuildMovement(MovementConfig c) =>
            new(c.WalkSpeed, c.RunSpeed, c.RotationSpeed, c.RepathInterval, c.StoppingDistance);

        private static TargetingDefinition BuildTargeting(TargetingConfig c) =>
            new(c.LoseTargetRange, c.LoseTargetDelay, c.MemoryDecayRate,
                c.RetargetCooldown, c.ReacquireRadius, c.RecentTargetBias);

        private static PerceptionDefinition BuildPerception(PerceptionConfig c) =>
            new(c.detectionRadius, c.updateInterval, c.hearingRadius, c.hearingSensitivity);

        private static EnemyPackDefinition BuildPack(ZombiePackConfig c) =>
            new(c.EncircleRadius, c.SlotAngleStep, c.MinSlotDistance,
                c.PackSlotWeight, c.MaxAttackersPerTarget);

        private static EnemyCombatDefinition BuildCombat(EnemyCombatConfig c) =>
            new(c.Damage, c.CritChance, c.AttackRange, c.AttackCooldown,
                c.Windup, c.Recovery, c.CanStrafe, c.CanChainAttacks, c.CanUseSpecialAttack);

        private static MeleeCombatDefinition BuildMelee(EnemyCombatConfig c) =>
            new(c.AttackRange, c.AttackRange * 0.7f,
                new Vector3(0f, 1.0f, 0.6f),
                c.Damage, c.AttackCooldown, 60f);
    }
}