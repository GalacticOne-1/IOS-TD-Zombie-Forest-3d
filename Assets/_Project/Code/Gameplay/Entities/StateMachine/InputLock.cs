namespace Galactic1.Gameplay.Player.StateMachine
{
    /// <summary>
    /// Простой счётчик блокировок ввода.
    /// Когда >0 — ввод заблокирован.
    /// </summary>
    public class InputLock
    {
        private int _count = 0;
        public bool IsLocked => _count > 0;

        /// <summary>Увеличить счетчик блокировок (заблокировать ввод)</summary>
        public void Acquire() => _count++;

        /// <summary>Уменьшить счетчик (освободить ввод)</summary>
        public void Release()
        {
            _count--;
            if (_count < 0) _count = 0;
        }

        public void Reset() => _count = 0;
    }
}