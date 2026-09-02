
using Galactic1.Code.Core.Ads;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Показывать interstitial только после рейда
    /// </summary>
    public class PostRaidRule : IInterstitialPlacementRule
    {
        // private readonly IRaidService raidService;
        //
        // public PostRaidRule(IRaidService raidService)
        // {
        //     this.raidService = raidService;
        // }

        public bool CanShow(AdPlacement placement, out string reason)
        {
            reason = "";
            // if (placement != AdPlacement.PostRaid)
            //     return true;
            //
            // // пример: показываем только после успешного рейда
            // return raidService.LastRaidResult == RaidResult.Win;
            return true;
        }
    }
}