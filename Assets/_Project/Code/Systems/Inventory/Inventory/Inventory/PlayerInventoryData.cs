using UnityEngine;
using Galactic1.Core;

namespace Galactic1.Code.UI.Inventory
{
    [CreateAssetMenu(menuName = "Game Configs/Inventory/Player Inventory Data")]
    public class PlayerInventoryData : CharacterInventoryData
    {




        public override void Initialize(Object data = null)
        {
            InventoryProxy = ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnits[0]
                .InventoryProxy;

            if (InventoryProxy.Slots.Count == 0)
            {
                for (int i = 0; i < baseCapacity; i++)
                    InventoryProxy.Slots
                        .Add(new InventorySlotProxy(new InventorySlotData(null, "", 0, 0, 0)));
            }
        }



        /// <summary>
        /// Установить рюкзак и изменить количество слотов
        /// </summary>
        /// <param name="extraSlots">Количество дополнительных слотов, которые даёт рюкзак</param>
        // public override void SetBackpack(int extraSlots)
        // {
        //     int newTotal = baseCapacity + extraSlots;
        //     
        //     List<InventorySlotProxy> oldSlots = new List<InventorySlotProxy>(InventoryProxy.Slots);
        //     InventoryProxy.Slots = new List<InventorySlotProxy>(newTotal);
        //     
        //     for (int i = 0; i < newTotal; i++)
        //     {
        //         if (i < oldSlots.Count)
        //             InventoryProxy.Slots.Add(oldSlots[i]);
        //         else
        //             InventoryProxy.Slots.Add(new InventorySlot(null, 0));
        //     }
        //     
        //     OnChanged?.Invoke();
        // }

    }
}
