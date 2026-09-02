
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Enemies.Definitions;
using Galactic1.Code.Gameplay.Enemies.Factories;
using Galactic1.Code.Gameplay.Enemies.Modifiers;
using Galactic1.Code.Gameplay.Enemies.Spawning.Positioning;
using Galactic1.Code.Gameplay.Enemies.Spawning.Requests;
using Galactic1.Code.Gameplay.Enemies.Variants;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Enemies.Spawning
{
    public sealed class EnemySpawnPipeline
    {
        private readonly EnemyVariantResolver _variantResolver;
        private readonly EnemyPresentationFactory _presentationFactory;
        private readonly EnemyModifierPipeline _modifierPipeline;
        private readonly EnemyRuntimeDefinitionBuilder _definitionBuilder;
        private readonly EnemyRuntimeFactory _runtimeFactory;
        private readonly EnemySpawnPointResolver _spawnPointResolver;
        private readonly EnemyArchetypeDefinitionCache _archetypeCache;
        private readonly RaidRuntime _raid;

        public EnemySpawnPipeline(
            EnemyVariantResolver variantResolver,
            EnemyPresentationFactory presentationFactory,
            EnemyModifierPipeline modifierPipeline,
            EnemyRuntimeDefinitionBuilder definitionBuilder,
            EnemyRuntimeFactory runtimeFactory,
            EnemySpawnPointResolver spawnPointResolver,
            EnemyArchetypeDefinitionCache archetypeCache,
            RaidRuntime raid)
        {
            _variantResolver = variantResolver;
            _presentationFactory = presentationFactory;
            _modifierPipeline = modifierPipeline;
            _definitionBuilder = definitionBuilder;
            _runtimeFactory = runtimeFactory;
            _spawnPointResolver = spawnPointResolver;
            _archetypeCache = archetypeCache;
            _raid = raid;
        }

        public EnemySpawnResult Spawn(EnemySpawnRequest request)
        {
            var context = new EnemySpawnContext { Request = request };

            // ── 1: архетип ────────────────────────────────────────────
            context.ArchetypeDefinition = _archetypeCache.Get(request.EnemyId);
            if (context.ArchetypeDefinition == null)
            {
                var reason = $"Архетип не найден для EnemyId={request.EnemyId}";
                Debug.LogError($"[EnemySpawnPipeline] {reason}");
                return EnemySpawnResult.Failed(reason);
            }

            // ── 2: позиция ────────────────────────────────────────────
            context.SpawnPosition = _spawnPointResolver.Resolve(request);

            // ── 3: резолюция варианта ─────────────────────────────────
            var resolveContext = EnemyVariantResolveContext.From(_raid.LocationDef);

            context.VariantResult = _variantResolver.Resolve(
                context.ArchetypeDefinition.Presentation,
                resolveContext);

            // ── 4: presentation ───────────────────────────────────────
            var presentation = _presentationFactory.Build(
                context.ArchetypeDefinition.Presentation,
                context.VariantResult);

            // ── 5: MutationContext ────────────────────────────────────
            context.MutationContext = new EnemyStatMutationContext(
                BuildBaseStats(context.ArchetypeDefinition),
                presentation);

            // ── 6: модификаторы ───────────────────────────────────────
            _modifierPipeline.Apply(context);

            // ── 7: RuntimeDefinition ──────────────────────────────────
            context.RuntimeDefinition = _definitionBuilder.Build(
                _raid,
                context.ArchetypeDefinition,
                context.MutationContext);

            // ── 8: Runtime ────────────────────────────────────────────
            var runtime = _runtimeFactory.Create(
                context.RuntimeDefinition,
                context.SpawnPosition,
                context.Request.Source);

            // ── 9: регистрация ────────────────────────────────────────
            _raid.Enemies.Register(runtime);

#if UNITY_EDITOR
            var m = "Modifiers: ";
            foreach (var modifier in request.ModifierIds) m += $"{modifier}\n";
            DLog.Alert(
                $"[EnemySpawnPipeline] OK | {runtime.Id} | " +
                $"Архетип={request.EnemyId} | Variant={request.VariantId} | " +
                $"Elite={context.MutationContext.IsElite} | {m}");
#endif
            return EnemySpawnResult.Succeeded(runtime);
        }

        private static Dictionary<StatId, float> BuildBaseStats(
            EnemyArchetypeDefinition archetype) =>
            new()
            {
                [StatId.Health] = archetype.BaseHealth,
                [StatId.Armor] = archetype.BaseArmor,
                [StatId.Damage] = archetype.Combat.Damage,
                [StatId.ReloadSpeed] = archetype.Combat.AttackCooldown,
                [StatId.AttackRange] = archetype.Combat.AttackRange,
            };
    }
}