namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Рантайм-условие одного guidance-варианта. Контрактно зеркалит ITutorialObjective
    /// (Start/Stop, та же событийная подписка через существующий EventBus), но НЕ имеет
    /// понятия "завершён" — IsSatisfied() это чистый запрос текущего состояния, вызываемый
    /// по требованию резолвером (TutorialGuidanceRuntimeState), а не единожды в ответ на
    /// событие.
    /// </summary>
    public interface ITutorialGuidanceCondition
    {
        /// <summary>onMightHaveChanged — сигнал "состояние могло измениться, перепроверь
        /// через IsSatisfied()", а НЕ "условие стало истинным". Источник истины всегда
        /// IsSatisfied(); событие — лишь триггер переоценки (см. TutorialGuidanceDefinition
        /// докстринг). Конкретная реализация сама решает, на какие события подписаться —
        /// резолвер никогда не подписывается на "все события".</summary>
        void Start(System.Action onMightHaveChanged);
        void Stop();
        bool IsSatisfied();
    }
}
