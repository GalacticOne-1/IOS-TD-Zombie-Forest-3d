
using System;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Code.UI.Units;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Event-семантика: "юнит появился в лагере". Подписывается напрямую на
    /// GameLoopContext.OnUnitCreated (плоский C#-event, не EventBus<T>) — тот же приём,
    /// что DomainTransitionObjective использует для IGameLoopStateQuery.OnDomainTransition.
    ///
    /// KNOWN LIMITATION: OnUnitCreated поднимается из GameLoopContext.CreateUnitCompletely
    /// для ЛЮБОГО создания юнита — на момент написания единственный вызывающий это
    /// RecruitmentTavernRuntime.TryRecruit, поэтому "юнит создан" фактически совпадает с
    /// "юнит нанят". Если появится другой путь через CreateUnitCompletely (дебаг, квест
    /// и т.п.), этот объектив будет засчитываться и от него — категорию найма различить
    /// нельзя, т.к. UnitDisplayData её не несёт.</summary>
    public sealed class RecruitCompletedObjective : ITutorialObjective
    {
        private readonly GameLoopContext _context;
        private System.Action _onProgressChanged;
        public bool IsCompleted { get; private set; }

        public RecruitCompletedObjective(GameLoopContext context) => _context = context;

        public void Start(Action onProgressChanged)
        {
            _onProgressChanged = onProgressChanged;
            
            _context.OnUnitCreated += OnUnitCreated;
        }

        public void Stop() => _context.OnUnitCreated -= OnUnitCreated;

        private void OnUnitCreated(UnitDisplayData _)
        {
            if (IsCompleted) return;
            IsCompleted = true;
            _onProgressChanged?.Invoke();
        }

        public bool EvaluateCurrentState() => false;
        public bool EvaluateEvent(object gameplayEvent) => false;

        public bool TryGetProgress(out int current, out int required)
        {
            current = 0;
            required = 0;
            return false;
        }
    }
}