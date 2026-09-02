using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Burst
{
    /// <summary>
    /// Expands a single FireRequest (from existing FireComponent) into
    /// a NativeHitBatch of per-pellet HitRequests.
    ///
    /// FireRequest carries pre-baked spread angles (computed by
    /// FireComponent.BuildRequest using SpreadComponent) — this resolver
    /// just applies each angle pair to the muzzle direction and builds
    /// one HitRequest per shot.
    ///
    /// Handles:
    /// - shotgun pellets (ShotsCount > 1, same origin, diverging directions)
    /// - burst fire (same as above, semantically one trigger pull)
    /// - future miniguns (same mechanism, just more pellets)
    ///
    /// Used by WeaponFireService.
    /// </summary>
    public sealed class BurstFireResolver
    {
        /// <summary>
        /// Builds a NativeHitBatch from the fire request and muzzle transform.
        /// origin/direction come from the scene layer (WeaponCombatBridge),
        /// NOT from FireRequest — FireRequest only carries damage/spread/ammo data.
        ///
        /// NOTE: Range and Accuracy are NOT part of HitRequest — they live on
        /// WeaponDefinitionData and are read directly by WeaponFireService /
        /// HitResolver from weapon.Definition. Per-pellet damage and armor
        /// penetration DO travel in HitRequest because FireComponent already
        /// applies damage variance per-request (see FireComponent.BuildRequest).
        /// </summary>
        public NativeHitBatch Resolve(
            FireRequest request,
            IUnitSceneContext attacker,
            Vector3 origin,
            Vector3 direction)
        {
            int count = Mathf.Max(1, request.ProjectilesCount);
            var hitRequests = new HitRequest[count];

            for (int i = 0; i < count; i++)
            {
                Vector3 shotDirection = ApplySpread(direction, request.SpreadAngles, i);

                hitRequests[i] = new HitRequest(
                    origin,
                    shotDirection,
                    request.Damage,
                    request.ArmorPiercing,
                    attacker);
            }

            return new NativeHitBatch(hitRequests);
        }

        private static Vector3 ApplySpread(Vector3 baseDirection, float[] spreadAngles, int index)
        {
            if (spreadAngles == null || spreadAngles.Length < (index + 1) * 2)
                return baseDirection;

            float angleX = spreadAngles[index * 2];
            float angleY = spreadAngles[index * 2 + 1];

            return Quaternion.Euler(angleX, angleY, 0f) * baseDirection;
        }
    }
}