using System;
using Galactic1.Code.Systems.Interaction;

namespace Galactic1.Code.Systems.GameTime
{
    /// <summary>
    /// Управляет паузой игры.
    /// Обёртка над GameTimeScaleService + блокировка input.
    /// </summary>
    public sealed class GamePauseService : IGameService
    {
        private readonly GameTimeScaleService _time;
        private readonly SceneInteractionBlocker _blocker;

        private bool _isPaused;

        public bool IsPaused => _isPaused;

        public event Action OnPaused;
        public event Action OnResumed;

        // уникальный ключ источника
        private readonly object _pauseSource = new();

        public GamePauseService(
            GameTimeScaleService time,
            SceneInteractionBlocker blocker)
        {
            _time = time;
            _blocker = blocker;
        }

        // =========================
        // API
        // =========================

        public void Pause()
        {
            if (_isPaused)
                return;

            _isPaused = true;

            // 1. стоп времени
            _time.Set(_pauseSource, 0f);

            // 2. блок gameplay input
            _blocker.Enable();

            OnPaused?.Invoke();
        }

        public void Resume()
        {
            if (!_isPaused)
                return;

            _isPaused = false;

            // 1. вернуть контроль времени
            _time.Remove(_pauseSource);

            // 2. разблок input
            _blocker.Disable();

            OnResumed?.Invoke();
        }

        public void Toggle()
        {
            if (_isPaused)
                Resume();
            else
                Pause();
        }
    }
}