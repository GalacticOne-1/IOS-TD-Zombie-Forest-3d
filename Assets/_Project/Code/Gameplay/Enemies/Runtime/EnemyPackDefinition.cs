// File: Code/Systems/Raid/Enemies/EnemyPackDefinition.cs

namespace Galactic1.Code.Systems.Raid.Enemies
{
    public sealed class EnemyPackDefinition
    {
        public float EncircleRadius { get; }
        public float SlotAngleStep { get; }
        public float MinSlotDistance { get; }
        public float PackSlotWeight { get; }
        public int MaxAttackersPerTarget { get; }

        public EnemyPackDefinition(
            float encircleRadius,
            float slotAngleStep,
            float minSlotDistance,
            float packSlotWeight,
            int maxAttackersPerTarget = 6)
        {
            EncircleRadius = encircleRadius;
            SlotAngleStep = slotAngleStep;
            MinSlotDistance = minSlotDistance;
            PackSlotWeight = packSlotWeight;
            MaxAttackersPerTarget = maxAttackersPerTarget;
        }
    }
}