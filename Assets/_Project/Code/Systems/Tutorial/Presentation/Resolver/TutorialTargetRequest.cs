using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Construction.Configs;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Runtime-представление "что нужно найти" — результат конвертации authoring
    /// TutorialTargetQuery через TutorialTargetRequestFactory. Обычный C#-класс: без
    /// [Serializable]/[SerializeReference], не ScriptableObject-граф, никогда не
    /// пересекает границу обратно в Editor/authoring слой. Тип конкретного request
    /// определяет, какой ITutorialTargetResolver его обработает.
    /// </summary>
    public abstract class TutorialTargetRequest { }

    public sealed class TutorialFixedTargetRequest : TutorialTargetRequest
    {
        public readonly TutorialTargetId TargetId;
        public TutorialFixedTargetRequest(TutorialTargetId targetId) => TargetId = targetId;
    }

    public sealed class TutorialInventoryItemRequest : TutorialTargetRequest
    {
        public readonly ItemId ItemId;
        public TutorialInventoryItemRequest(ItemId itemId) => ItemId = itemId;
    }

    public sealed class TutorialInboxItemRequest : TutorialTargetRequest
    {
        public readonly ItemId ItemId;
        public TutorialInboxItemRequest(ItemId itemId) => ItemId = itemId;
    }

    public sealed class TutorialUnitSearchRequest : TutorialTargetRequest
    {
        public readonly TutorialUnitSearchCriteria Criteria;
        public TutorialUnitSearchRequest(TutorialUnitSearchCriteria criteria) => Criteria = criteria;
    }

    public sealed class TutorialFacilityCardRequest : TutorialTargetRequest
    {
        public readonly ItemId FacilityItemId;
        public TutorialFacilityCardRequest(ItemId facilityItemId) => FacilityItemId = facilityItemId;
    }

    public sealed class TutorialConstructionTabRequest : TutorialTargetRequest
    {
        public readonly ConstructionCategory Category;
        public TutorialConstructionTabRequest(ConstructionCategory category) => Category = category;
    }
}