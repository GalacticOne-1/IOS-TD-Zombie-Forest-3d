using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Authoring.Objectives;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Единственная точка Definition → Runtime. Authoring НЕ знает о рантайм-типах
    /// (TutorialObjectiveDefinition.CreateRuntime() отсутствует — исправлено в
    /// P1 corrective pass). Явный Dictionary-реестр вместо рефлексии/атрибутов —
    /// проще отлаживать, не требует конвенций именования.
    /// </summary>
    public sealed class TutorialObjectiveFactory
    {
        private readonly Dictionary<Type, Func<TutorialObjectiveDefinition, ITutorialObjective>> _map = new();

        public TutorialObjectiveFactory(
            ITutorialInventoryQuery inventory,
            ITutorialSquadQuery squad,
            IGameLoopStateQuery gameLoop)
        {
            Register<EnemyKilledObjectiveDefinition>(d => new EnemyKilledObjective(d.requiredCount));
            Register<ItemEquippedObjectiveDefinition>(d => new ItemEquippedObjective(inventory, d.slot, d.itemId));
            Register<ResourceAmountObjectiveDefinition>(d => new ResourceAmountObjective(inventory, d.itemId, d.requiredAmount));
            Register<SquadSizeObjectiveDefinition>(d => new SquadSizeObjective(squad, d.requiredSize));
            Register<GameLoopDomainReachedObjectiveDefinition>(d => new GameLoopDomainReachedObjective(gameLoop, d.targetDomain));
            Register<DomainTransitionObjectiveDefinition>(d => new DomainTransitionObjective(gameLoop, d.fromDomain, d.toDomain));
            Register<RaidCompletedObjectiveDefinition>(d => new RaidCompletedObjective(d.requireVictory));
            Register<ExitReachedObjectiveDefinition>(_ => new ExitReachedObjective());
            Register<ContainerOpenedObjectiveDefinition>(_ => new ContainerOpenedObjective());
            Register<LootCollectedObjectiveDefinition>(_ => new LootCollectedObjective());
            Register<ItemCollectedObjectiveDefinition>(d => new ItemCollectedObjective(d.itemId, d.requiredAmount));
            Register<WorldMapLocationSelectedObjectiveDefinition>(d => new WorldMapLocationSelectedObjective(d.locationId));
            Register<ButtonPressedObjectiveDefinition>(d => new ButtonPressedObjective(d.targetId));
            Register<UIScreenOpenedObjectiveDefinition>(d => new UIScreenOpenedObjective(d.screenId));
            Register<UnitMovedObjectiveDefinition>(_ => new UnitMovedObjective());
            Register<TargetSelectedObjectiveDefinition>(_ => new TargetSelectedObjective());
            Register<WeaponFiredObjectiveDefinition>(_ => new WeaponFiredObjective());
        }

        private void Register<TDef>(Func<TDef, ITutorialObjective> factory) where TDef : TutorialObjectiveDefinition
            => _map[typeof(TDef)] = def => factory((TDef)def);

        /// <summary>
        /// Fix: раньше при неизвестном типе Definition сюда возвращался null, который потом
        /// оседал в TutorialObjectiveRuntimeState и падал NRE где-то глубоко внутри
        /// TutorialStepRuntimeState.IsCompleted — далеко от места настоящей ошибки. Теперь
        /// авторинг-ошибка (незарегистрированный тип объектива) падает немедленно и явно,
        /// прямо в точке Create(), с полным контекстом (тип + asset name + ObjectiveTypeId).
        /// </summary>
        public ITutorialObjective Create(TutorialObjectiveDefinition definition)
        {
            if (definition == null)
                throw new InvalidOperationException("[TutorialObjectiveFactory] Received a null TutorialObjectiveDefinition.");

            if (_map.TryGetValue(definition.GetType(), out var factory))
                return factory(definition);

            throw new InvalidOperationException(
                $"[TutorialObjectiveFactory] No factory registered for objective type '{definition.GetType().Name}' " +
                $"(ObjectiveTypeId='{definition.ObjectiveTypeId}', asset='{definition.name}'). " +
                "This is an authoring/registration error — the tutorial step referencing this objective cannot run " +
                "until a factory entry is added for this type.");
        }
    }
}
