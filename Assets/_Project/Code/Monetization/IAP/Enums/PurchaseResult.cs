namespace Galactic1.Systems.Purchase
{
    /// <summary>
    /// Результат операции покупки
    /// </summary>
    public enum PurchaseResult
    {
        Success,
        Failed,
        NotInitialized,
        Cancelled,
        AlreadyOwned,
        Timeout
    }
}