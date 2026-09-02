using System;
using System.Collections.Generic;

namespace Galactic1
{
    [Serializable]
    public struct CGameStateIAP
    {
        public bool vip_pack_paid;                              // true - куплен вип пакет (за реал)
        public List<bool> double_hard;                          // первая покупка кристаллов x2
        public bool[] startPack;                                // купленные пакеты
    }
    
}