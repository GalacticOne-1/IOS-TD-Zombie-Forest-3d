
using System.Collections.Generic;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Game.Runtime.Stats
{
    public static class StatAggregator
    {
        public static float Calculate(
            float baseValue,
            List<StatModifier> modifiers)
        {
            float value = baseValue;

            foreach (var mod in modifiers)
            {
                switch (mod.Operation)
                {
                    case ModifierOperation.Flat:
                        value += mod.Value;
                        break;

                    case ModifierOperation.Multiplier:
                        value *= mod.Value;
                        break;

                    case ModifierOperation.Override:
                        value = mod.Value;
                        break;
                }
            }

            return value;
        }
    }
}