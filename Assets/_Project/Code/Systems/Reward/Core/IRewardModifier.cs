
namespace Galactic1.Code.Game.Rewards.Modifiers
{
    /// <summary>
    /// Бизнес-правило, изменяющее итоговое количество награды.
    /// </summary>
    public interface IRewardModifier
    {
        int Order { get; } // порядок применения
        int Modify(RewardEntry reward, int currentAmount, RewardContext context);
    }
}