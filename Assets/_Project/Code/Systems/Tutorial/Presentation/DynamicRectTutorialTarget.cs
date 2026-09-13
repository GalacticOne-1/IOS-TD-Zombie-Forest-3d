using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// ITutorialTarget-обёртка над динамически найденным RectTransform (слот инвентаря,
    /// показывающий нужный предмет прямо сейчас) — в отличие от TutorialTargetBehaviour,
    /// не регистрируется в TutorialTargetRegistry и не имеет стабильного TargetId: этот
    /// путь резолва (ITutorialItemSlotTargetProvider → сюда) полностью обходит реестр,
    /// поэтому TargetId здесь принципиально null (не участвует ни в каком lookup).
    /// </summary>
    public sealed class DynamicRectTutorialTarget : ITutorialTarget
    {
        public TutorialTargetId TargetId => null;
        public RectTransform UIAnchor { get; }
        public Transform WorldAnchor => null;

        public DynamicRectTutorialTarget(RectTransform uiAnchor) => UIAnchor = uiAnchor;
    }
}
