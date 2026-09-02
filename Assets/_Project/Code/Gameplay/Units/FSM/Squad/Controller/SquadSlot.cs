using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Один слот формации. Назначается агенту один раз при создании отряда.
    /// Никогда не переназначается во время движения.
    ///
    /// Pipeline полей:
    ///   LocalOffset            → статика, пересчитывается только при RebuildOffsets()
    ///   DesiredWorldPosition   → FormationFollower пишет каждый тик
    ///   ProjectedWorldPosition → SlotProjector пишет каждый тик
    ///   FinalWorldPosition     → SlotSeparator пишет каждый тик
    ///   IsProjected            → только для дебага и визуализации, на геймплей не влияет
    /// </summary>
    public sealed class SquadSlot
    {
        public int Index;
        public SurvivorInstance Occupant;

        // Статика: локальный офсет в нейтральном базисе (forward = Vector3.forward).
        // FormationFollower применяет реальный forward через Quaternion.LookRotation.
        // Пересчитывается только в SquadFormationSlots.RebuildOffsets().
        public Vector3 LocalOffset;

        // ── Slot pipeline stages ──────────────────────────────
        public Vector3 DesiredWorldPosition; // Center + Rotation(Forward) * LocalOffset
        public Vector3 ProjectedWorldPosition; // после снапа на navmesh
        public Vector3 FinalWorldPosition; // после SlotSeparator

        // Только для дебага: true если ProjectedWorldPosition получен через navmesh snap,
        // false если использован fallback (= DesiredWorldPosition).
        public bool IsProjected;
    }
}