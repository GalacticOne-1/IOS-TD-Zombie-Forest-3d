using Galactic1.UI.Core;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Требует UIScreenOpenedEvent — интеграционная точка, одна строка
    /// в UIScreenManager.OpenScreenRoutine, см. Integration/.</summary>
    public sealed class UIScreenOpenedObjective : TutorialEventObjectiveBase<UIScreenOpenedEvent>
    {
        private readonly UIScreenId _screenId;
        public UIScreenOpenedObjective(UIScreenId screenId) => _screenId = screenId;

        protected override bool EvaluateEvent(UIScreenOpenedEvent e) => e.ScreenId == _screenId;
    }
}
