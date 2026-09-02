
using System.Collections.Generic;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Runtime;

namespace Galactic1.RaidLoot.Services
{
    /// <summary>
    /// Управляет кривой истощения контейнеров.
    ///
    /// Первое открытие  → Full    (100% budget)
    /// Второе открытие  → Reduced (50% budget, max T2)
    /// Третье открытие  → Scarce  (20% budget, max T1)
    /// Четвёртое+       → Empty   (0% budget)
    ///
    /// Scope: текущий рейд. Для cross-raid depletion — сохранять Records в SaveData.
    /// </summary>
    public sealed class ContainerDepletionService
    {
        private readonly Dictionary<string, ContainerDepletionRecord> _records = new();
        private readonly DepletionCurveConfig _curve;
        private readonly int _currentDay;

        public ContainerDepletionService(
            DepletionCurveConfig curve,
            int currentDay)
        {
            _curve = curve;
            _currentDay = currentDay;
        }

        /// <summary>
        /// Возвращает текущую стадию истощения.
        /// Вызывать ДО RegisterOpen — чтобы получить стадию для текущего открытия.
        /// </summary>
        public DepletionCurveConfig.DepletionStageRule GetCurrentStage(string id)
        {
            var openCount = _records.TryGetValue(id, out var rec) ? rec.OpenCount : 0;
            return _curve.GetStage(openCount);
        }

        /// <summary>Зарегистрировать факт открытия. Вызывать ПОСЛЕ получения стадии.</summary>
        public void RegisterOpen(string id)
        {
            if (!_records.TryGetValue(id, out var rec))
            {
                rec = new ContainerDepletionRecord(id, _currentDay);
                _records[id] = rec;
            }

            rec.RegisterOpen();
        }
        
        public int GetOpenCount(string id)
            => _records.TryGetValue(id, out var rec) ? rec.OpenCount : 0;

        public bool IsEmpty(string id)
        {
            var stage = GetCurrentStage(id);
            return stage.BudgetMultiplier <= 0f;
        }

        public void Clear() => _records.Clear();
    }
}