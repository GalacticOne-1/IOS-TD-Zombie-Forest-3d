using Galactic1.Code.Gameplay.Combat;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Core.Gameplay;
using UnityEngine;

namespace Galactic1.Code.Gameplay.AoE
{
    /// <summary>
    /// Центральный сервис AoE.
    ///
    /// ИЗМЕНЕНИЯ после рефакторинга:
    ///   Было: AoEService не поднимал CombatHitEvent → UI не реагировал на взрывы.
    ///   Стало: DamageResolver.Apply() → CombatEventService.RaiseHit() → CombatHitEvent.
    ///
    ///   AoEService не знает о CombatHitEvent напрямую.
    ///   Он только вызывает ApplyDamage + RaiseHit.
    ///   Остальное (UI, анимации, смерть) — в CombatEventService и подписчиках EventBus.
    ///
    /// DamageService.ApplyDamage заменяет DamageResolver.Apply —
    /// единый API для всех источников урона.
    /// </summary>
    public sealed class AoEService : IGameService
    {
        private readonly LayerMask _damageable;
        private CombatEventService _combatEvents;

        public AoEService(LayerService layerService)
        {
            _damageable = layerService.Damageable;
        }

        public void Initialize(CombatEventService combatEvents)
        {
            _combatEvents = combatEvents;
        }

        public void Execute(AoERequest request)
        {
            if (request.Duration <= 0f)
            {
                if (request.BigRadius > 0)
                    ApplyInstantGrenade(request);
                else
                    ApplyInstant(request);
            }
        }

        // ── Instant ───────────────────────────────────────────────────────

        private void ApplyInstant(AoERequest request)
        {
            var hits = Physics.OverlapSphere(
                request.Position,
                request.SmallRadius,
                request.TargetMask);

            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent<HitboxProxy>(out var proxy)) continue;
                if (request.RequireLOS && !HasLOS(request.Position, hit)) continue;

                float damage = CalculateDamage(request, hit.transform.position);
                // Используем позицию ближайшей точки коллайдера цели — не центр взрыва.
                // VisualImpactEvent (кровь, декали) должен появляться на теле юнита.
                Vector3 hitPoint = hit.ClosestPoint(request.Position);
                ApplyDamageAndRaiseEvents(request, proxy.Receiver, damage, hitPoint);

                if (request.ApplyEffects)
                    ApplyEffects(proxy.Receiver, request);
            }
        }

        private void ApplyInstantGrenade(AoERequest request)
        {
            float searchRadius = Mathf.Max(request.SmallRadius, request.BigRadius);

            var hits = Physics.OverlapSphere(
                request.Position,
                searchRadius,
                request.TargetMask);

            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent<HitboxProxy>(out var proxy)) continue;
                if (request.RequireLOS && !HasLOS(request.Position, hit)) continue;

                float damage = CalculateDamageGrenade(request, hit.transform.position);
                if (damage <= 0f) continue;

                Vector3 hitPoint = hit.ClosestPoint(request.Position);
                ApplyDamageAndRaiseEvents(request, proxy.Receiver, damage, hitPoint);

                if (request.ApplyEffects)
                    ApplyEffects(proxy.Receiver, request);
            }

            // Один flush после всех целей взрыва — аналогично батчу пуль.
            _combatEvents.FlushSuppression();
        }

        // ── Damage + Events ───────────────────────────────────────────────

        /// <summary>
        /// Применяет урон и сразу поднимает боевые события через CombatEventService.
        /// Это единственное место в AoEService где происходит взаимодействие с combat layer.
        /// </summary>
        private void ApplyDamageAndRaiseEvents(
            AoERequest request,
            DamageReceiverProxy receiver,
            float damage,
            Vector3 hitPoint)
        {
            var attacker = request.Attacker;

            if (attacker == null || receiver == null) return;
            if (!TeamService.CanDamage(attacker.Runtime, receiver.Unit?.RuntimeBase)) return;

            // Normal: направление от центра взрыва к точке попадания на юните.
            // VisualImpactEvent использует нормаль для ориентации эффекта крови/декали.
            Vector3 normal = (hitPoint - request.Position).normalized;
            if (normal == Vector3.zero) normal = Vector3.up;

            var hitInfo = new HitInfo
            {
                Point = hitPoint,
                Normal = normal
            };

            DamageResult result = DamageService.ApplyDamage(
                attacker,
                receiver.Unit,
                damage,
                DamageType.Explosion,
                hitInfo);

            // shotDirection = Vector3.zero — взрыв ненаправленный.
            _combatEvents.RaiseHit(
                attacker,
                receiver.Unit,
                result,
                hitInfo,
                shotDirection: Vector3.zero);
        }

        // ── Utils (без изменений) ─────────────────────────────────────────

        private float CalculateDamage(AoERequest req, Vector3 targetPos)
        {
            float dist = Vector3.Distance(req.Position, targetPos);
            float t = Mathf.Clamp01(dist / req.SmallRadius);

            float multiplier = req.DamageFalloff != null
                ? req.DamageFalloff.Evaluate(t)
                : 1f - t;

            return req.MaxDamage * multiplier;
        }

        private float CalculateDamageGrenade(AoERequest req, Vector3 targetPos)
        {
            float dist = Vector3.Distance(req.Position, targetPos);

            if (dist <= req.SmallRadius)
            {
                float t = req.SmallRadius > 0f
                    ? Mathf.Clamp01(dist / req.SmallRadius)
                    : 0f;

                float multiplier = req.DamageFalloff != null
                    ? req.DamageFalloff.Evaluate(t)
                    : 1f - t;

                return req.MaxDamage * multiplier;
            }

            if (req.BigRadius > req.SmallRadius && dist <= req.BigRadius)
            {
                float bigDamage = req.MaxDamage * req.BigRadiusDamagePercent;
                float t = Mathf.Clamp01(
                    (dist - req.SmallRadius) / (req.BigRadius - req.SmallRadius));
                return bigDamage * (1f - t);
            }

            return 0f;
        }

        private bool HasLOS(Vector3 origin, Collider target)
        {
            Vector3 dir = (target.bounds.center - origin).normalized;
            if (Physics.Raycast(origin, dir, out var hit, 100f, _damageable))
                return hit.collider == target;
            return false;
        }

        private void ApplyEffects(DamageReceiverProxy receiver, AoERequest request)
        {
        }
    }
}