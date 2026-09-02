namespace Galactic1.Code.Game.Rewards
{
    /// <summary>
    /// Контекст, описывающий причину и условия выдачи награды.
    /// </summary>
    public struct RewardContext
    {
        public RewardSource Source;     // Ad, Quest, Chest, Purchase
        public bool IsVip;
        public string EventId;
        public float ServerMultiplier;

        public static RewardContext Default => new RewardContext
        {
            Source = RewardSource.Regular,
            IsVip = false,
            EventId = null,
            ServerMultiplier = 1f
        };
    }
}