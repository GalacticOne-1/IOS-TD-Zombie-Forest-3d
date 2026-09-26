using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>World-аналог DynamicRectTutorialTarget — оборачивает динамически найденный
    /// Transform (ближайший живой враг группы). Не регистрируется в TutorialTargetRegistry,
    /// TargetId всегда null — путь резолва полностью обходит реестр, как и у Rect-варианта.</summary>
    public sealed class DynamicWorldTutorialTarget : ITutorialTarget
    {
        public TutorialTargetId TargetId => null;
        public RectTransform UIAnchor => null;
        public Transform WorldAnchor { get; }

        public DynamicWorldTutorialTarget(Transform worldAnchor) => WorldAnchor = worldAnchor;
    }
}