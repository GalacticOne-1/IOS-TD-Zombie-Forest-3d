using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Units.Stats;
using R3;

namespace Galactic1.Code.Gameplay.Units.Abstractions
{
    /// <summary>
    /// Унифицированный runtime-интерфейс статов юнита.
    /// Работает для Meta и Raid.
    /// </summary>
    public interface IUnitStatsRuntime
    {
        IReadOnlyDictionary<StatId, float> GetBaseStats { get; }
        /// <summary>
        /// Текущие значения статов (runtime).
        /// Используется UI и боевой логикой.
        /// </summary>
        IReadOnlyDictionary<StatId, ReactiveProperty<float>> CurrentStats_ { get; }

        /// <summary>
        /// Максимальное значение стата (после экипировки / бафов).
        /// </summary>
        float GetMax(StatId stat);

        /// <summary>
        /// Текущее значение стата.
        /// </summary>
        float GetCurrent(StatId stat);

        /// <summary>
        /// Изменить runtime-значение стата (урон / хил / расход).
        /// </summary>
        void ModifyStat(StatId stat, float delta);
        /// <summary>
        /// Установить runtime-значение стата (урон / хил / расход).
        /// </summary>
        void SetStat(StatId stat, float amount);

        /// <summary>
        /// Быстрый доступ к HP.
        /// </summary>
        float CurrentHP { get; }

        /// <summary>
        /// Максимальное HP.
        /// </summary>
        float MaxHP { get; }

        /// <summary>
        /// Мёртв ли юнит.
        /// </summary>
        bool IsDead { get; }
        
        string Owner { get; }

        /// <summary>
        /// Событие изменения любого стата.
        /// </summary>
        event Action<StatChangedEvent, bool> OnStatChanged;
        event Action OnDeath;
        
        
        void AddBuff(Buff buff);
        void RemoveBuff(BuffId buffId);
        bool HasBuff(BuffId buffId);

        /// <summary>
        /// обратный переход (revive-safe)
        /// </summary>
        /// <param name="hp"></param>
        void Revive(float hp);

        void PushAllStats();
    }
}