using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>Overlay-панель с текстом, привязанная к guidance-entry — параллельный,
    /// независимый от highlight/arrow/camera canal. Использует ТОТ ЖЕ condition, что и
    /// основной guidance-резолв этого entry (см. TutorialGuidanceDefinition.condition),
    /// но резолвится и рендерится отдельно (TutorialGuidancePanelRuntimeState), поэтому
    /// никак не влияет на "first satisfied wins" резолв highlight/arrow/camera.
    ///
    /// enabled=false по умолчанию — существующие ассеты без панели не затронуты.</summary>
    [Serializable]
    public sealed class TutorialGuidancePanelDefinition
    {
        [Tooltip("Этот guidance-entry также управляет overlay-панелью с текстом.")]
        public bool enabled;

        [TextArea]
        public string text;

        [Tooltip("true = показывается ОДИН РАЗ за жизнь шага — после клика по панели " +
                 "(Dismiss) больше не появляется для этого entry, даже если condition " +
                 "снова станет истинным (например экран закрыли и открыли заново).\n" +
                 "false = появляется/исчезает синхронно с condition, как highlight — " +
                 "может показываться повторно за один шаг.")]
        public bool oneShot = true;
    }
}