using System.Collections.Generic;
using Galactic1.Code.Gameplay.AI.LOD;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Animation.Variants;
using Galactic1.Code.Gameplay.Animation.Zombie;
using Galactic1.Code.Gameplay.Noise;
using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Brain.Core;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Code.Systems.Runtime.Enemy;
using Galactic1.Code.Systems.Squad;
using Galactic1.Configs;
using Galactic1.Gameplay.Player;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Zombie
{
    public sealed class EnemyInstance : UnitInstance, INoiseListener
    {
        /// <summary>
        /// Корневой Transform для visual prefab.
        ///
        /// EnemyVisualAssembler инстанцирует VisualPrefab как дочерний объект сюда.
        /// Назначить в Inspector на gameplay prefab (Zombie_Base).
        ///
        /// Fallback: если не назначен — используется transform объекта (legacy совместимость).
        /// </summary>
        [SerializeField] private Transform _visualRoot;

        public Transform VisualRoot => _visualRoot != null ? _visualRoot : transform;

        [SerializeField] private Transform eyePoint;



        private ZombieAttackAnimationModule _attackAnim;
        private NoiseSystem _noiseSystem;

        // ── RuntimeDefinition ─────────────────────────────────────────────

        protected override UnitGameplayDefinition RuntimeDefinition
            => EnemyAdapter.Runtime.Definition;

        private EnemyRuntimeDefinition EnemyDefinition
            => EnemyAdapter.Runtime.Definition;

        public override IUnitRuntimeBase RuntimeBase
            => EnemyAdapter.RuntimeBase;

        // ── BuildStates ───────────────────────────────────────────────────

        protected override Dictionary<UnitStateId, IUnitState> BuildStates()
        {
            var def = EnemyDefinition;
            // ИЗМЕНЕНО: UtilityUnitBrain → IEnemyBrainWithBlackboard.
            // Brain может быть UtilityUnitBrain (Raid) ИЛИ SiegeUtilityBrain (Siege) —
            // оба реализуют IEnemyBrainWithBlackboard, поэтому каст работает для обоих.
            var blackboard = (Brain as IEnemyBrainWithBlackboard)?.Blackboard;

            return new Dictionary<UnitStateId, IUnitState>
            {
                { UnitStateId.Idle, new IdleState() },
                { UnitStateId.Roaming, new RoamingState(def) },
                { UnitStateId.Chasing, new ChasingState(def) },
                { UnitStateId.MeleeEngaging, new ZombieMeleeEngagingState(def, blackboard) },
                { UnitStateId.Suppressed, new SuppressedState() },
                { UnitStateId.Dying, new DyingState() },
                { UnitStateId.Dead, new DeadState() },
            };
        }

        // ── Entity_Dependency_Injection ───────────────────────────────────

        protected override void Entity_Dependency_Injection()
        {
            _noiseSystem = ServiceLocator.Current.Get<NoiseSystem>();

            var def = EnemyDefinition;
            var movement = def.MovementDefinition;
            GetComponent<UnitMover>().Setup(movement.WalkSpeed, movement.RunSpeed);

            GetComponent<ZombieAnimationVariantModule>().Initialize(
                def.Presentation.AnimationVariants,
                def.Presentation.OverrideController,
                GetAnimConfig() as ZombieAnimConfig,
                GetComponentInChildren<Animator>());

            _attackAnim = GetComponent<ZombieAttackAnimationModule>();
            _attackAnim?.Initialize(GetAnimConfig());

            // ИЗМЕНЕНО: первый аргумент — EnemyAIProfile, определяется сценарием
            // рейда (IRaidScenario.AIProfile), прокинут через EnemySpawnPipeline →
            // EnemyRuntimeFactory → EnemyRuntime.AIProfile → сюда.
            Brain = UtilityBrainFactory.Create(EnemyAdapter.Runtime.AIProfile, def);

            // base registers ISlowable and IStunnable
            base.Entity_Dependency_Injection();

            _noiseSystem.Register(this);

            StateMachine.ForceState(UnitStateId.Roaming);




            // ── Enemy gameplay capability ──────────────────────────────────

            RegisterCapability<INoiseListener>(this);
        }

        public override Transform GetEyePoint()
            => eyePoint;

        protected override BaseAnimConfig GetAnimConfig()
            => ServiceLocator.Current.Get<ConfigProvider>().Get<ZombieAnimConfig>();

        public override void Entity_Setup<T>(T data)
        {
            if (!(data is EnemyLoadData))
                Debug.LogError($"[ZombieInstance] Неверный тип данных для Setup: {data}");
        }

        public void Bind(ISceneEnemy adapter) => Entity_Initialize(adapter);

        public override void Entity_Destroy()
        {
            _noiseSystem?.Unregister(this);
            base.Entity_Destroy();
        }

        // ── INoiseListener ────────────────────────────────────────────────

        Vector3 INoiseListener.Position => transform.position;

        void INoiseListener.OnNoiseHeard(NoiseEvent evt)
        {
            // Sleeping враг не должен реагировать на шум — это часть контракта LOD.
            // Проверка тут, а не в NoiseSystem, т.к. только сам юнит знает свой уровень.
            if (CurrentSimulationLevel == SimulationLevel.Sleeping)
                return;

            // ИЗМЕНЕНО: UtilityUnitBrain → IEnemyBrainWithBlackboard
            var blackboard = (Brain as IEnemyBrainWithBlackboard)?.Blackboard;
            if (blackboard == null) return;
            if (blackboard.AlertPhase == AlertPhase.Combat) return;

            var perception = RuntimeDefinition.Perception;
            float effectiveRadius = perception.HearingRadius * perception.HearingSensitivity;

            if (Vector3.Distance(transform.position, evt.Position) > effectiveRadius) return;

            blackboard.HeardNoise = true;
            blackboard.NoisePosition = evt.Position;
            blackboard.NoiseIntensity = evt.Intensity;
            blackboard.NoiseSource = evt.Source;

            if (blackboard.AlertPhase == AlertPhase.Calm)
                blackboard.AlertPhase = AlertPhase.Suspicious;

            if (evt.Source != null && !evt.Source.IsDead)
            {
                blackboard.AggroTargetId = evt.Source.TargetId;
                blackboard.LastKnownTargetPosition = evt.Source.Position;
                blackboard.LastTimeSawTarget = Time.time;
                blackboard.AlertPhase = AlertPhase.Alerted;
            }
        }

        // ── Damage aggro ──────────────────────────────────────────────────

        public void OnDamaged(ITargetInfo attacker)
        {
            if (attacker == null || attacker.IsDead) return;

            // ИЗМЕНЕНО: UtilityUnitBrain → IEnemyBrainWithBlackboard
            var blackboard = (Brain as IEnemyBrainWithBlackboard)?.Blackboard;
            if (blackboard == null) return;

            blackboard.AggroTargetId = attacker.TargetId;
            blackboard.LastKnownTargetPosition = attacker.Position;
            blackboard.LastTimeSawTarget = Time.time;
            blackboard.AlertPhase = AlertPhase.Alerted;
        }
    }
}
