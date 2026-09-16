using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Подписывается напрямую на GameLoopContext.OnBuildingCreated — тот же приём, что
    /// RecruitCompletedObjective использует для OnUnitCreated (плоский C#-event, не
    /// EventBus&lt;T&gt;, поэтому не наследуется от TutorialEventObjectiveBase). Не
    /// ретроактивен: EvaluateCurrentState() == false, здание, построенное до активации
    /// шага, не засчитывается — тот же принцип, что у ItemCollectedObjective/
    /// InboxItemCollectedObjective ("сделал за время шага", не "уже имеет").
    /// </summary>
    public sealed class FacilityBuiltObjective : ITutorialObjective
    {
        private readonly GameLoopContext _context;
        private readonly ItemId _itemId; // null = любое здание

        private Action _onProgressChanged;
        public bool IsCompleted { get; private set; }

        public FacilityBuiltObjective(GameLoopContext context, ItemId itemId)
        {
            _context = context;
            _itemId = itemId;
        }

        public void Start(Action onProgressChanged)
        {
            _onProgressChanged = onProgressChanged;
            _context.OnBuildingCreated += OnBuildingCreated;
        }

        public void Stop() => _context.OnBuildingCreated -= OnBuildingCreated;

        private void OnBuildingCreated(BaseCampFacilityRuntime runtime)
        {
            if (IsCompleted) return;
            if (_itemId != null && runtime.Config.Item.Id != _itemId) return;

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
