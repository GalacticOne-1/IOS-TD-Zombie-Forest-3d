using System.Collections.Generic;

namespace Galactic1.Game.Meta.Items
{
    
    /// <summary>
    /// Это универсальный слой для:
    /// юнита
    /// оружия
    /// брони
    /// модулей
    /// </summary>
    public class StatContainer
    {
        private readonly Dictionary<StatId, float> baseValues = new();
        private readonly List<StatModifier> modifiers = new();

        private readonly Dictionary<StatId, float> cache = new();

        public void SetBase(StatId stat, float value)
            => baseValues[stat] = value;

        public void AddModifier(StatModifier mod)
            => modifiers.Add(mod);

        public float Get(StatId stat)
        {
            if (cache.TryGetValue(stat, out var v))
                return v;

            float value = baseValues.TryGetValue(stat, out var baseValue)
                ? baseValue
                : 0f;

            float percent = 0f;
            float multiplier = 1f;

            foreach (var mod in modifiers)
            {
                if (mod.StatId != stat) continue;

                switch (mod.Operation)
                {
                    case ModifierOperation.Flat:
                        value += mod.Value;
                        break;

                    case ModifierOperation.Percent:
                        percent += mod.Value;
                        break;

                    case ModifierOperation.Multiplier:
                        multiplier *= mod.Value;
                        break;
                }
            }

            value *= 1f + percent;
            value *= multiplier;

            cache[stat] = value;

            return value;
        }

        public void Clear()
        {
            modifiers.Clear();
            cache.Clear();
        }
    }
}