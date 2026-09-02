using Galactic1.Code.Gameplay.Combat.Data;
using UnityEngine;

namespace Galactic1.Code.Data.Combat
{
    /// <summary>
    /// Data-driven surface behavior config.
    /// Used by:
    /// - SurfaceMaterialDatabase
    /// - PenetrationResolver
    /// - Visual FX systems
    /// - Audio systems
    /// </summary>
    [CreateAssetMenu(
        menuName = "Game Configs/Combat/Surface Material",
        fileName = "SurfaceMaterial_")]
    public sealed class SurfaceMaterialConfig : ScriptableObject
    {
        public SurfaceType Surface;

        [Header("Combat")]
        [Tooltip("Multiplier applied to damage after hitting this surface. " +
                 "< 1 = absorbs (flesh), > 1 = amplifies (none by default)")]
        [Range(0f, 2f)]
        public float PenetrationModifier = 1f;

        [Tooltip("Chance (0-1) that a projectile ricochets off this surface.")]
        [Range(0f, 1f)]
        public float RicochetChance;
    }
}