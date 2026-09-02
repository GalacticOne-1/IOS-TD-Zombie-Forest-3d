using UnityEngine;

namespace Galactic1.PoolObject
{
    /// <summary>
    /// Пример: визуальный эффект с конфигом.
    /// </summary>
    public class EffectPoolable : PoolableMonoBehaviour, IPoolItemConfig<EffectConfig>
    {
        [SerializeField] private ParticleSystem _particles;

        private EffectConfig _config;

        // ── IPoolItemConfig ───────────────────────────
        // Вызывается один раз при Instantiate из ObjectPool
        public void SetConfig(EffectConfig config)
        {
            _config = config;
            SetDuration(config.ObjectPoolParam.Duration);
        }

        public void Setup(float duration) => SetDuration(duration);

        // ── IPoolable overrides ───────────────────────
        public override void OnCreate()
        {
            // доп. кэш если нужен
        }

        public override void OnSpawn()
        {
            _particles.Play();
            base.OnSpawn(); // активирует GO и запускает авто-возврат
        }

        public override void OnDespawn()
        {
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            base.OnDespawn(); // деактивирует GO и останавливает корутину
        }

        public override void ResetState()
        {
            base.ResetState();
            // сброс специфичных полей если нужно
        }
    }
}