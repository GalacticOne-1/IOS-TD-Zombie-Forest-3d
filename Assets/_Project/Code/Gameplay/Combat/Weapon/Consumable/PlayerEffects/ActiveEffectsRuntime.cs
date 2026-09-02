using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.Effect
{
    /// <summary>
    /// Runtime контейнер эффектов.
    /// </summary>
    public sealed class ActiveEffectsRuntime
    {
        private readonly List<IActiveEffect> _effects = new(8);

        public void Add(IActiveEffect effect)
        {
            _effects.Add(effect);
        }

        public void Tick(float dt)
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var e = _effects[i];
                e.Tick(dt);

                if (e.IsFinished)
                    _effects.RemoveAt(i);
            }
        }

        public bool HasAny<T>() where T : IActiveEffect
        {
            foreach (var e in _effects)
                if (e is T)
                    return true;

            return false;
        }

        /// <summary>
        /// Используется при получении урона.
        /// </summary>
        public void CancelAll<T>() where T : IActiveEffect
        {
            foreach (var e in _effects)
            {
                if (e is CastEffect cast)
                    cast.Cancel();
            }
        }
    }
}