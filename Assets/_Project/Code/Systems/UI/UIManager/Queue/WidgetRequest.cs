using System;

namespace Galactic1.UI.Core
{
    public class WidgetRequest
    {
        public UIScreenId ScreenId;
        public Action<Action> OnShow;
        public int Priority;

        public void Show(Action onDone)
        {
            OnShow?.Invoke(onDone);
        }
    }
}