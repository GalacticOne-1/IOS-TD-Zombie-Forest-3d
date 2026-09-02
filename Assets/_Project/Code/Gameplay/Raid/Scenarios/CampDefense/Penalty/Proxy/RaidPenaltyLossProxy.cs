using R3;

namespace Galactic1.Code.Core
{
    /// <summary>
    /// Reactive-обёртка над RaidPenaltyLossData.
    /// Аналог RaidRewardLootProxy — тот же паттерн биндинга на сохранение.
    /// </summary>
    public class RaidPenaltyLossProxy
    {
        public readonly RaidPenaltyLossData Origin;

        public int Id => Origin.Id;
        public readonly ReactiveProperty<string> ConfigId;
        public readonly ReactiveProperty<int> Amount;

        public RaidPenaltyLossProxy(RaidPenaltyLossData data)
        {
            Origin = data;

            ConfigId = new(Origin.ConfigId);
            Amount = new(Origin.Amount);

            BindToSave(data);
        }

        public void BindToSave(RaidPenaltyLossData origin)
        {
            ConfigId.Skip(1).Subscribe(_ => origin.ConfigId = _);
            Amount.Skip(1).Subscribe(_ => origin.Amount = _);
        }
    }
}