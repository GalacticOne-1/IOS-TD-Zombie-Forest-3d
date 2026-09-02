using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Cover
{
    /// <summary>
    /// Runtime tactical cover state.
    /// Owned by unit runtime, exposed via IUnitSceneContext.Cover.
    /// Used by CoverResolver.
    /// </summary>
    public struct UnitCoverState
    {
        public CoverType CoverType;

        /// <summary>
        /// Direction the unit is facing relative to cover.
        /// Used to determine if attacker is on covered side.
        /// </summary>
        public Vector3 CoverDirection;

        /// <summary>
        /// Convenience default — no cover.
        /// Used by adapters that don't yet implement cover detection
        /// (e.g. EnemySceneAdapter until enemy cover-seeking AI exists).
        /// </summary>
        public static UnitCoverState None_ => new UnitCoverState
        {
            CoverType = CoverType.None,
            CoverDirection = Vector3.zero
        };
    }
}