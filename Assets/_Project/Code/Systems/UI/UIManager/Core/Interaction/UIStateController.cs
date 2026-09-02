using System.Collections.Generic;

namespace Galactic1.Code.UI.Interaction
{
    /// <summary>
    /// Управляет активным UI состоянием по приоритету
    /// </summary>
    public sealed class UIStateController
    {
        private readonly List<IUIState> _states = new();

        private readonly IUILayerService _layers;
        private readonly IUIInteractionLockService _lock;

        private IUIState _current;

        public UIStateController(
            IUILayerService layers,
            IUIInteractionLockService lockService)
        {
            _layers = layers;
            _lock = lockService;
        }

        public void Push(IUIState state)
        {
            _states.Add(state);
            Recalculate();
        }

        public void Remove<T>() where T : IUIState
        {
            _states.RemoveAll(s => s is T);
            Recalculate();
        }
        
        public void RemoveAll() => _states.Clear();

        private void Recalculate()
        {
            IUIState next = null;

            foreach (var s in _states)
            {
                if (next == null || s.Priority > next.Priority)
                    next = s;
            }

            if (_current == next)
                return;

            _current?.OnExit();

            // 🔥 снимаем все блокировки
            ResetLocks();

            _current = next;

            _current?.OnEnter();

            if (_current != null)
            {
                ApplyLocks(_current);
                _current.Apply(_layers, _lock);
            }
        }

        private void ApplyLocks(IUIState state)
        {
            if (state.BlocksUIInput)
                _lock.LockUI();

            if (state.BlocksGameplayInput)
                _lock.LockGameplay();
        }

        private void ResetLocks()
        {
            while (_lock.IsUIBlocked)
                _lock.UnlockUI();

            while (_lock.IsGameplayBlocked)
                _lock.UnlockGameplay();
        }
    }
}