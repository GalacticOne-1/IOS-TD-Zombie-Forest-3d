using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.UI.Interaction
{
    public sealed class UILayerController : IUILayerService
    {
        private readonly List<UILayerElement> _elements = new();


        public UILayerController()
        {
            // === очистка для новой сцены
            EventBus<SceneServicesResetReusableEvent>.Register(new EventBinding<SceneServicesResetReusableEvent>(
                () => _elements.Clear()));
            
            // === собираем все элементы в сцене
            EventBus<SceneUIReadyEvent>.Register(new EventBinding<SceneUIReadyEvent>(() =>
            {
                var el = Object.FindObjectsByType<UILayerElement>(
                    FindObjectsInactive.Include, 
                    FindObjectsSortMode.None);
                
                var l = el.Length;
                for (int i = 0; i < l; i++)
                    _elements.Add(el[i]);
            }));
        }


        public void Register(UILayerElement element)
            => _elements.Add(element);

        public void Unregister(UILayerElement element)
            => _elements.Remove(element);
        

        public void Show(UILayerType layer)
            => SetActive(layer, true);

        public void Hide(UILayerType layer)
            => SetActive(layer, false);

        public void HideAllExcept(params UILayerType[] except)
        {
            var exceptions = new HashSet<UILayerType>(except);
            foreach (var e in _elements)
                e.gameObject.SetActive(exceptions.Contains(e.Layer));
        }

        private void SetActive(UILayerType layer, bool active)
        {
            foreach (var e in _elements)
                if (e.Layer == layer)
                    e.gameObject.SetActive(active);
        }
    }
}