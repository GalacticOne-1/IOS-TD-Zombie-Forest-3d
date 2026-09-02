using System;
using System.Collections.Generic;

namespace Galactic1.Code.Systems.Runtime
{
    public interface IWeightedRandomService : IGameService
    {
        T PickWeighted<T>(
            IReadOnlyList<T> items,
            Func<T, int> weightSelector);

        int Range(int minInclusive, int maxExclusive);

        float Value();
        float Value01();
    }
}