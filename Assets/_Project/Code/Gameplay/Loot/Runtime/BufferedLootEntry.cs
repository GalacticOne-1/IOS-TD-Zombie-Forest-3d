using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.RaidLoot.Authoring;

namespace Galactic1.RaidLoot.Runtime
{
    /// <summary>
    /// One entry inside RaidLootBuffer.
    /// Wraps LootGenerationRecord and adds the timestamp of pickup.
    /// </summary>
    public readonly struct BufferedLootEntry
    {
        public LootGenerationRecord Record { get; }
        public float PickedAt { get; } // Time.time at moment of pickup

        public BufferedLootEntry(LootGenerationRecord record, float pickedAt)
        {
            Record = record;
            PickedAt = pickedAt;
        }

        // Convenience pass-throughs so callers don't always write .Record.X
        public ItemConfig Item => Record.Item;
        public int Amount => Record.Amount;
        
        
        
    }
}