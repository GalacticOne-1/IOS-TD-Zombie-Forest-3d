using System;
using UnityEngine;

namespace Galactic1
{
    
    
    [Serializable]
    public class CSaveTask
    {
        public byte status;
        public short id;
        public int[] progress;
    }


    [Serializable]
    public class CSaveADBox
    {
        public byte number_launch;                      // кол-во запусков для одного цикла (7 times)
        public byte[] received_reward;                  // полученные предметы в рамках одного цикла (7 times)
    }
    
    [Serializable]
    public class CSaveCampBonus
    {
        public bool claim;
        public byte adAmount;
    }


    [Serializable]
    public class CWorkstationFuel
    {
        public int duration;                       // продолжительность работы топлива
        public int assetId;
    }


    [Serializable]
    public class CSaveCrate
    {
        public bool use;
        public CPlayerInventory[] slot;
    }
    

    // --- EQUIPMENT
    [Serializable]
    public class CEquipmentList
    {
        public CEquipmentType[] type;
    }
    [Serializable]
    public class CEquipmentType
    {
        public CEquipment[] equip;
    }
    [Serializable]
    public class CEquipment
    {
        public bool unlocked;
        public short level;
        public short score;
    }
    //
    
    
    
    // --- LOCATION
    [Serializable]
    public class CLocationState
    {
        public bool unlocked;
        public long timerForLock;           // когда локация будет сброшена
        
        public CSaveCrateBunker[] crate;
    }

    [Serializable]
    public class CSaveCrateBunker
    {
        public CPlayerInventory[] slot;
    }
}