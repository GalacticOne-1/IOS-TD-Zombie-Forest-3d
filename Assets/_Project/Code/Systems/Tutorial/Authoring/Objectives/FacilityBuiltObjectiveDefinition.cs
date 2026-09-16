using UnityEngine;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    /// <summary>
    /// Event-семантика: "построил здание ЗА ВРЕМЯ ЭТОГО ШАГА" — по тому же принципу, что
    /// ItemCollectedObjective/InboxItemCollectedObjective. Не ретроактивен: здание,
    /// построенное ДО активации шага, не засчитывается (ближайший аналог по природе
    /// события — RecruitCompletedObjectiveDefinition, тоже подписывается на плоский
    /// C#-event GameLoopContext, а не на EventBus&lt;T&gt;).
    /// </summary>
    [CreateAssetMenu(fileName = "Objective_FacilityBuilt",
        menuName = "Game Configs/Tutorial/Objectives/Facility Built")]
    public sealed class FacilityBuiltObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "FacilityBuilt";

        [Tooltip("Пусто = засчитывается постройка ЛЮБОГО здания. Иначе — только этой " +
                 "конкретной конфигурации (совпадает с FacilityModule.Item.Id — тот же " +
                 "ItemId, по которому GameContent.Facilities резолвит FacilityModule).")]
        public ItemId itemId;
    }
}
