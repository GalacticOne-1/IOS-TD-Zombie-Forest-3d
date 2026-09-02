namespace Galactic1.Code.Core.Ads
{
    /// <summary>
    /// Результат проверки возможности показа рекламы.
    /// Используется UI и геймплеем.
    /// </summary>
    public struct AdDecision
    {
        public bool Allowed;
        public string Reason;
        public float CooldownRemaining;

        public static AdDecision Allow() => new AdDecision { Allowed = true };
        public static AdDecision Deny(string reason, float cd = 0) =>
            new AdDecision { Allowed = false, Reason = reason, CooldownRemaining = cd };
    }
}