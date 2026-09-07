using System.Linq;
using ObservableCollections;
using R3;

namespace Galactic1.Code.Core
{
    /// <summary>
    /// Sync layer between ProgressionData (persisted) and runtime code.
    /// Same shape as every other proxy in the project: ReactiveProperty for
    /// scalars, ObservableList for collections, Skip(1).Subscribe to write
    /// changes back into Origin.
    /// </summary>
    public class ProgressionProxy
    {
        public readonly ProgressionData Origin;

        public readonly ReactiveProperty<int> Level;
        public readonly ReactiveProperty<int> CurrentXP;

        public ObservableList<string> UnlockedIds { get; } = new();
        public ObservableList<int> PendingSkillChoiceLevels { get; } = new();

        public ProgressionProxy(ProgressionData data)
        {
            Origin = data;

            Level = new(Origin.Level);
            Level.Skip(1).Subscribe(v => Origin.Level = v);

            CurrentXP = new(Origin.CurrentXP);
            CurrentXP.Skip(1).Subscribe(v => Origin.CurrentXP = v);

            foreach (var id in Origin.UnlockedIds)
                UnlockedIds.Add(id);

            UnlockedIds.ObserveAdd().Subscribe(e => Origin.UnlockedIds.Add(e.Value));
            UnlockedIds.ObserveRemove().Subscribe(e => Origin.UnlockedIds.Remove(e.Value));

            foreach (var level in Origin.PendingSkillChoiceLevels)
                PendingSkillChoiceLevels.Add(level);

            PendingSkillChoiceLevels.ObserveAdd().Subscribe(e => Origin.PendingSkillChoiceLevels.Add(e.Value));
            PendingSkillChoiceLevels.ObserveRemove().Subscribe(e => Origin.PendingSkillChoiceLevels.Remove(e.Value));
        }
    }
}
