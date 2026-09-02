using System.Threading.Tasks;
using Galactic1.Code.Core.Ads;
using Galactic1.Configs;

namespace Galactic1.Code.Systems.Ads.AdMob
{
    /// <summary>
    /// Реализация IAdNetworkAdapter для AdMob.
    /// </summary>
    public class AdMobAdapter : IAdNetworkAdapter
    {
        private readonly AdMobRewardedHandler rewarded;
        private readonly AdMobInterstitialHandler interstitial;

        public AdMobAdapter()
        {
            if (ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().adTestMode)
            {
                rewarded = new AdMobRewardedHandler("ca-app-pub-3940256099942544/5662855259");
                interstitial = new AdMobInterstitialHandler("ca-app-pub-3940256099942544/4411468910");
            }
            else
            {
                // todo ...
            }
        }

        public Task LoadAsync(AdFormat format)
        {
            return format switch
            {
                AdFormat.Rewarded => rewarded.LoadAsync(),
                AdFormat.Interstitial => interstitial.LoadAsync(),
                _ => Task.CompletedTask
            };
        }

        public bool IsReady(AdFormat format)
        {
            return format switch
            {
                AdFormat.Rewarded     => rewarded.IsReady(),
                AdFormat.Interstitial => interstitial.IsReady(),
                _ => false
            };
        }

        public Task<bool> ShowAsync(AdFormat format)
        {
            return format switch
            {
                AdFormat.Rewarded     => rewarded.ShowAsync(),
                AdFormat.Interstitial => interstitial.ShowAsync(),
                _ => Task.FromResult(false)
            };
        }
    }
}