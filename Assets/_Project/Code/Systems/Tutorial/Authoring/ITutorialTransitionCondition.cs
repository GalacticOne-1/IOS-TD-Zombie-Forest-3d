namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Контракт условного перехода графа. Реализации — обычные C#-классы (не SO),
    /// сериализуются через [SerializeReference]. Для первого тутора граф линейный,
    /// реализаций пока нет — интерфейс объявлен заранее под будущее ветвление.
    /// </summary>
    public interface ITutorialTransitionCondition
    {
        bool Evaluate();
    }
}
