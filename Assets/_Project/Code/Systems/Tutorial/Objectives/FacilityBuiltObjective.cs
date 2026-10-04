using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.Systems.Tutorial.Runtime;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// "Построй здание". Подписывается напрямую на GameLoopContext.OnBuildingCreated — тот же
    /// приём, что RecruitCompletedObjective использует для OnUnitCreated (плоский C#-event,
    /// не EventBus&lt;T&gt;, поэтому не наследуется от TutorialEventObjectiveBase).
    ///
    /// Семантика зависит от itemId:
    ///  • itemId задан — STATE-семантика: большинство зданий существуют в одном экземпляре,
    ///    поэтому если такое здание УЖЕ построено к старту шага, объектив завершается сразу
    ///    (ретроактивно, иначе шаг навсегда зависнет — второй раз его не построить).
    ///    Если не построено — ждём OnBuildingCreated.
    ///  • itemId пуст ("любое здание") — EVENT-семантика: ретроактивность бессмысленна
    ///    (в лагере всегда есть хотя бы главное здание), засчитывается только постройка
    ///    за время шага.
    ///
    /// Здания, восстановленные из сейва (FacilityRuntimeService.Initialize → AddFacility),
    /// OnBuildingCreated не поднимают — для них работает только стартовая проверка.
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

            // Как в TutorialEventObjectiveBase: уже выполнено на старте — не подписываемся.
            if (EvaluateCurrentState())
            {
                IsCompleted = true;
                _onProgressChanged?.Invoke();
                return;
            }

            _context.OnBuildingCreated += OnBuildingCreated;
        }

        public void Stop()
        {
            _context.OnBuildingCreated -= OnBuildingCreated;
            _onProgressChanged = null;
        }

        private void OnBuildingCreated(BaseCampFacilityRuntime runtime)
        {
            if (IsCompleted) return;
            if (_itemId != null && runtime.Config.Item.Id != _itemId) return;

            IsCompleted = true;
            _onProgressChanged?.Invoke();
        }

        /// <summary>Ретроактивно только для конкретного itemId: такое здание уже есть в лагере.</summary>
        public bool EvaluateCurrentState()
            => _itemId != null && _context.GetFacilityByConfigId(_itemId) != null;

        public bool EvaluateEvent(object gameplayEvent) => false;

        public bool TryGetProgress(out int current, out int required)
        {
            current = 0;
            required = 0;
            return false;
        }
    }
}