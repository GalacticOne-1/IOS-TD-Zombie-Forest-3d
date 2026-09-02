using System.Collections.Generic;

namespace Galactic1.Code.UI.Interaction
{
    public sealed class UIBlockRegistry
    {
        private readonly List<IUIBlockable> _elements = new();

        public IReadOnlyList<IUIBlockable> Elements => _elements;
        
        
        public void Register(IUIBlockable element)
        {
            if (!_elements.Contains(element))
                _elements.Add(element);
        }

        public void Unregister(IUIBlockable element)
        {
            _elements.Remove(element);
        }

    }
}