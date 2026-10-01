using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Analytics
{
    /// <summary>
    /// Монотонные "foreground" секунды: растут только пока приложение в фокусе.
    /// Не зависит от Time.time / timescale / GameTimeService. Нужны для duration_sec шагов туториала.
    /// </summary>
    public sealed class AppForegroundClock : IDisposable
    {
        private double _accumulated;
        private double _mark;
        private bool _focused;

        public AppForegroundClock()
        {
            _focused = Application.isFocused;
            _mark = Time.realtimeSinceStartupAsDouble;
            Application.focusChanged += OnFocusChanged;
        }

        public double Now => _focused
            ? _accumulated + (Time.realtimeSinceStartupAsDouble - _mark)
            : _accumulated;

        private void OnFocusChanged(bool focused)
        {
            double t = Time.realtimeSinceStartupAsDouble;
            if (_focused)
                _accumulated += t - _mark;

            _mark = t;
            _focused = focused;
        }

        public void Dispose() => Application.focusChanged -= OnFocusChanged;
    }
}
