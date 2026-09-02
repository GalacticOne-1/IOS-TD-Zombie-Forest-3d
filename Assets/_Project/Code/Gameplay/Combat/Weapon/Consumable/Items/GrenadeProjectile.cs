
using Galactic1.Code.Gameplay.AoE;
using Galactic1.Code.Gameplay.Audio.Grenades;
using Galactic1.Code.Gameplay.Combat.Events;
using Galactic1.Core.Gameplay;
using Galactic1.Game.Meta.Items;
using Galactic1.PoolObject;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Projectiles
{
    /// <summary>
    /// Production-ready grenade projectile.
    ///
    /// Responsibilities:
    /// - Ballistic flight
    /// - Collision sweep
    /// - Bounce phase
    /// - Explosion dispatch (instant AoE или temporal AoE зона)
    /// - Pool-safe lifecycle
    ///
    /// IMPORTANT:
    /// - Does NOT apply damage directly
    /// - Does NOT know gameplay rules
    /// - Only delivers AoE payload
    /// </summary>
    public sealed class GrenadeProjectile :
        PoolableMonoBehaviour,
        IPoolItemConfig<ItemConfig>,
        IUpdate
    {
        // ─────────────────────────────────────────────
        // Config
        // ─────────────────────────────────────────────

        private ItemConfig _itemConfig;

        // ─────────────────────────────────────────────
        // Runtime phases
        // ─────────────────────────────────────────────

        private enum GrenadePhase
        {
            None,
            Flight,
            Bounce,
            Exploded
        }

        private GrenadePhase _phase;

        // ─────────────────────────────────────────────
        // Flight data
        // ─────────────────────────────────────────────

        private Vector3 _flightStart;
        private Vector3 _flightTarget;

        private Vector3 _bounceStart;
        private Vector3 _bounceDirection;
        private Vector3 _lastPosition;

        private float _flightSpeed;
        private float _flightHeight;

        private float _elapsed;

        // Стандартный мгновенный AoE (взрыв)
        private AoERequest _aoe;

        // Temporal AoE зона (молотов, кислота, электро, шумовая)
        // Duration > 0 означает что граната создаёт зону а не взрыв
        private TemporalAoERequest _temporalAoe;
        private bool _hasTemporalAoe;
        
        
        
        // Runtime-safe audio data. Null = граната без звука (валидная конфигурация).
        private GrenadeAudioData _audio;
        
        // ─────────────────────────────────────────────
        // Detonation
        // ─────────────────────────────────────────────

        public enum DetonationMode
        {
            Timer,
            Impact,
            Bounce
        }

        private DetonationMode _detonationMode;
        private float _detonationDelay;
        private float _detonationTimer;

        // ─────────────────────────────────────────────
        // Bounce config
        // ─────────────────────────────────────────────

        private const float BounceHeight = 0.3f;
        private const float BounceDuration = 0.25f;
        private const float BounceDistance = 1.25f;

        // ─────────────────────────────────────────────
        // Collision
        // ─────────────────────────────────────────────

        [Header("Collision")] 
        [SerializeField] private float collisionRadius = 0.08f;

        // ─────────────────────────────────────────────
        // Public API — мгновенный AoE (без изменений)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Запуск гранаты с мгновенным взрывом.
        /// </summary>
        public void Launch(
            Vector3 start,
            Vector3 target,
            float speed,
            float height,
            AoERequest aoe,
            DetonationMode mode,
            float delay = 0f,
            GrenadeAudioConfig audioConfig = null)
        {
            _aoe = aoe;
            _hasTemporalAoe = false;

            LaunchInternal(
                start, 
                target, 
                speed, 
                height,
                mode,
                delay,
                audioConfig);
        }

        // ─────────────────────────────────────────────
        // Public API — temporal AoE (молотов, электро, шумовая)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Запуск гранаты с созданием временной зоны.
        /// Вызывается из GrenadeAbilityBehaviour вместо Launch(AoERequest).
        /// </summary>
        public void Launch(
            Vector3 start,
            Vector3 target,
            float speed,
            float height,
            TemporalAoERequest temporalAoe,
            DetonationMode mode,
            float delay = 0f,
            GrenadeAudioConfig audioConfig = null)
        {
            _temporalAoe = temporalAoe;
            _hasTemporalAoe = true;

            LaunchInternal(
                start, 
                target, 
                speed,
                height,
                mode,
                delay,
                audioConfig);
        }

        // ─────────────────────────────────────────────
        // Общая инициализация полёта
        // ─────────────────────────────────────────────

        private void LaunchInternal(
            Vector3 start,
            Vector3 target,
            float speed,
            float height,
            DetonationMode mode,
            float delay,
            GrenadeAudioConfig audioConfig)
        {
            _flightStart = start;
            _flightTarget = target;
            _flightSpeed = Mathf.Max(0.01f, speed);
            _flightHeight = height;
            
            _detonationMode = mode;
            _detonationDelay = delay;
            _detonationTimer = delay;

            _elapsed = 0f;
            _phase = GrenadePhase.Flight;

            CachedTransform.position = start;
            _lastPosition = start;
            
            // Конвертация SO → runtime data один раз при броске.
            _audio = audioConfig != null ? audioConfig.ToData() : null;

            if (_audio != null)
            {
                EventBus<AudioGrenadeThrowEvent>.Raise(
                    new AudioGrenadeThrowEvent(start, _audio));
            }

            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
        }

        // ─────────────────────────────────────────────
        // Pool config
        // ─────────────────────────────────────────────

        public void SetConfig(ItemConfig config)
        {
            _itemConfig = config;
        }

        // ─────────────────────────────────────────────
        // Pool lifecycle
        // ─────────────────────────────────────────────

        public override void OnSpawn()
        {
            base.OnSpawn();
            _phase = GrenadePhase.None;
            _elapsed = 0f;
        }

        public override void OnDespawn()
        {
            _phase = GrenadePhase.None;
            _elapsed = 0f;

            _flightStart = Vector3.zero;
            _flightTarget = Vector3.zero;
            _bounceStart = Vector3.zero;

            _aoe = default;
            _temporalAoe = default;
            _hasTemporalAoe = false;

            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
            base.OnDespawn();
        }

        public override void ResetState()
        {
            base.ResetState();
            _phase = GrenadePhase.None;
            _elapsed = 0f;
            _flightSpeed = 0f;
            _flightHeight = 0f;
        }

        // ─────────────────────────────────────────────
        // Update
        // ─────────────────────────────────────────────

        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        public void UpdateM()
        {
            if (!IsSpawned) return;

            switch (_phase)
            {
                case GrenadePhase.Flight:
                    UpdateFlight(Time.deltaTime);
                    break;
                case GrenadePhase.Bounce:
                    UpdateBounce(Time.deltaTime);
                    break;
            }
        }

        // ─────────────────────────────────────────────
        // Flight (без изменений)
        // ─────────────────────────────────────────────

        private void UpdateFlight(float dt)
        {
            _elapsed += dt;
            float t = Mathf.Clamp01(_elapsed / _flightSpeed);

            Vector3 pos = Vector3.Lerp(_flightStart, _flightTarget, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * _flightHeight;

            if (SweepCollision(_lastPosition, pos, out var hit))
            {
                switch (_detonationMode)
                {
                    case DetonationMode.Impact:
                        Explode(hit.point);
                        return;

                    case DetonationMode.Bounce:
                        BeginBounce(hit.point, hit.normal);
                        return;
                }
            }

            CachedTransform.position = pos;
            RotateAlongVelocity(pos);
            _lastPosition = pos;
            
            
            if (_detonationMode == DetonationMode.Timer)
            {
                _detonationTimer -= dt;

                if (_detonationTimer <= 0f)
                {
                    Explode(CachedTransform.position);
                    return;
                }
            }

            if (t >= 1f)
            {
                switch (_detonationMode)
                {
                    case DetonationMode.Timer:
                        // ждём пока дотикает timer
                        CachedTransform.position = _flightTarget;
                        break;

                    case DetonationMode.Bounce:
                        BeginBounce(_flightTarget, Vector3.up);
                        break;

                    case DetonationMode.Impact:
                        // impact уже обработан collision sweep
                        Explode(_flightTarget);
                        break;
                }
            }
        }

        // ─────────────────────────────────────────────
        // Bounce (без изменений)
        // ─────────────────────────────────────────────

        private void BeginBounce(Vector3 position, Vector3 normal)
        {
            _phase = GrenadePhase.Bounce;

            _bounceStart = position;

            // Входящий вектор движения
            Vector3 incoming =
                (position - _lastPosition);

            if (incoming.sqrMagnitude <= 0.0001f)
                incoming = CachedTransform.forward;

            incoming.Normalize();

            // Рикошет от поверхности
            Vector3 reflected =
                Vector3.Reflect(incoming, normal);

            // Убираем вертикальный компонент
            reflected.y = 0f;

            // fallback
            if (reflected.sqrMagnitude <= 0.0001f)
                reflected = CachedTransform.forward;

            _bounceDirection = reflected.normalized;

            _elapsed = 0f;

            CachedTransform.position = position;
            _lastPosition = position;
        }

        private void UpdateBounce(float dt)
        {
            _elapsed += dt;

            float t = Mathf.Clamp01(_elapsed / BounceDuration);

            // Ease-out движение вперёд
            float horizontalT =
                1f - Mathf.Pow(1f - t, 2f);

            // Баллистическая дуга
            float vertical =
                4f * BounceHeight * t * (1f - t);

            // Горизонтальное смещение
            Vector3 horizontal =
                _bounceDirection *
                (BounceDistance * horizontalT);

            Vector3 pos =
                _bounceStart +
                horizontal;

            pos.y += vertical;

            CachedTransform.position = pos;

            RotateAlongVelocity(pos);

            _lastPosition = pos;

            if (t >= 1f)
                Explode(pos);
        }

        // ─────────────────────────────────────────────
        // Explosion — точка расширения для temporal AoE
        // ─────────────────────────────────────────────

        private void Explode(Vector3 explosionPosition)
        {
            if (!IsSpawned) return;
            if (_phase == GrenadePhase.Exploded) return;

            _phase = GrenadePhase.Exploded;

            // #1 Audio — раскрывается только если для гранаты задан аудио-ассет.
            if (_audio != null)
            {
                EventBus<AudioGrenadeExplosionEvent>.Raise(
                    new AudioGrenadeExplosionEvent(explosionPosition, _audio));
            }

            // #2 Gameplay: instant AoE или temporal зона
            if (_hasTemporalAoe)
            {
                // Передаём позицию взрыва в запрос зоны
                var req = _temporalAoe;
                req.Position = explosionPosition;

                ServiceLocator.Current.Get<TemporalAoEService>().Register(req);
            }
            else
            {
                
                
                EventBus<ExplosionVisualEvent>.Raise(
                    new ExplosionVisualEvent(
                        transform.position,
                        50,
                        1.2f));
                
                
                // VFX взрыва
                ServiceLocator.Current.Get<EffectRequestSystem>().Request(
                    new EffectRequest
                    {
                        Id = _aoe.VfxId,
                        Position = explosionPosition
                    },
                    EffectPriority.Normal,
                    fx => fx.gameObject.SetActive(true));
                
                ServiceLocator.Current.Get<AoEService>().Execute(_aoe);
            }
            

            // #4 Return to pool
            ReturnToPool();
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        // ─────────────────────────────────────────────
        // Helpers (без изменений)
        // ─────────────────────────────────────────────

        private void RotateAlongVelocity(Vector3 currentPosition)
        {
            Vector3 velocity = currentPosition - _lastPosition;
            if (velocity.sqrMagnitude < 0.0001f) return;

            CachedTransform.rotation = Quaternion.LookRotation(velocity.normalized);
        }

        private bool SweepCollision(Vector3 from, Vector3 to, out RaycastHit hit)
        {
            Vector3 dir = to - from;
            float distance = dir.magnitude;

            if (distance <= 0.0001f)
            {
                hit = default;
                return false;
            }

            return Physics.SphereCast(
                from,
                collisionRadius,
                dir.normalized,
                out hit,
                distance,
                Layers.Damageable,
                QueryTriggerInteraction.Ignore);
        }
    }
}