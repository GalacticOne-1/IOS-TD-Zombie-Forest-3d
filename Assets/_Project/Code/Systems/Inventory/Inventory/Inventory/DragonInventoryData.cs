using UnityEngine;
using Galactic1.Core;

namespace Galactic1.Code.UI.Inventory
{
    [CreateAssetMenu(menuName = "Game Configs/Inventory/Dragon Inventory Data")]
    public class DragonInventoryData : CharacterInventoryData
    {




        public override void Initialize(Object data = null)
        {
            InventoryProxy = ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnits[1]
                .InventoryProxy;
            if (InventoryProxy.Slots.Count == 0)
            {
                for (int i = 0; i < baseCapacity; i++)
                    InventoryProxy.Slots
                        .Add(new InventorySlotProxy(new InventorySlotData(null, "", 0 , 0, 0)));
            }
        }



        /// <summary>
        /// Установить рюкзак и изменить количество слотов
        /// </summary>
        /// <param name="extraSlots">Количество дополнительных слотов, которые даёт рюкзак</param>
        // public override void SetBackpack(int extraSlots)
        // {
        //     // int newTotal = baseCapacity + extraSlots;
        //     //
        //     // List<InventorySlot> oldSlots = new List<InventorySlot>(slots);
        //     // slots = new List<InventorySlot>(newTotal);
        //     //
        //     // for (int i = 0; i < newTotal; i++)
        //     // {
        //     //     if (i < oldSlots.Count)
        //     //         slots.Add(oldSlots[i]);
        //     //     else
        //     //         slots.Add(new InventorySlot(null, 0));
        //     // }
        //     //
        //     // OnChanged?.Invoke();
        // }

    }
}