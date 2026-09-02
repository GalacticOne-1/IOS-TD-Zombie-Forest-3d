using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Gameplay.Combat.Resolvers;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Core.Gameplay;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Hit
{
    /// <summary>
    /// Central deterministic hit simulation.
    ///
    /// Pipeline per HitRequest:
    ///   Physics.Raycast → Receiver lookup → Surface → BodyPart
    ///
    /// NOTE: Accuracy and Cover resolution are intentionally NOT part of
    /// this resolver per the Phase 1 migration spec's explicit scope
    /// ("Required Systems → HitResolver" lists only Raycast/Surface/BodyPart).
    /// If accuracy rolls or cover blocking need to be added, they belong
    /// here as an explicit follow-up — not silently inferred.
    ///
    /// NO damage application here — only resolution.
    /// Damage is applied by WeaponFireService via the existing DamagePipeline.
    ///
    /// Used by WeaponFireService.
    /// </summary>
    public sealed class HitResolver
    {
        private readonly SurfaceResolver _surface;
        private readonly BodyPartResolver _bodyPart;

        public HitResolver(
            SurfaceResolver surface,
            BodyPartResolver bodyPart)
        {
            _surface = surface;
            _bodyPart = bodyPart;
        }

        /// <summary>
        /// Resolves a single HitRequest.
        /// range comes from WeaponDefinitionData.Range (read by WeaponFireService).
        /// </summary>
        public HitResult Resolve(HitRequest request, float range)
        {
            RaycastHit[] hits = Physics.RaycastAll(
                request.Origin,
                request.Direction,
                range,
                Layers.Damageable);

            if (hits.Length == 0)
                return Miss();

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                HitboxRegistry.TryGetReceiver(hit.collider, out DamageReceiverProxy receiver);

                // Стена / объект мира — останавливаем луч
                if (receiver == null)
                {
                    return new HitResult(
                        true,
                        hit.point,
                        hit.normal,
                        request.Direction,
                        _surface.Resolve(hit.collider),
                        BodyPartType.Torso,
                        null,
                        request.Damage,
                        request.ArmorPenetration);
                }

                // Союзник — луч проходит насквозь
                if (!TeamService.CanDamage(request.Attacker.RuntimeBase, receiver.Unit.RuntimeBase))
                    continue;

                // Враг — попадание
                return new HitResult(
                    true,
                    hit.point,
                    hit.normal,
                    request.Direction,
                    _surface.Resolve(hit.collider),
                    _bodyPart.Resolve(hit.collider),
                    receiver.Unit,
                    request.Damage,
                    request.ArmorPenetration);
            }

            return Miss();
        }

        private static HitResult Miss() => new(
            false, default, default, default,
            SurfaceType.Default, BodyPartType.Torso,
            null, 0f, 0f);
    }
}