using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.UI.Interaction
{
    public sealed class UIBlockService : IGameService
    {
        private UIBlockRegistry _registry;

        private readonly HashSet<UIBlockGroup> _blockedGroups = new();

        public UIBlockService()
        {
            _registry = new UIBlockRegistry();
            
            // === собираем все кнопки в сцене
            EventBus<SceneUIReadyEvent>.Register(new EventBinding<SceneUIReadyEvent>(() =>
            {
                var buttons = Object.FindObjectsByType<UIBlockableButton>(
                    FindObjectsInactive.Include, 
                    FindObjectsSortMode.None);
                
                var l = buttons.Length;
                for (int i = 0; i < l; i++)
                    buttons[i].Register(_registry);
            }));
            
            
            // === переиспользование для новой сцены
            EventBus<SceneServicesResetReusableEvent>.Register(new EventBinding<SceneServicesResetReusableEvent>(
                () =>
                {
                    _registry = new UIBlockRegistry();
                    
                    EventBus<SceneUIReadyEvent>.Register(new EventBinding<SceneUIReadyEvent>(() =>
                    {
                        var buttons = Object.FindObjectsByType<UIBlockableButton>(
                            FindObjectsInactive.Include, 
                            FindObjectsSortMode.None);
                
                        var l = buttons.Length;
                        for (int i = 0; i < l; i++)
                            buttons[i].Register(_registry);
                    }));
                }));
        }

        public void Block(UIBlockGroup group)
        {
            if (_blockedGroups.Add(group))
                Apply();
        }

        public void Unblock(UIBlockGroup group)
        {
            if (_blockedGroups.Remove(group))
                Apply();
        }

        public void Clear()
        {
            _blockedGroups.Clear();
            Apply();
        }

        private void Apply()
        {
            foreach (var e in _registry.Elements)
            {
                bool blocked =
                    _blockedGroups.Contains(UIBlockGroup.Global) ||
                    _blockedGroups.Contains(e.Group);

                e.SetBlocked(blocked);
            }
        }
    }
}