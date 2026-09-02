// Code/Gameplay/AoE/TemporalAoEZone.cs

using System.Collections.Generic;
using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.Combat;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Gameplay.Units.Interfaces;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.Code.Gameplay.AoE
{
    /// <summary>
    /// Runtime-объект временной AoE-зоны.
    ///
    /// Responsibilities:
    /// - Отслеживает юнитов внутри радиуса (enter/exit).
    /// - Применяет эффект при входе, снимает при выходе.
    /// - Тикает урон по интервалу (Burn, Electric).
    /// - Уничтожается по истечении Duration.
    ///
    /// ИЗМЕНЕНИЯ после рефакторинга:
    ///   ApplyDamageToReceiver теперь вызывает CombatEventService.RaiseHit()
    ///   после DamageResolver.Apply() — DoT-урон поднимает CombatHitEvent
    ///   и регистрирует смерть так же как пуля или взрыв.
    ///   FlushSuppression() вызывается один раз в конце каждого тика урона.
    /// </summary>
    public sealed class TemporalAoEZone
    {
        // ─── Запрос ───────────────────────────────────────
        private readonly TemporalAoERequest _request;
        private readonly CombatEventService _combatEvents;

        // ─── Таймеры ──────────────────────────────────────
        private float _remainingDuration;
        private float _tickTimer;

        // ─── Юниты внутри зоны ───────────────────────────
        private readonly HashSet<DamageReceiverProxy> _affected = new();
        private readonly HashSet<DamageReceiverProxy> _currentFrame = new();
        private readonly List<DamageReceiverProxy> _removeBuffer = new();
        private readonly Collider[] _overlapBuffer = new Collider[128];

        // ─── Состояние ────────────────────────────────────
        public bool IsExpired => _remainingDuration <= 0f;

        // ─── VFX handle ───────────────────────────────────
        private GameObject _vfxInstance;

        // ─────────────────────────────────────────────────

        public TemporalAoEZone(TemporalAoERequest request, CombatEventService combatEvents)
        {
            _request = request;
            _combatEvents = combatEvents;

            _remainingDuration = request.Duration;
            _tickTimer = request.TickInterval; // первый тик — сразу при создании

            SpawnVFX();

#if UNITY_EDITOR
            DLog.Alert("Start Temporal AoE Zone -> " + request.Type);
#endif
        }

        // ─────────────────────────────────────────────────
        // Tick — вызывается из TemporalAoEService каждый кадр
        // ─────────────────────────────────────────────────

        public void Tick(float dt)
        {
            if (IsExpired) return;

            _remainingDuration -= dt;

            RefreshAffected();

            if (_request.Type != TemporalAoEType.Concussive)
            {
                _tickTimer += dt;
                if (_tickTimer >= _request.TickInterval)
                {
                    _tickTimer = 0f;
                    ApplyDamageTick();
                }
            }

            if (IsExpired)
                Expire();
        }

        // ─────────────────────────────────────────────────
        // Обновление списка юнитов
        // ─────────────────────────────────────────────────

        private void RefreshAffected()
        {
            int count = Physics.OverlapSphereNonAlloc(
                _request.Position,
                _request.Radius,
                _overlapBuffer,
                _request.TargetMask,
                QueryTriggerInteraction.Collide);

            _currentFrame.Clear();
            _removeBuffer.Clear();

            for (int i = 0; i < count; i++)
            {
                var hit = _overlapBuffer[i];
                if (!HitboxRegistry.TryGetReceiver(hit, out var receiver) || receiver == null)
                    continue;
                _currentFrame.Add(receiver);
            }

            foreach (var receiver in _currentFrame)
            {
                if (_affected.Add(receiver))
                    OnReceiverEnter(receiver);
            }

            foreach (var receiver in _affected)
            {
                if (receiver == null || !_currentFrame.Contains(receiver))
                    _removeBuffer.Add(receiver);
            }

            foreach (var receiver in _removeBuffer)
            {
                _affected.Remove(receiver);
                OnReceiverExit(receiver);
            }
        }

        // ─────────────────────────────────────────────────
        // Enter / Exit
        // ─────────────────────────────────────────────────

        private void OnReceiverEnter(DamageReceiverProxy receiver)
        {
            if (receiver.Unit == null ||
                !TeamService.CanDamage(_request.Attacker.Runtime, receiver.Unit?.RuntimeBase))
                return;

            switch (_request.Type)
            {
                case TemporalAoEType.Electric:
                    ApplySlow(receiver.Entity);
                    break;
                case TemporalAoEType.Concussive:
                    ApplyStun(receiver.Entity);
                    break;
            }
        }

        private void OnReceiverExit(DamageReceiverProxy receiver)
        {
            if (receiver.Unit == null ||
                !TeamService.CanDamage(_request.Attacker.Runtime, receiver.Unit?.RuntimeBase))
                return;

            switch (_request.Type)
            {
                case TemporalAoEType.Electric:
                    RemoveSlow(receiver.Entity);
                    break;
            }
        }

        // ─────────────────────────────────────────────────
        // Урон по тику (Burn, Electric)
        // ─────────────────────────────────────────────────

        private void ApplyDamageTick()
        {
            if (_request.DamagePerTick <= 0f) return;

            _removeBuffer.Clear();

            foreach (var receiver in _affected)
            {
                if (receiver == null)
                {
                    _removeBuffer.Add(receiver);
                    continue;
                }

                ApplyDamageToReceiver(receiver, _request.DamagePerTick);
            }

            foreach (var r in _removeBuffer)
                _affected.Remove(r);

            // Один flush на тик — аналогично батчу пуль и взрыву.
            _combatEvents.FlushSuppression();
        }

        /// <summary>
        /// Применяет урон и сразу поднимает боевые события.
        /// DoT-урон теперь виден в UI (HP bar, FloatingDamage) так же как пуля.
        /// </summary>
        private void ApplyDamageToReceiver(DamageReceiverProxy receiver, float damage)
        {
            if (!TeamService.CanDamage(_request.Attacker.Runtime, receiver.Unit?.RuntimeBase))
                return;

            // Позиция юнита — не центр зоны.
            Vector3 unitPos = receiver.transform.position;

            Vector3 toUnit = (unitPos - _request.Position).normalized;
            Vector3 normal = toUnit != Vector3.zero ? toUnit : Vector3.up;

            var hitInfo = new HitInfo
            {
                Point = unitPos,
                Normal = normal
            };

            DamageResult result = DamageResolver.Apply(
                receiver,
                _request.Attacker,
                damage,
                DamageType.Flame,
                hitInfo);

            // DoT: shotDirection = Vector3.zero (нет направленного выстрела).
            _combatEvents.RaiseHit(
                _request.Attacker,
                receiver.Unit,
                result,
                hitInfo,
                shotDirection: Vector3.zero);
        }

        // ─────────────────────────────────────────────────
        // Эффекты на юнитов
        // ─────────────────────────────────────────────────

        private void ApplySlow(ISceneEntity entity)
        {
            if (entity.TryGetCapability(out ISlowable s))
                s.ApplySlow(this, _request.SpeedMultiplier);
        }

        private void RemoveSlow(ISceneEntity entity)
        {
            if (entity.TryGetCapability(out ISlowable s))
                s.RemoveSlow(this);
        }

        private void ApplyStun(ISceneEntity entity)
        {
            if (entity.TryGetCapability(out IStunnable s))
                s.ApplyStun(_request.StunDuration);
        }

        // ─────────────────────────────────────────────────
        // Истечение зоны
        // ─────────────────────────────────────────────────

        private void Expire()
        {
            foreach (var receiver in _affected)
                OnReceiverExit(receiver);

            _affected.Clear();

#if UNITY_EDITOR
            DLog.Alert("End Temporal AoE Zone -> " + _request.Type, EDlogColor.YELLOW);
#endif
        }

        // ─────────────────────────────────────────────────
        // VFX
        // ─────────────────────────────────────────────────

        private void SpawnVFX()
        {
            ServiceLocator.Current.Get<EffectRequestSystem>().Request(
                new EffectRequest
                {
                    Id = _request.VfxId,
                    Position = _request.Position,
                    Duration = _request.VfxSelfDuration ? 0 : _request.Duration
                },
                EffectPriority.Normal,
                fx =>
                {
                    _vfxInstance = fx;
                    fx.SetActive(true);
                });
        }
    }
}