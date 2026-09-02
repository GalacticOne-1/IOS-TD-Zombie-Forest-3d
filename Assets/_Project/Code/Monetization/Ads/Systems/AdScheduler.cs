using System.Threading.Tasks;
using Galactic1.Code.Core.Ads;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Отвечает за загрузку рекламы и приоритеты.
    /// </summary>
    public class AdScheduler
    {
        private readonly IAdNetworkAdapter adapter;

        public AdScheduler(IAdNetworkAdapter adapter)
        {
            this.adapter = adapter;
        }

        public Task Preload(AdFormat format)
            => adapter.LoadAsync(format);
    }
}