using System;

namespace Galactic1.Code.Gameplay.Effect
{
    /// <summary>
    /// Эффект каста (блокирующий).
    /// </summary>
    public sealed class CastEffect : IActiveEffect
    {
        private readonly float _castTime;
        private readonly Action _onComplete;
        private readonly Action _onCanceled;

        private float _elapsed;
        private bool _done;

        public bool IsFinished => _done;
        public bool IsCasting => !_done;

        /// <summary> Используется для блокировки действий </summary>
        public bool BlocksActions => !_done;

        public CastEffect(float castTime, Action onComplete, Action onCanceled = null)
        {
            _castTime = castTime;
            _onComplete = onComplete;
            _onCanceled = onCanceled;
        }

        public void Tick(float dt)
        {
            if (_done) return;

            _elapsed += dt;

            if (_elapsed >= _castTime)
            {
                _done = true;
                _onComplete?.Invoke();
            }
        }

        public void Cancel()
        {
            if (_done) return;

            _done = true;
            _onCanceled?.Invoke();
        }
    }
}