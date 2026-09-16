using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Construction.Configs;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Рантайм-снэпшот "что сейчас показать" для одного guidance-варианта — Unity-free
    /// подмножество presentation-данных, отражающее на рантайм-стороне авторскую
    /// TutorialGuidanceTargetDefinition. Обёрнут в отдельный тип, а не передаётся как
    /// голые поля, чтобы TutorialGuidanceRuntimeState могла сравнивать "старый/новый
    /// resolved target" одной ссылкой (см. её докстринг про dedupe).
    ///
    /// HighlightTargetId / HighlightItemId / HighlightFacilityItemId — взаимоисключимые
    /// альтернативные способы резолва highlight (см. TutorialGuidanceTargetDefinition
    /// докстринг): фиксированный таргет из реестра или "слот/карточка, где сейчас лежит X",
    /// резолвится заново на каждый показ в TutorialPresentationService.
    /// </summary>
    public sealed class TutorialGuidanceTarget
    {
        public readonly HighlightMode HighlightMode;
        public readonly TutorialTargetId HighlightTargetId;
        public readonly ItemId HighlightItemId;
        public readonly ItemId HighlightInboxItemId;
        public readonly TutorialUnitSearchCriteria HighlightUnitSearch;
        public readonly ItemId HighlightFacilityItemId;
        public readonly ConstructionCategory? HighlightConstructionTabCategory;
        public readonly TutorialTargetId ArrowTargetId;
        public readonly TutorialTargetId CameraFocusTargetId;

        public TutorialGuidanceTarget(
            HighlightMode highlightMode,
            TutorialTargetId highlightTargetId, 
            ItemId highlightItemId, 
            ItemId highlightInboxItemId, 
            TutorialUnitSearchCriteria highlightUnitSearch,
            TutorialTargetId arrowTargetId,
            TutorialTargetId cameraFocusTargetId,
            ItemId highlightFacilityItemId = null,
            ConstructionCategory? highlightConstructionTabCategory = null)
        {
            HighlightMode = highlightMode;
            HighlightTargetId = highlightTargetId;
            HighlightItemId = highlightItemId;
            HighlightInboxItemId = highlightInboxItemId;
            HighlightUnitSearch = highlightUnitSearch;
            HighlightFacilityItemId = highlightFacilityItemId;
            HighlightConstructionTabCategory = highlightConstructionTabCategory;
            ArrowTargetId = arrowTargetId;
            CameraFocusTargetId = cameraFocusTargetId;
        }
    }
}