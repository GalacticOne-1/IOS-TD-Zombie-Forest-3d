using System.Threading.Tasks;
using Galactic1.Code.Core.Ads;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Абстракция SDK рекламной сети.
    /// </summary>
    public interface IAdNetworkAdapter
    {
        Task LoadAsync(AdFormat format);
        bool IsReady(AdFormat format);
        Task<bool> ShowAsync(AdFormat format);
    }
}