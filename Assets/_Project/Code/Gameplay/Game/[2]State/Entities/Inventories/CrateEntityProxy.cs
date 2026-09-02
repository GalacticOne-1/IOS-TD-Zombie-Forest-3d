using Galactic1.Code.UI.Inventory;
using R3;

namespace Galactic1
{
    /// <summary>
    /// Proxy >> CrateEntity
    /// </summary>
    public class CrateEntityProxy
    {
        public string Id { get; }
        
        
        public CrateEntityData Origin { get; }
        public ReactiveProperty<bool> Unlock { get; }
        //public ReactiveProperty<InventorySlot>[] Slot { get; }


        
        public CrateEntityProxy(CrateEntityData crateEntityData)
        {
            Id = crateEntityData.UniqueId;
            Origin = crateEntityData;

            // R3
            Unlock = new(crateEntityData.Unlock);

            // var l = crateEntityData.Slot.Length;
            // Slot = new ReactiveProperty<InventorySlot>[l];
            // for (int i = 0; i < l; i++)
            // {
            //     Slot[i] = new(crateEntityData.Slot[i]);
            //     
            //     // subscription
            //     Slot[i].Skip(1).Subscribe(_ => crateEntityData.Slot[i] = _);
            // }


            // subscription
            Unlock.Skip(1).Subscribe(_ => crateEntityData.Unlock = _);
        }
    }
}