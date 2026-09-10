using System;

namespace Galactic1.Code.Gameplay.Tasks
{
    /// <summary>Стабильный идентификатор задачи в рамках текущей сессии. Источник задачи
    /// сам решает, что использовать как значение — Tutorial использует stepId.Guid
    /// (см. TutorialTaskPresenter). Ничего не знает про то, откуда взялось значение.</summary>
    public readonly struct ScenarioTaskId : IEquatable<ScenarioTaskId>
    {
        public readonly string Value;

        public ScenarioTaskId(string value) => Value = value;

        public bool Equals(ScenarioTaskId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ScenarioTaskId other && Equals(other);
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        public override string ToString() => Value;
    }
}