using Galactic1.Code.Core.Ads;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Правило показа interstitial рекламы.
    /// Возвращает true, если реклама разрешена для данного placement.
    /// </summary>
    public interface IInterstitialPlacementRule
    {
        bool CanShow(AdPlacement placement, out string reason);
    }
}