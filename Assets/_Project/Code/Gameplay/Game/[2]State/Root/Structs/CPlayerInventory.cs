using System;

namespace Galactic1
{
    [Serializable]
    public class CPlayerInventory
    {
        public bool unlock;
        public int type;                           // доступ к массиву по типу предмета (ресурс/предмет крафта)
        // access to lib[]
        public byte category;
        public int id;
        public short volume;
        public short strength;                      // прочность предмета
    }
}