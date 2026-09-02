using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Gameplay.Enemies.Waves
{
    /// <summary>Группа однотипных врагов в волне.</summary>
    [System.Serializable]
    public sealed class WaveEntry
    {
        public EnemyId EnemyId;
        public int Count = 1;

        /// <summary>Опциональный явный вариант. Пустой = случайный.</summary>
        public string VariantId;

        /// <summary>Модификаторы для этой группы ("armored", "elite", ...).</summary>
        public List<string> ModifierIds = new();
    }
}