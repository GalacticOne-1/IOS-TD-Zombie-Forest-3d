using System;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Runtime боевого сооружения, которое может получать урон.
    ///
    /// Используется сценой и combat pipeline.
    /// Реализуется как CombatFacilityRuntime, так и RaidCombatFacilityRuntime.
    /// </summary>
    public interface IRaidFacilityRuntime : IFacilityRuntime, IUnitRuntimeBase
    {
        /// <summary>
        /// Боевые статы сооружения.
        /// </summary>
        IUnitStatsRuntime Stats { get; }

        /// <summary>
        /// Модуль здоровья (если нужен другим системам).
        /// </summary>
        BuildingHealthModule HealthModule { get; }

        /// <summary>
        /// Текущее здоровье.
        /// </summary>
        float CurrentHP { get; }

        /// <summary>
        /// Максимальное здоровье.
        /// </summary>
        float MaxHP { get; }

        /// <summary>
        /// Здание уничтожено.
        /// </summary>
        bool IsDestroyed { get; }


        /// <summary>
        /// Изменилось здоровье.
        /// </summary>
        event Action<float, float> OnHealthChanged;

        /// <summary>
        /// Здание уничтожено.
        /// </summary>
        event Action OnDestroyed;
    }
}