using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Combat.Data;
using UnityEngine;

namespace Galactic1.Code.Data.Combat
{
    /// <summary>
    /// Data-driven surface → FX mapping config.
    ///
    /// Holds VfxId keys used by EffectRequestSystem.
    /// One asset per surface type.
    ///
    /// Used by:
    /// - CombatSurfaceFXDatabase
    /// - ImpactAggregationSystem
    /// - DecalSystem
    /// </summary>
    [CreateAssetMenu(
        menuName = "Game Configs/Combat/Surface FX Config",
        fileName = "SurfaceFX_")]
    public sealed class CombatSurfaceFXConfig : ScriptableObject
    {
        public SurfaceType Surface;

        [Header("Visual FX")] [Tooltip("VfxId for impact particle effect. Must be registered in EffectRequestSystem.")]
        public VfxId ImpactFXId;

        [Tooltip("VfxId for decal (bullet hole / blood). Leave empty to skip.")]
        public VfxId DecalId;

        [Header("Audio")] [Tooltip("Audio key for impact sound. Used by AudioSurfaceDatabase (Phase 5).")]
        public string ImpactAudioKey;
    }
}