using Galactic1.Code.Gameplay.Noise;
using UnityEngine;

namespace Galactic1.Code.Gameplay.RaidDirector
{
    public enum DirectorState
    {
        Calm,
        Searching,
        Pressure,
        Hunting,
    }

    /// <summary>
    /// Хранит Threat и инкапсулирует всю логику его изменения.
    ///
    /// v2: методы ProcessNoise / ProcessEnemyKilled / ProcessPlayerDamaged
    /// перенесены сюда из RaidDirectorRuntime.
    /// Runtime не знает никаких весов — только вызывает методы ThreatModel.
    /// </summary>
    public sealed class DirectorThreatModel
    {
        private readonly DirectorConfig _config;

        public float Threat { get; private set; }

        public DirectorState State
        {
            get
            {
                if (Threat >= _config.ThresholdHunting) return DirectorState.Hunting;
                if (Threat >= _config.ThresholdPressure) return DirectorState.Pressure;
                if (Threat >= _config.ThresholdSearching) return DirectorState.Searching;
                return DirectorState.Calm;
            }
        }

        public DirectorThreatModel(DirectorConfig config)
        {
            _config = config;
        }

        // ── Публичный API изменения Threat ────────────────────────────

        /// <summary>Обработать NoiseEvent из NoiseSystem.</summary>
        public void ProcessNoise(NoiseEvent evt)
        {
            float weight = NoiseWeight(evt.Type);
            Add(weight * evt.Intensity);
        }

        /// <summary>Враг убит.</summary>
        public void ProcessEnemyKilled()
        {
            Add(_config.KillThreatWeight);
        }

        /// <summary>Игрок получил урон — давление уже есть, снижаем угрозу.</summary>
        public void ProcessPlayerDamaged()
        {
            Reduce(_config.PlayerDamagedThreatReduction);
        }

        /// <summary>После успешного спавна группы снижаем Threat.</summary>
        public void ProcessSpawnCommitted()
        {
            Reduce(_config.ThreatReductionAfterSpawn);
        }

        /// <summary>Натуральный распад за тик.</summary>
        public void Decay(float dt)
        {
            Reduce(_config.ThreatDecayPerSecond * dt);
        }

        // ── Приватные ─────────────────────────────────────────────────

        private void Add(float amount)
        {
            Threat = Mathf.Clamp(Threat + amount, _config.ThreatMin, _config.ThreatMax);
        }

        private void Reduce(float amount)
        {
            Threat = Mathf.Clamp(Threat - amount, _config.ThreatMin, _config.ThreatMax);
        }

        private float NoiseWeight(NoiseType type) => type switch
        {
            NoiseType.Footstep => _config.FootstepWeight,
            NoiseType.Running => _config.RunningWeight,
            NoiseType.Melee => _config.MeleeWeight,
            NoiseType.Gunshot => _config.GunshotWeight,
            NoiseType.Explosion => _config.ExplosionWeight,
            _ => 0f
        };
    }
}