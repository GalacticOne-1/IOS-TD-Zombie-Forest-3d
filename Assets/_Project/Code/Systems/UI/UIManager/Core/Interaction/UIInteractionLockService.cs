using UnityEngine;

namespace Galactic1.Code.UI.Interaction
{
    /// <summary>
    /// Глобальная блокировка UI и gameplay input
    /// </summary>
    public sealed class UIInteractionLockService : IUIInteractionLockService
    {
        private int _uiLockCount;
        private int _gameplayLockCount;

        public bool IsUIBlocked => _uiLockCount > 0;
        public bool IsGameplayBlocked => _gameplayLockCount > 0;

        public void LockUI() => _uiLockCount++;
        public void UnlockUI() => _uiLockCount = Mathf.Max(0, _uiLockCount - 1);

        public void LockGameplay() => _gameplayLockCount++;
        public void UnlockGameplay() => _gameplayLockCount = Mathf.Max(0, _gameplayLockCount - 1);
    }
}