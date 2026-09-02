using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Gameplay.Weapons
{
    public interface IAmmoSelector
    {
        /// Возвращает упорядоченный список стаков для расхода
        IReadOnlyList<(RuntimeId ammoId, int count)> Order(
            IReadOnlyList<(RuntimeId ammoId, int count)> available);
    }
}