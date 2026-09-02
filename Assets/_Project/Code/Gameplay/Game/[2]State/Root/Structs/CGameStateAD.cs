using System;
using UnityEngine.Serialization;

namespace Galactic1
{
    [Serializable]
    public struct CGameStateAD
    {
        public bool ShowAutoAds;        // показ авто рекламы
        public int RemainingLimit;      // сколько рекламы осталось для дня
    }
}