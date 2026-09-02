
using System;
using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;

namespace Galactic1.Code.UI.RaidReport.Drone
{
    /// <summary>
    /// Данные дрона для передачи в InventoryManagementWindow.
    /// Живёт только на время открытия окна инвентаря.
    /// </summary>
    public class DroneOpenContext
    {
        public DroneSessionState State { get; }
        public Action<List<InventorySlotRuntime>> OnSent { get; }

        public DroneOpenContext(
            DroneSessionState state,
            Action<List<InventorySlotRuntime>> onSent)
        {
            State = state;
            OnSent = onSent;
        }
    }
}