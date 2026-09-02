using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.GameModes
{
    /// <summary>
    /// Scene scoped service responsible for switching game modes.
    /// Controls lifecycle of scene modes such as Normal, Construction and Raid.
    /// </summary>
    public class SceneGameModeService
    {
        private readonly Dictionary<GameModeType, ISceneGameMode> _modes = new();

        private ISceneGameMode _currentMode;

        public GameModeType CurrentMode => _currentMode?.ModeType ?? GameModeType.Normal;

        public event Action<GameModeType> ModeChanged;

        public void RegisterMode(ISceneGameMode mode)
        {
            _modes[mode.ModeType] = mode;
        }

        public void SetMode(GameModeType modeType)
        {
            if (_currentMode != null && _currentMode.ModeType == modeType)
                return;

            _currentMode?.Exit();

            if (!_modes.TryGetValue(modeType, out var newMode))
                throw new Exception($"GameMode not registered: {modeType}");

            _currentMode = newMode;

            _currentMode.Enter();

            ModeChanged?.Invoke(modeType);
        }
        
        public T Get<T>(GameModeType type) where T : class, ISceneGameMode
        {
            if (!_modes.TryGetValue(type, out var mode))
                return null;

            return mode as T;
        }
    }
}