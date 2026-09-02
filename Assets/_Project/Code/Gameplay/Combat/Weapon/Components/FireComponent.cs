using System;
using Random = System.Random;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public sealed class FireComponent : WeaponComponentBase
    {
        private float _cooldown;
        private int _burstFired;

        private FireRequest? _pendingRequest;
        private WeaponEntity _pendingEntity;

        public override void Tick(float dt)
        {
            if (_cooldown > 0f)
            {
                _cooldown -= dt;
            }
        }

        public bool IsReady => _cooldown <= 0f;

        /// <summary>
        /// Инициирует выстрел с боевым контекстом.
        /// context.TargetDistance используется SpreadComponent для расчёта штрафа за дистанцию.
        /// Остальные поля FireContext будут читаться здесь по мере добавления.
        /// </summary>
        public void Execute(WeaponEntity e, FireContext context)
        {
            if (!IsReady) return;

            _pendingRequest = BuildRequest(e, context);
            _pendingEntity = e;

            e.RaiseFireAnimationRequested();
        }

        private void ApplyCooldown(WeaponEntity e)
        {
            var def = e.Definition;
            float interval = 60f / def.RoundsPerMinute;

            switch (def.FireMode)
            {
                case FireMode.SemiAuto:
                    _cooldown = interval;
                    break;

                case FireMode.Burst:
                    _burstFired++;
                    _cooldown = _burstFired >= def.BurstCount
                        ? def.BurstPauseSec
                        : interval;

                    if (_burstFired >= def.BurstCount)
                        _burstFired = 0;
                    break;

                case FireMode.FullAuto:
                    _cooldown = interval;
                    break;
            }
        }

        public void OnAnimationFireEvent()
        {
            if (_pendingRequest == null || _pendingEntity == null)
                return;

            var request = _pendingRequest.Value;
            var entity = _pendingEntity;
            _pendingRequest = null;
            _pendingEntity = null;

            entity.RaiseCombatFireRequested(request);
        }

        public void CompleteFire(WeaponEntity e)
        {
            ApplyCooldown(e);
        }

        /// <summary>
        /// Строит FireRequest с углами разброса для всех картечин/пуль.
        /// SpreadComponent.GetSpreadForDistance() инкапсулирует весь расчёт:
        ///   CurrentSpreadDeg (базовый × движение × стресс) × RangePenalty(distance).
        /// Fallback на BaseSpreadDeg если SpreadComponent не установлен.
        /// </summary>
        private FireRequest BuildRequest(WeaponEntity e, FireContext context)
        {
            var def = e.Definition;
            var spread = e.Get<SpreadComponent>();
            var falloff = e.Get<DamageFalloffComponent>();
            var projectilesCount = Math.Max(1, def.ProjectilesPerShot);

            var angles = new float[projectilesCount * 2];
            for (int i = 0; i < projectilesCount; i++)
            {
                float s = spread != null
                    ? spread.GetSpreadForDistance(context.TargetDistance)
                    : def.BaseSpreadDeg;

#if UNITY_EDITOR
                if (i == 0)
                    DLog.Alert(
                        $"[Weapon|{def.FireMode}] " +
                        $"Distance={context.TargetDistance:F1}m " +
                        $"Spread={s:F2}°", EDlogColor.YELLOW);
#endif

                angles[i * 2] = RandomRange(-s, s);
                angles[i * 2 + 1] = RandomRange(-s, s);
            }

            float baseDmg = def.Damage * (1f + RandomRange(-def.DamageVariance, def.DamageVariance));
            
            // Falloff применяется здесь — downstream системы получают уже финальный урон
            float damageMultiplier = falloff?.GetDamageMultiplier(context.TargetDistance) ?? 1f;
            float finalDmg = baseDmg * damageMultiplier;
            //DLog.Alert($"Final damage: {finalDmg} ({baseDmg}*{damageMultiplier}) ", EDlogColor.YELLOW);
            
            return new FireRequest(
                angles, 
                projectilesCount,
                finalDmg,
                def.ArmorPiercing,
                def.FireType,
                context.TargetAimPoint);
        }

        public Func<float, float, float> RandomRange = (min, max) =>
            min + (float)new Random().NextDouble() * (max - min);
    }
}