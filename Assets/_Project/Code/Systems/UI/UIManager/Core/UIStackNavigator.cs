using System.Collections.Generic;


namespace Galactic1.UI.Core
{
    public class UIStackNavigator
    {
        private readonly Stack<UIScreenId> history = new();


        public void Push(UIScreenId id)
        {
            if (history.Count == 0 || history.Peek() != id)
                history.Push(id);
        }


        public UIScreenId Pop()
        {
            if (history.Count == 0) return UIScreenId.NULL;
            history.Pop();
            return history.Count > 0 ? history.Peek() : UIScreenId.NULL;
        }


        public UIScreenId Current => history.Count == 0 ? UIScreenId.NULL : history.Peek();


        public void Clear() => history.Clear();
    }
}