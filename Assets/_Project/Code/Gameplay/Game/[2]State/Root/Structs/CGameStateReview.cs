using System;

namespace Galactic1
{
    [Serializable]
    public struct CGameStateReview
    {
        public bool review, reviewRequest;
        public int reviewDelay;
    }
}