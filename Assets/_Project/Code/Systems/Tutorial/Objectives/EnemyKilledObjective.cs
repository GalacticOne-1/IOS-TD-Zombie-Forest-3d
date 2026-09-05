namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class EnemyKilledObjective : TutorialEventObjectiveBase<EnemyKilledEvent>
    {
        private readonly int _requiredCount;
        private int _current;

        public EnemyKilledObjective(int requiredCount) => _requiredCount = requiredCount;

        // Убийство "до старта" объектива не подсчитываем — счётчик обнуляется с
        // активацией шага (EvaluateCurrentState = false по умолчанию из базового класса).

        protected override bool EvaluateEvent(EnemyKilledEvent e)
        {
            _current++;
            return _current >= _requiredCount;
        }
    }
}
