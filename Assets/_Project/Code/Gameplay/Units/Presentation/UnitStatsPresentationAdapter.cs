using System;
using Galactic1.Code.Gameplay.Units.Stats;
using R3;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.UI.Inventory;

namespace Galactic1.Code.UI.Units.Presentation
{
    /// <summary>
    /// Адаптер для UI → читает статы из IUnitStatsRuntime
    /// </summary>
    public sealed class UnitStatsPresentationAdapter : IReadOnlyStatsView
    {
        private IUnitStatsRuntime _runtime;
        private readonly IReadOnlyDictionary<StatId, ReactiveProperty<float>> _stats;
        
        public float MaxHP { get; }
        public event Action<StatChangedEvent, bool> OnStatChanged; // для обновления статов выбранного юнита
        
        

        public UnitStatsPresentationAdapter(IUnitStatsRuntime runtime)
        {
            _runtime = runtime;
            
            // 🔹 snapshot MaxHP (редко меняется)
            MaxHP = runtime.MaxHP;


            // 🔹 прокидываем все ReactiveProperty напрямую
            _stats = runtime.CurrentStats_;

            runtime.OnStatChanged += HandleRuntimeStatChanged;
        }

        private void HandleRuntimeStatChanged(StatChangedEvent e, bool pushStart)
        {
            var unitId = ServiceLocator.Current.Get<InventoryManagementWindow>().modeController.SelectedUnit.unitId;
            if (_runtime.Owner == unitId)
                OnStatChanged?.Invoke(e, pushStart);
        }

        /// <summary>
        /// Для UI — подписка или чтение значения статов
        /// </summary>
        public ReactiveProperty<float> Get(StatId statId)
        {
            if (_stats.TryGetValue(statId, out var prop))
                return prop;

            return new ReactiveProperty<float>(0); // fallback
        }

        public void PushAllStats() => _runtime.PushAllStats();
        
        public void Dispose()
        {
            _runtime.OnStatChanged -= HandleRuntimeStatChanged;
        }
    }
}