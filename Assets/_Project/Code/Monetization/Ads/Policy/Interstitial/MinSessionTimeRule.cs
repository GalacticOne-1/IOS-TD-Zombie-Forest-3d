using Galactic1.Code.Core.Ads;

namespace Galactic1.Code.Systems.Ads
{
    /// <summary>
    /// Минимальное время с начала сессии
    /// </summary>
    public class MinSessionTimeRule : IInterstitialPlacementRule
    {
        private readonly float sessionStartTime;
        private readonly float minSeconds;

        public MinSessionTimeRule(float minSeconds)
        {
            sessionStartTime = UnityEngine.Time.time;
            this.minSeconds = minSeconds;
        }

        public bool CanShow(AdPlacement placement, out string reason)
        {
            reason = "";
            var sessionTime= UnityEngine.Time.time - sessionStartTime;

            if (sessionTime < minSeconds)
            {
                reason = $"Session time less {minSeconds} [{sessionTime}]";
                return false;
            }

            return true;
        }
    }
}