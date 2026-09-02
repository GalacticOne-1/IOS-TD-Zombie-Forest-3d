using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Gameplay.Effect
{
    public sealed class CooldownTracker
    {
        private readonly Dictionary<string, float> _timers = new();

        public Dictionary<string, float> Timers => _timers;

        public bool IsReady(string id)
            => !_timers.ContainsKey(id) || _timers[id] <= 0f;

        public void Set(string id, float duration)
        {
            _timers[id] = duration;
        }

        public void Tick(float dt)
        {
            var keys = new List<string>(_timers.Keys);

            foreach (var k in keys)
            {
                _timers[k] -= dt;
                if (_timers[k] <= 0f)
                    _timers.Remove(k);
            }
        }
    }
}