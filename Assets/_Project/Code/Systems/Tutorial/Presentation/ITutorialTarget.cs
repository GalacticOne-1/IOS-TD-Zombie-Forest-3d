using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Абстракция UI/world-объекта, на который тутор может указать
    /// (highlight/arrow/camera focus), без хранения прямых ссылок на объекты
    /// сцены в persistent-состоянии.</summary>
    public interface ITutorialTarget
    {
        TutorialTargetId TargetId { get; }
        /// <summary>Null, если таргет не является UI-элементом.</summary>
        RectTransform UIAnchor { get; }
        /// <summary>Null, если таргет не является world-объектом.</summary>
        Transform WorldAnchor { get; }
    }
}
