using System.Collections.Generic;
using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.AI.LOD;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Animation.Zombie;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Gameplay.Units.Interfaces;
using Galactic1.Code.Gameplay.Weapon.Animation;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Gameplay.Weapons.View;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Runtime.Enemy;
using Galactic1.Code.Systems.Squad;
using Galactic1.Core.Gameplay;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Базовый класс для ВСЕХ юнитов.
    ///
    /// Изменения:
    ///   — Убран ConfigProvider и PerceptionConfig из Entity_Dependency_Injection.
    ///   — Все компоненты инициализируются через RuntimeDefinition.
    ///   — BuildStates() больше не принимает PerceptionConfig.
    ///   — Scene layer не знает об authoring layer.
    ///
    /// Правило:
    ///   UnitInstance МОЖЕТ читать RuntimeDefinition.
    ///   UnitInstance НЕ МОЖЕТ читать ScriptableObject или ConfigProvider.
    /// </summary>
    public abstract class UnitInstance : 
        _Entity, 
        ISlowable, 
        IStunnable
    {
        public UnitStateMachine StateMachine { get; private set; }
        public UnitMover Mover { get; private set; }
        public PhysicsPerception PhysicsPerception { get; private set; }
        public ICombatLogic CombatLogic { get; private set; }
        public UnitAnimationController AnimationController { get; private set; }
        public Animator Animator { get; private set; }
        public MeleeAttackComponent MeleeAttack { get; private set; }
        public AbilityComponent Ability { get; private set; }

        protected ILocomotionAnimationModule LocomotionAnimation;
        private IDeathAnimationModule _deathModule;

        public UnitReloadHandler ReloadHandler { get; protected set; }
        public WeaponSlot WeaponSlot { get; protected set; }
        public WeaponHandle CurrentWeaponHandle { get; set; }

        public IUnitBrain Brain { get; set; }
        
        protected IUnitSceneContext UnitContext => RuntimeAdapter as IUnitSceneContext;
        public ISceneUnit UnitAdapter => RuntimeAdapter as ISceneUnit;
        public ISceneEnemy EnemyAdapter => RuntimeAdapter as ISceneEnemy;
        public abstract IUnitRuntimeBase RuntimeBase { get; }
        public UnitStatus Status => UnitAdapter.Runtime.Status;
        
        /// <summary>
        /// Текущий уровень AI-симуляции, назначаемый ИЗВНЕ (AILODSystem).
        /// UnitInstance никогда сам не решает, какой уровень ему нужен —
        /// только исполняет команду и гейтит собственный Tick.
        /// </summary>
        public SimulationLevel CurrentSimulationLevel { get; private set; } = SimulationLevel.Full;
        // Упрощённая Low-LOD частота. В будущем можно прокинуть сюда
        // значения из AILODConfig (LowBrainThinkInterval/LowPerceptionInterval),
        // сейчас — намеренное упрощение по ТЗ ("may temporarily behave like Full").
        private float _lodAccumulator;
        private const float LowLodTickInterval = 1f;
        

        /// <summary>
        /// Runtime definition текущего юнита.
        /// Реализуется в подклассе:
        ///   SurvivorInstance → UnitAdapter.Runtime.Definition
        ///   ZombieInstance   → EnemyAdapter.Runtime.Definition
        /// </summary>
        protected abstract UnitGameplayDefinition RuntimeDefinition { get; }
        
        
        
        private readonly Dictionary<object, float> _slowModifiers = new();

        // ── Entity_Dependency_Injection ───────────────────────────────────

        protected override void Entity_Dependency_Injection()
        {
            var def = RuntimeDefinition; // единственный источник данных
            var animCfg = GetAnimConfig();

            // 1. Perception ─────────────────────────────────
            PhysicsPerception = GetComponent<PhysicsPerception>();
            PhysicsPerception.Initialize(
                def.Perception,
                GetEyePoint(),
                Layers.Detectable,
                Layers.Occlusion);

            // 2. Combat logic ─────────────────────────────────
            var combatLogic = GetComponent<UnitCombatLogic>();
            combatLogic.Initialize(PhysicsPerception, WeaponSlot, UnitContext);
            CombatLogic = combatLogic;

            // 3. Mover + Animator
            Mover = GetComponent<UnitMover>();
            Animator = GetComponentInChildren<Animator>();

            // 4. Animation ─────────────────────────────────
            var animController = GetComponent<UnitAnimationController>();
            animController.Initialize(animCfg);
            AnimationController = animController;

            LocomotionAnimation = GetComponent<ILocomotionAnimationModule>();
            LocomotionAnimation?.Initialize(animCfg, def);

            _deathModule = GetComponent<IDeathAnimationModule>();
            _deathModule?.Initialize(animCfg);

            // 5. Melee ─────────────────────────────────
            var weaponRigController = GetComponent<WeaponRigController>();
            Transform hitOrigin = weaponRigController ? weaponRigController.HitOrigin : transform;

            MeleeAttack = new MeleeAttackComponent(
                UnitContext,
                hitOrigin,
                Layers.Damageable,
                def.MeleeCombat);

            // 6. Ability ─────────────────────────────────
            Ability = new AbilityComponent();

            var animHandler = GetComponentInChildren<CombatAnimHandler>();
            animHandler.Bind(MeleeAttack);
            animHandler.Bind(Ability);

            // 7. FSM ─────────────────────────────────
            StateMachine = new UnitStateMachine();
            var states = BuildStates();
            StateMachine.Initialize(this, states, UnitStateId.Idle);

            // 8. Brain ─────────────────────────────────
            if (Brain == null)
            {
                Debug.LogError($"[UnitInstance] {name}: Brain не назначен до вызова base!");
                return;
            }
            
            // 9. target info ─────────────────────────────────
            var targetInfoBase = GetComponent<TargetInfoBase>();
            targetInfoBase.Initialize(UnitContext);
            GetComponent<TargetInfoProxy>().Bind(targetInfoBase);
            // подключаем все хитбоксы на префабе
            var hitboxProxy = GetComponentsInChildren<HitboxProxy>();
            var l = hitboxProxy.Length;
            for (int i = 0; i < l; i++)
                hitboxProxy[i].Bind();
            // ──────────────────────────────────────────────────────────────────
            // ──────────────────────────────────────────────────────────────────
            
            
            Brain.Initialize(this);
            StateMachine.OnStateChanged += Brain.OnStateChanged;
            
            
            // Gameplay capabilities only ─────────────────────────────────
 
            RegisterCapability<ISlowable>(this);
            RegisterCapability<IStunnable>(this);
        }

        protected abstract BaseAnimConfig GetAnimConfig();

        public virtual Transform GetEyePoint() => transform;

        /// <summary>
        /// Подкласс возвращает FSM states.
        /// Не принимает PerceptionConfig — данные берутся из RuntimeDefinition.
        /// </summary>
        protected abstract Dictionary<UnitStateId, IUnitState> BuildStates();

        // ── Update ────────────────────────────────────────────────────────
        
        public override void UpdateM()
        {
            if (disable) return;

            switch (CurrentSimulationLevel)
            {
                case SimulationLevel.Sleeping:
                    return; // полная заморозка — ничего не тикаем вообще

                case SimulationLevel.Low:
                    TickLow(Time.deltaTime);
                    return;

                default: // Full — поведение как сегодня, 1:1
                    TickFull(Time.deltaTime);
                    return;
            }
        }

        private void TickFull(float dt)
        {
            CurrentWeaponHandle?.Entity?.Tick(dt);
            MeleeAttack?.Tick(dt);
            PhysicsPerception.Tick();      // 1. свежий snapshot
            StateMachine.Tick(dt);         // 2. FSM
            Brain?.Tick(dt);               // 3. AI думает
            ReloadHandler?.Tick(dt);
            LocomotionAnimation?.Tick();
        }

        private void TickLow(float dt)
        {
            // Combat/Reload/LocomotionAnimation намеренно не тикаются на Low —
            // Animator всё равно выключен, а Combat отключён по ТЗ.
            _lodAccumulator += dt;
            if (_lodAccumulator < LowLodTickInterval) return;

            float elapsed = _lodAccumulator;
            _lodAccumulator = 0f;

            PhysicsPerception.Tick();
            StateMachine.Tick(elapsed);
            Brain?.Tick(elapsed);
        }

        
        #region Capabilities
        
        
        public void ApplySlow(object source, float speedMultiplier)
        {
            if (Mover == null)
                return;

            _slowModifiers[source] = speedMultiplier;

            RecalculateSlow();
        }

        public void RemoveSlow(object source)
        {
            if (_slowModifiers.Remove(source))
                RecalculateSlow();
        }
        
        private void RecalculateSlow()
        {
            if (Mover == null)
                return;

            if (_slowModifiers.Count == 0)
            {
                Mover.ClearSlowOverride();
                return;
            }

            float strongestSlow = 1f;

            foreach (var kv in _slowModifiers)
            {
                if (kv.Value < strongestSlow)
                    strongestSlow = kv.Value;
            }

            Mover.SetSlowOverride(
                Mover.WalkSpeed * strongestSlow,
                Mover.RunSpeed * strongestSlow);
        }

        public void ApplyStun(float duration)
        {
            StateMachine.Execute(new StunCommand(duration));
        }


        #endregion
        
        
        
        
        /// <summary>
        /// Единственная точка входа для смены уровня симуляции.
        /// Вызывается ТОЛЬКО AILODSystem.
        /// </summary>
        public void SetSimulationLevel(SimulationLevel level)
        {
            if (CurrentSimulationLevel == level) return;

            var previous = CurrentSimulationLevel;
            CurrentSimulationLevel = level;

            switch (level)
            {
                case SimulationLevel.Sleeping:
                    EnterSleeping();
                    break;
                case SimulationLevel.Low:
                    EnterLow(previous);
                    break;
                case SimulationLevel.Full:
                    EnterFull(previous);
                    break;
            }
        }

        private void EnterSleeping()
        {
            Mover?.Sleep();
            PhysicsPerception?.Sleep();
            if (Animator != null) Animator.enabled = false;

            // Brain/StateMachine/MeleeAttack/ReloadHandler/LocomotionAnimation
            // не получают явного Sleep() — они просто перестают тикаться
            // из UpdateM() ниже. Так проще: не плодим Sleep/Wake на каждой
            // мелкой системе, а гейтим один раз в корне.
        }

        private void EnterLow(SimulationLevel previous)
        {
            if (previous == SimulationLevel.Sleeping)
            {
                Mover?.Wake();
                PhysicsPerception?.Wake();
            }

            if (Animator != null) Animator.enabled = false;
            _lodAccumulator = 0f;
        }

        private void EnterFull(SimulationLevel previous)
        {
            if (previous == SimulationLevel.Sleeping)
            {
                Mover?.Wake();
                PhysicsPerception?.Wake();
            }

            if (Animator != null) Animator.enabled = true;
        }
        
        
        
        // ── Death ─────────────────────────────────────────────────────────

        public virtual void HandleDeath()
        {
            StateMachine.ForceState(UnitStateId.Dying);
        }

        public override void Entity_Die()
        {
            base.Entity_Die();
            Mover.Die();

            if (_deathModule == null)
                AnimationController.PlayDeath();
            else 
                _deathModule.PlayDeath();
        }

        public void AE_DeathComplete()
            => StateMachine.ForceState(UnitStateId.Dead);

        public override void Entity_Destroy()
        {
            StateMachine.OnStateChanged -= Brain.OnStateChanged;
            Brain?.Dispose();
            base.Entity_Destroy();
        }
    }
}