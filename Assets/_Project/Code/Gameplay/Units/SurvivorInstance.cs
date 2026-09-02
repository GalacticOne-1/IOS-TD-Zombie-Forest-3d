using System.Collections.Generic;
using Galactic1.AbstractFactory;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Animation.Player;
using Galactic1.Code.Gameplay.Equipment_Preview;
using Galactic1.Code.Gameplay.Units.Definitions;
using Galactic1.Code.Gameplay.Units.States.Survivor;
using Galactic1.Code.Gameplay.Weapon.Animation;
using Galactic1.Code.Gameplay.Weapons.Infrastructure;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Code.Gameplay.Weapons.View;
using Galactic1.Code.Scene.Units;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.UI.Equipment;
using Galactic1.Configs;
using Galactic1.Gameplay.Player;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    public class SurvivorInstance : UnitInstance, ISquadMember
    {
        public Vector3 DesiredPosition { get; set; }
        public IAmmoInventory AmmoInventory { get; private set; }
        public IOwnerStatsProvider StatsProvider { get; private set; }
        public WeaponRigController WeaponRigController { get; private set; }
        public MarineReactiveAI ReactiveAI { get; private set; }

        private WeaponRuntimeBinder _weaponBinder;
        private EquipmentContainer _equipmentContainer;
        private PlayerCommandBrain _playerBrain;

        // ── RuntimeDefinition ─────────────────────────────────────────────

        protected override UnitGameplayDefinition RuntimeDefinition
            => UnitAdapter.Runtime.Definition;

        private SurvivorGameplayDefinition PlayerDefinition
            => UnitAdapter.Runtime.Definition;

        public override IUnitRuntimeBase RuntimeBase
            => UnitAdapter.RuntimeBase;

        // ── BuildStates ───────────────────────────────────────────────────

        protected override Dictionary<UnitStateId, IUnitState> BuildStates()
        {
            return new Dictionary<UnitStateId, IUnitState>
            {
                { UnitStateId.Idle, new IdleState() },
                { UnitStateId.SquadMoving, new SquadMovingState() },
                { UnitStateId.MeleeEngaging, new MeleeEngagingState(600) },
                { UnitStateId.Engaging, new EngagingState() },
                {
                    UnitStateId.UsingAbility, 
                    new UsingAbilityState(ServiceLocator.Current.Get<AbilityUseCoordinator>())
                },
                { UnitStateId.Suppressed, new SuppressedState() },
                { UnitStateId.Dying, new DyingState() },
                { UnitStateId.Dead, new DeadState() },
            };
        }

        // ── Entity_Dependency_Injection ───────────────────────────────────

        protected override void Entity_Dependency_Injection()
        {
            var configProvider = ServiceLocator.Current.Get<ConfigProvider>();
            
            _entityOption = new EntityOption()
            {
                isDetectable = !(RuntimeBase as IUnitRuntime).IsCampDefender,
                useGravity = true
            };

            WeaponRigController = GetComponent<WeaponRigController>();
            WeaponSlot = GetComponent<WeaponSlot>();
            _weaponBinder = GetComponent<WeaponRuntimeBinder>();
            ReactiveAI = GetComponent<MarineReactiveAI>();

            _playerBrain = new PlayerCommandBrain(PlayerDefinition.BrainDefinition);
            Brain = _playerBrain;

            GetComponent<PlayerWeaponAnimationModule>().Initialize();
            GetComponent<PlayerAttackAnimationModule>().Initialize(configProvider.Get<PlayerAnimConfig>());
            GetComponent<IAbilityAnimationModule>().Initialize(configProvider.Get<PlayerAnimConfig>());
            GetComponent<ICombatAnimationModule>().Initialize(configProvider.Get<PlayerAnimConfig>());

            // base registers ISlowable and IStunnable
            base.Entity_Dependency_Injection();

            ReactiveAI.BindBrain(_playerBrain);
            ReloadHandler = new UnitReloadHandler(this);
            UnitAdapter.Runtime.OnAbilityAnimationRequested += AnimationController.OnAbilityAnimation;

            
            
            
            // ── Survivor gameplay capability ───────────────────────────────

            RegisterCapability<ISquadMember>(this);
        }

        protected override BaseAnimConfig GetAnimConfig()
            => ServiceLocator.Current.Get<ConfigProvider>().Get<PlayerAnimConfig>();

        public override Transform GetEyePoint()
            => WeaponRigController.HitOrigin;

        public override void Entity_Setup<T>(T data)
        {
            if (data is PlayerLoadData loadData)
            {
                _weaponBinder.Initialize(loadData.UnitRuntime);
                AmmoInventory = new InventoryAmmoAdapter(loadData.InventoryPort, GameContent.Ammo);
                GetComponent<WeaponAnimSwitcher>().AnimLibrary = loadData.AnimLibrary;
                _equipmentContainer = GetComponent<EquipmentContainer>();
                _equipmentContainer.BindSource(
                    UnitAdapter.EquipmentStatsProvider,
                    new EquipmentVisualHandler());
            }
            else
            {
                Debug.LogError($"[SurvivorInstance] Неверный тип данных для Setup: {data}");
            }
        }


        public void Bind(ISceneUnit adapter)
        {
            Entity_Initialize(adapter);
            if (UnitAdapter == null)
                Debug.LogError($"[SurvivorInstance] Scene adapter is not ISceneUnit.");
        }

        public override void Entity_Die()
        {
            base.Entity_Die();
            GetComponent<WeaponAnimSwitcher>().Die();
            WeaponRigController.DetachWeapon();
        }
        
        
        public void StopSquadMovement()
        {
            Mover.Stop();
            StateMachine.TransitionTo(UnitStateId.Idle, null);
        }
    }
}