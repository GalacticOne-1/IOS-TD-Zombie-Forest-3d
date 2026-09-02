
using System;
using R3;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units.Abstractions;

namespace Galactic1.Code.UI.Units.Presentation
{
    /// <summary>
    /// Адаптер для UI → читает статы из IUnitStatsRuntime
    /// </summary>
    public sealed class UnitStatsSceneAdapter : IUnitStatsScene
    {
        private IUnitStatsRuntime _statsRuntime;
        private readonly IReadOnlyDictionary<StatId, ReactiveProperty<float>> _stats;

        public bool IsDead => _statsRuntime.IsDead;
        public float MaxHP => _statsRuntime.MaxHP;

        public event Action OnDeath;
        
        

        public UnitStatsSceneAdapter(IUnitStatsRuntime statsRuntime)
        {
            _statsRuntime = statsRuntime;

            // 🔹 прокидываем все ReactiveProperty напрямую
            _stats = statsRuntime.CurrentStats_;

            _statsRuntime.OnDeath += HandleDeath;
        }

        /// <summary>
        /// Для UI — подписка или чтение значения статов
        /// </summary>
        public ReactiveProperty<float> Get(StatId type)
        {
            if (_stats.TryGetValue(type, out var prop))
                return prop;

            return new ReactiveProperty<float>(0); // fallback
        }

        public void ModifyStat(StatId type, float delta)
            => _statsRuntime.ModifyStat(type, delta);
        
        private void HandleDeath() => OnDeath?.Invoke();

        public void Dispose()
        {
            _statsRuntime.OnDeath -= HandleDeath;
        }
    }
}