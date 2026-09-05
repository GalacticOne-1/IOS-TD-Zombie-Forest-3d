namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Рантайм-логика одного объектива. Создаётся TutorialObjectiveFactory при
    /// активации шага, уничтожается при деактивации.
    /// </summary>
    public interface ITutorialObjective
    {
        /// <summary>onProgressChanged вызывается каждый раз, когда IsCompleted мог
        /// измениться — включая немедленный вызов из Start(), если объектив уже
        /// удовлетворён на момент старта (ретроактивное завершение).</summary>
        void Start(System.Action onProgressChanged);
        void Stop();
        bool EvaluateCurrentState();
        bool EvaluateEvent(object gameplayEvent);
        bool IsCompleted { get; }
    }
}
