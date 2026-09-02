namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Асинхронная state machine формата рекламы.
    /// </summary>
    public enum AdRuntimeState
    {
        Idle,
        Loading,
        Ready,
        Showing,
        Cooldown
    }
}