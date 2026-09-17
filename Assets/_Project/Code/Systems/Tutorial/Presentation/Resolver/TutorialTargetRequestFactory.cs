using System;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Единственная точка TutorialTargetQuery (Authoring) → TutorialTargetRequest (Runtime) —
    /// та же роль на Authoring/Runtime границе, что TutorialObjectiveFactory/
    /// TutorialGuidanceFactory уже играют для Definition → Runtime condition/objective.
    /// Ничего не резолвит сама — чистый мэппинг данных.
    /// </summary>
    public sealed class TutorialTargetRequestFactory
    {
        /// <summary>Null query = "highlight не нужен" — тот же authoring-паттерн, что
        /// condition == null в остальной системе.</summary>
        public TutorialTargetRequest Create(TutorialTargetQuery query)
        {
            switch (query)
            {
                case null: return null;
                case TutorialFixedTargetQuery q: return new TutorialFixedTargetRequest(q.targetId);
                case TutorialInventoryItemQuery q: return new TutorialInventoryItemRequest(q.itemId);
                case TutorialInboxItemQuery q: return new TutorialInboxItemRequest(q.itemId);
                case TutorialUnitSearchQuery q: return new TutorialUnitSearchRequest(q.criteria);
                case TutorialFacilityCardQuery q: return new TutorialFacilityCardRequest(q.facilityItemId);
                case TutorialConstructionTabQuery q: return new TutorialConstructionTabRequest(q.category);
                default:
                    throw new InvalidOperationException(
                        $"[TutorialTargetRequestFactory] No mapping for query type '{query.GetType().Name}'. " +
                        "Authoring/registration error — add a case here for the new TutorialTargetQuery subtype.");
            }
        }
    }
}