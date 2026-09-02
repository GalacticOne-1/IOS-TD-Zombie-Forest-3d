using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Gameplay.Weapons
{
    public sealed class WorstFirstAmmoSelector : IAmmoSelector
    {
        public IReadOnlyList<(RuntimeId ammoId, int count)> Order(IReadOnlyList<(RuntimeId ammoId, int count)> available)
            => available as List<(RuntimeId, int)> ?? new List<(RuntimeId, int)>(available);
        // порядок уже задаётся реестром/приоритетом в ItemConfig
    }
}