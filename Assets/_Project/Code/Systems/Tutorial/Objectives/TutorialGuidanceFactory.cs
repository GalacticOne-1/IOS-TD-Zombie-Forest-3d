using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Authoring.Guidance;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Единственная точка GuidanceConditionDefinition → runtime ITutorialGuidanceCondition —
    /// по структуре точная копия TutorialObjectiveFactory (явный Dictionary-реестр, Authoring
    /// не знает о Runtime-типах). Composite-условия (AllOf/AnyOf) резолвят своих детей
    /// рекурсивно через тот же Create(), поэтому вложенность (AND внутри OR и наоборот)
    /// работает "бесплатно", без отдельной обработки в этом классе.
    /// </summary>
    public sealed class TutorialGuidanceFactory
    {
        private readonly Dictionary<Type, Func<TutorialGuidanceConditionDefinition, ITutorialGuidanceCondition>> _map = new();

        public TutorialGuidanceFactory(
            ITutorialInventoryQuery inventory,
            ITutorialInboxQuery inbox,
            ITutorialUIStateQuery uiState,
            IGameLoopStateQuery gameLoop,
            ITutorialSquadQuery squad,
            ITutorialInventoryInteractionQuery interaction,
            ITutorialFacilityPanelQuery facilityPanel,
            ITutorialConstructionQuery construction)
        {
            
            Register<UIScreenOpenGuidanceConditionDefinition>(
                d => new UIScreenOpenGuidanceCondition(uiState, d.screenId, d.expectedOpen));
            
            Register<GameLoopDomainGuidanceConditionDefinition>(
                d => new GameLoopDomainGuidanceCondition(gameLoop, d.domain));
            
            Register<AllOfGuidanceConditionDefinition>(
                d => new AllOfGuidanceCondition(BuildChildren(d.conditions)));
            Register<AnyOfGuidanceConditionDefinition>(
                d => new AnyOfGuidanceCondition(BuildChildren(d.conditions)));
            
            // squad
            Register<UnitSelectGuidanceConditionDefinition>(
                d => new UnitSelectGuidanceCondition(squad, d.expectedFree));
            
            // inventory
            Register<ItemSelectedGuidanceConditionDefinition>(
                d => new ItemSelectedGuidanceCondition(interaction, d.itemId, d.expectedSelected));
            Register<ItemDraggedGuidanceConditionDefinition>(
                d => new ItemDraggedGuidanceCondition(interaction, d.itemId, d.expectedDragged));
            Register<ItemEquippedGuidanceConditionDefinition>(
                d => new ItemEquippedGuidanceCondition(inventory, d.slot, d.itemId, d.expectedEquipped));
            
            Register<InboxItemAvailableGuidanceConditionDefinition>(
                d => new InboxItemAvailableGuidanceCondition(inbox, d.itemId, d.expectedAvailable));
            
            // facility
            Register<FacilityPanelOpenGuidanceConditionDefinition>(
                d => new FacilityPanelOpenGuidanceCondition(facilityPanel, d.facilityType, d.expectedOpen));

            // construction
            Register<FacilityBuiltGuidanceConditionDefinition>(
                d => new FacilityBuiltGuidanceCondition(construction, d.itemId, d.expectedBuilt));
        }

        private void Register<TDef>(Func<TDef, ITutorialGuidanceCondition> factory)
            where TDef : TutorialGuidanceConditionDefinition
            => _map[typeof(TDef)] = def => factory((TDef)def);

        private List<ITutorialGuidanceCondition> BuildChildren(List<TutorialGuidanceConditionDefinition> defs)
        {
            var list = new List<ITutorialGuidanceCondition>(defs?.Count ?? 0);
            if (defs != null)
                foreach (var d in defs)
                    list.Add(Create(d));
            return list;
        }

        /// <summary>Null definition = "условие всегда истинно" — тот же authoring-паттерн,
        /// что у TutorialTransitionDefinition.condition/TutorialGuidanceDefinition.condition
        /// (см. их докстринги): отсутствие явного condition не ошибка, а осознанный
        /// "безусловный" guidance-вариант.</summary>
        public ITutorialGuidanceCondition Create(TutorialGuidanceConditionDefinition definition)
        {
            if (definition == null)
                return AlwaysGuidanceCondition.Instance;

            if (_map.TryGetValue(definition.GetType(), out var factory))
                return factory(definition);

            throw new InvalidOperationException(
                $"[TutorialGuidanceFactory] No factory registered for guidance condition type " +
                $"'{definition.GetType().Name}' (ConditionTypeId='{definition.ConditionTypeId}', " +
                $"asset='{definition.name}'). This is an authoring/registration error — the guidance " +
                "entry referencing this condition cannot resolve until a factory entry is added for this type.");
        }
    }
}