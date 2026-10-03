using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.RaidLoot.Events;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// Счётчик контейнеров с опциональным фильтром по локации. Локация берётся в момент
    /// события через currentLocation (в рейде — GameLoopContext.CurrentRaid.Id).
    /// Прогресс сохраняется через IPersistentTutorialObjective.
    /// </summary>
    public abstract class ContainerCountObjectiveBase<TEvent>
        : TutorialEventObjectiveBase<TEvent>, IPersistentTutorialObjective
        where TEvent : IEvent
    {
        private readonly int _required;
        private readonly LocationId _locationFilter;      // null = любая локация
        private readonly Func<LocationId> _currentLocation;
        private int _current;

        protected ContainerCountObjectiveBase(
            int required, 
            LocationId locationFilter,
            Func<LocationId> currentLocation)
        {
            _required = Mathf.Max(1, required);
            _locationFilter = locationFilter;
            _currentLocation = currentLocation;
        }

        // Ретроактивность только для восстановленного прогресса (Start() вызывает до подписки).
        public override bool EvaluateCurrentState() => _current >= _required;

        protected override bool EvaluateEvent(TEvent e)
        {
            if (_locationFilter != null && _currentLocation?.Invoke() != _locationFilter)
                return false;

            _current++;
            return _current >= _required;
        }

        public override bool TryGetProgress(out int current, out int required)
        {
            current = Mathf.Min(_current, _required);
            required = _required;
            return true;
        }

        public int ProgressValue => _current;
        public void RestoreProgress(int value) => _current = Mathf.Clamp(value, 0, _required);
    }

    /// <summary>Считает открытые контейнеры (ContainerOpenedEvent, момент Closed → Opening).</summary>
    public sealed class ContainersOpenedCountObjective : ContainerCountObjectiveBase<ContainerOpenedEvent>
    {
        public ContainersOpenedCountObjective(int required, LocationId filter, Func<LocationId> loc)
            : base(required, filter, loc) { }
    }

    /// <summary>Считает залутанные контейнеры (ContainerLootCollectedEvent; пустые не считаются).</summary>
    public sealed class ContainersLootedCountObjective : ContainerCountObjectiveBase<ContainerLootCollectedEvent>
    {
        public ContainersLootedCountObjective(int required, LocationId filter, Func<LocationId> loc)
            : base(required, filter, loc) { }
    }
}