using System.Collections.Generic;

namespace Galactic1.Code.Systems.GameTime
{
    /// <summary>
    /// Централизованное управление Time.timeScale (stack-based).
    /// Поддерживает несколько источников (ability, pause, debug).
    /// </summary>
    public sealed class GameTimeScaleService: IGameService
    {
        private readonly Dictionary<object, float> _sources = new();

        private float _current = 1f;

        public float Current => _current;

        // =========================
        // API
        // =========================

        /// <summary>
        /// Установить scale от конкретного источника
        /// </summary>
        public void Set(object source, float scale)
        {
            if (scale <= 0f)
                scale = 0.01f;

            _sources[source] = scale;
            Recalculate();
        }

        /// <summary>
        /// Убрать влияние источника
        /// </summary>
        public void Remove(object source)
        {
            if (_sources.Remove(source))
                Recalculate();
        }

        /// <summary>
        /// Очистить всё (например при reset сцены)
        /// </summary>
        public void Clear()
        {
            _sources.Clear();
            Apply(1f);
        }

        // =========================
        // Internal
        // =========================

        private void Recalculate()
        {
            float result = 1f;

            // 🔥 правило: берём минимальный scale (самый "медленный")
            foreach (var kv in _sources)
            {
                if (kv.Value < result)
                    result = kv.Value;
            }

            Apply(result);
        }

        private void Apply(float scale)
        {
            _current = scale;
            UnityEngine.Time.timeScale = scale;
        }
        
    }
}