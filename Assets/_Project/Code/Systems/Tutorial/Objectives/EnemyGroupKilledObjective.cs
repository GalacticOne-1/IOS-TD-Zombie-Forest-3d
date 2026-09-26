using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Presentation;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public sealed class EnemyGroupKilledObjective : TutorialEventObjectiveBase<EnemyKilledEvent>
    {
        private readonly TutorialEnemyGroupSelectionService _selection;
        private readonly TutorialTargetId _originTargetId;
        private readonly int _count;

        public EnemyGroupKilledObjective(
            TutorialEnemyGroupSelectionService selection,
            TutorialTargetId originTargetId,
            int count)
        {
            _selection = selection;
            _originTargetId = originTargetId;
            _count = count;
        }

        /// <summary>Start() базового класса вызывает это первым — единственный доступный
        /// хук, чтобы зафиксировать selection до подписки на EnemyKilledEvent. Возвращает
        /// true (ретроактивное завершение) ТОЛЬКО в вырожденном случае: origin уже
        /// зарегистрирован и рядом с ним не нашлось вообще ни одного врага.</summary>
        public override bool EvaluateCurrentState()
        {
            _selection.Begin(_originTargetId, _count);
            return _selection.IsResolvedEmpty(_originTargetId);
        }

        protected override bool EvaluateEvent(EnemyKilledEvent e)
        {
            _selection.Remove(_originTargetId, e.Runtime.Id);
            return _selection.AliveCount(_originTargetId) == 0;
        }

        public override bool TryGetProgress(out int current, out int required)
        {
            required = _selection.ResolvedTotal(_originTargetId);
            current = required - _selection.AliveCount(_originTargetId);
            return true;
        }
    }
}