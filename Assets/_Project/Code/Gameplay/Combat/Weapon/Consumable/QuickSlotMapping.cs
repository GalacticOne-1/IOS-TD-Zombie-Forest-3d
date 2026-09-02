
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Core.Enums;

namespace Galactic1.Code.Systems.Raid
{
    public sealed class QuickSlotMapping
    {
        public const int SlotCount = 4;

        private readonly int[] _slotToInventoryIndex = new int[SlotCount];

        // =========================================================
        // INIT (важно!)
        // =========================================================
        public QuickSlotMapping(IInventorySource source)
        {
            // дефолт
            for (int i = 0; i < SlotCount; i++)
                _slotToInventoryIndex[i] = -1;

            var map = source.EquipmentSlots;

            foreach (var kv in map)
            {
                int inventoryIndex = kv.Key;
                var type = kv.Value;

                switch (type)
                {
                    case EquipmentSlotType.QuickSlot1:
                        _slotToInventoryIndex[0] = inventoryIndex;
                        break;

                    case EquipmentSlotType.QuickSlot2:
                        _slotToInventoryIndex[1] = inventoryIndex;
                        break;

                    case EquipmentSlotType.QuickSlot3:
                        _slotToInventoryIndex[2] = inventoryIndex;
                        break;

                    case EquipmentSlotType.QuickSlot4:
                        _slotToInventoryIndex[3] = inventoryIndex;
                        break;
                }
            }
        }

        // =========================================================
        // API
        // =========================================================
        public InventorySlotRuntime GetSlot(IInventorySource source, int quickIndex)
        {
            if (quickIndex < 0 || quickIndex >= SlotCount)
                return null;

            int invIndex = _slotToInventoryIndex[quickIndex];

            if (invIndex < 0 || invIndex >= source.GetSlots().Count)
                return null;

            return source.GetSlot(invIndex);
        }

        public int GetInventoryIndex(int quickIndex)
        {
            if (quickIndex < 0 || quickIndex >= SlotCount)
                return -1;

            return _slotToInventoryIndex[quickIndex];
        }
    }
}