using System.Collections.Generic;
using Galactic1.Code.Gameplay.Combat.Burst;
using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Code.Gameplay.Combat.Hit;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat
{
    /// <summary>
    /// Thin weapon fire orchestrator.
    ///
    /// FIXED vs earlier draft: WeaponFireService used to also own damage
    /// dispatch, suppression aggregation, and CombatHitEvent/CombatKillEvent
    /// raising. That responsibility is now in CombatBatchProcessor.
    ///
    /// WeaponFireService's ONLY job:
    ///   FireRequest → BurstFireResolver → NativeHitBatch (transient, internal)
    ///     → HitResolver (per HitRequest) → List&lt;HitResult&gt; → HitBatchResult
    ///
    /// CombatMissEvent is raised HERE, not in CombatBatchProcessor — because
    /// HitRequest.Origin/Direction (the real shot vector) is only available
    /// at this stage. HitResult for a miss carries no meaningful Point/Normal,
    /// so by the time CombatBatchProcessor sees the batch, miss data would
    /// already be lost.
    ///
    /// Called by WeaponCombatBridge — NOT by FireComponent or WeaponView directly.
    /// </summary>
    public sealed class WeaponFireService
    {
        private readonly BurstFireResolver _burst;
        private readonly HitResolver _hitResolver;
        private readonly CombatBatchProcessor _batchProcessor;

        public WeaponFireService(
            BurstFireResolver burst,
            HitResolver hitResolver,
            CombatBatchProcessor batchProcessor)
        {
            _burst = burst;
            _hitResolver = hitResolver;
            _batchProcessor = batchProcessor;
        }

        /// <summary>
        /// Executes the full gameplay combat resolution for one fire action.
        /// origin/direction are muzzle-space values supplied by the scene layer
        /// (WeaponCombatBridge) — WeaponFireService has zero Transform/MonoBehaviour
        /// dependencies of its own.
        /// </summary>
        public HitBatchResult Execute(
            WeaponEntity weapon,
            FireRequest request,
            IUnitSceneContext attacker,
            Vector3 origin,
            Vector3 direction)
        {
            float range = weapon.Definition.Range;

            // NativeHitBatch is transient — never leaves this method.
            NativeHitBatch batch = _burst.Resolve(request, attacker, origin, direction);

            var hits = new List<HitResult>(batch.Requests.Length);

            for (int i = 0; i < batch.Requests.Length; i++)
            {
                HitRequest hitRequest = batch.Requests[i];
                HitResult result = _hitResolver.Resolve(hitRequest, range);

                hits.Add(result);

                if (!result.Hit)
                {
                    // Raised here — HitRequest still has the real shot vector.
                    EventBus<CombatMissEvent>.Raise(new CombatMissEvent(
                        attacker,
                        hitRequest.Origin,
                        hitRequest.Direction,
                        weapon.Definition.WeaponType));
                }
                
                // --- DEBUG ---
                Vector3 endPoint = result.Hit
                    ? result.Point
                    : hitRequest.Origin + hitRequest.Direction.normalized * range;

                EventBus<CombatTraceEvent>.Raise(
                    new CombatTraceEvent(
                        hitRequest.Origin,
                        hitRequest.Direction,
                        result.Hit,
                        endPoint));
            }

            var batchResult = new HitBatchResult(hits);

            // Damage dispatch, suppression, CombatHitEvent/CombatKillEvent —
            // all delegated. WeaponFireService does not touch DamagePipeline.
            _batchProcessor.Process(batchResult, attacker);

            return batchResult;
        }
    }
}