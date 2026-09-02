using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.AbstractFactory;
using Galactic1.Code.Core.Lifecycle;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.Raid;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public struct EntityOption
    {
        public bool isDetectable;
        public bool useGravity;
    }
    
    
    public abstract class _Entity : 
        _Object_, 
        IUpdate, 
        ISceneEntity
    {
        public ItemConfig ItemConfig { get; set; }
        
        /// <summary>
        /// Своя рантайм модель (GameloopContext)
        /// </summary>
        public ISceneEntityRuntime RuntimeAdapter { get; private set; }
        
        private bool _initialized;
        
        private Rigidbody rb;
        
        private IEntityProxy[] _proxies;

        public Transform UIAnchor { get; private set; }
        
        
        // ── Capability Map ────────────────────────────────────────────────

        protected EntityOption _entityOption = new()
        {
            isDetectable = true,    // сузность доступна для обнаружения
            useGravity = true,
        };
 
        /// <summary>
        /// Gameplay capability registry.
        ///
        /// IMPORTANT:
        /// Only gameplay contracts/interfaces should be registered here.
        /// Never register internal implementation details
        /// such as Animator, FSM, UnitMover, etc.
        /// </summary>
        private readonly Dictionary<Type, object> _capabilities = new();
 
        /// <summary>
        /// Registers a gameplay capability under key type T.
        /// Safe to call multiple times — later registration overwrites earlier one.
        /// ONLY call during Entity_Dependency_Injection or its callee chain.
        /// </summary>
        protected void RegisterCapability<T>(T component) where T : class
        {
            if (component == null)
            {
                Debug.LogWarning($"[{name}] RegisterCapability<{typeof(T).Name}> skipped: null.");
                return;
            }
            _capabilities[typeof(T)] = component;
        }
 
        /// <summary>
        /// Returns the capability registered under T.
        /// Never falls back to GetComponent — pure dictionary lookup.
        /// </summary>
        public bool TryGetCapability<T>(out T component) where T : class
        {
            if (_capabilities.TryGetValue(typeof(T), out var raw))
            {
                component = raw as T;
                return component != null;
            }
            component = null;
            return false;
        }
        
        #region TRANSITION

        [SerializeField] private CTransition[] _transitions; 
        [Serializable] 
        public struct CTransition
        {
            public EUnitStateType stateType;
            public CStateTransition setup;
        }

        #endregion
        
        
        
        

        #region ACCESS

        public _UnitInterface UnitInterface { get; private set; }

        public FSM _FSM { get; private set; }
        public _AI AI { get; private set; }
        public _Target Target { get; private set; }
        
        // _UI _ui { get; private set; }        << отдельный класс для ui юнита
        
        public _Animation Animation { get; private set; }
        
        public UnitSpritesContainer SpritesContainer { get; private set; }
        public AttackContainer AttackContainer { get; private set; }
        
        

        #endregion

        
        
        
        

        #region ITarget

        public EUnitStateType CurrentState => STATE;

        public bool IsLive => base.IsActive && STATE != EUnitStateType.DIE;

        public GameObject Obj => gameObject;


        /// координаты для атаки по объекту (ground/air)
        public virtual Vector3 HitCoord() => base.Tr.position; // todo

        #endregion
        
        
        
        
        

        #region STATE HANDLER

        /// <summary>
        /// Находит настройки перехода для состояния
        /// <br/>(если нет, передаст пустую структуру)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public CStateTransition FindTransitionSetup(EUnitStateType type)
        {
            var l = _transitions.Length;
            for (int i = 0; i < l; i++)
            {
                if (_transitions[i].stateType == type)
                    return _transitions[i].setup;
            }

            return new CStateTransition();
        }
        
        protected override void SetStateHandler(EUnitStateType newState)
        {
            _FSM.ChangeState(newState);
        }


        #endregion







        #region ENTITY >> INITIALIZING


        /*
         *      ! Не переписывать этот метод !
         *          - для добавления отличных зависимостей использовать Entity_Initialize_Additing
         */
        public override void Entity_Initialize(ISceneEntityRuntime adapter)
        {
            if (_initialized)
            {
                Debug.LogError($"[{name}] already initialized.");
                return;
            }
            _initialized = true;
            
            RuntimeAdapter = adapter;
            UnitInterface = GetComponent<_UnitInterface>();
            SetTeam(UnitInterface.Team);
            UIAnchor = GetComponentInChildren<UnitUIAnchor>()?.transform ?? transform;
            
            
            // #1
            _proxies = GetComponentsInChildren<IEntityProxy>(true)
                .OrderBy(p => p.Priority)
                .ToArray();
            
            rb = GetComponent<Rigidbody>();
            
            
            // *** initialize controllers 
            // контроллеры зависят от установок в интерфейсе

            //  *** LOGIC ************************************************************************************
            // * пустая логика
            // if (!UnitInterface.RequestLogic)
            // {
            //     AI = new _AI_Null(this);
            // }
            // // * логика для активных сущностей
            // else
            // {
            //     AI = UnitInterface.Team == _UnitInterface.EGameSide.Player ? new _AI_Player_v1(this) : new _AI_Enemy_v1(this);
            // }
            // ************************************************************************************************
            
            //Target = UnitInterface.Team == _UnitInterface.EGameSide.Player ? new _Target_Player(this) : new _Target_Enemy(this);
            //Hp = UnitInterface.Team == _UnitInterface.EGameSide.Player ? new PlayerHP(this) : new EnemyHp(this);
            //GetComponentInChildren<HitboxProxy>()?.Bind(UnitAdapter);
            var receiver = GetComponent<DamageReceiverProxy>();
            if (receiver != null)
            {
                receiver.Bind(this);
                if (RuntimeAdapter is IUnitSceneContext unit)
                    receiver.Bind(unit);
            }
            
            Animation = new _Animation(this);
            //SpritesContainer = GetComponentInChildren<UnitSpritesContainer>();
            
            
            // для атакующих юнитов
            // if(UnitInterface.RequestState.attack)
            // {
            //     AttackContainer = GetComponentInChildren<AttackContainer>();
            //     AttackContainer.SetupFullAttack(UnitInterface.FullAttack);
            //     Attack_Initialize();
            // }
            
            Entity_Dependency_Injection();
            
            
            // *** initialize state >> IDLE
            // _FSM = new FSM(this);
            // LoadState();
            // ResetState();
            // _FSM.Initialize(EUnitStateType.IDLE);
            // ***
            
            
            //
        }
        
        /// <summary>
        /// Вызывать для активации сущности (get pool)
        /// </summary>
        public override void Entity_Activate()
        {
            //
            gameObject.SetActive(true);
            IsActive = true;
            disable = false;
            SetRigidbodyGravity(true);
            GetComponent<UnitHitboxController>()?.SetEnabled(_entityOption.isDetectable);
            
            for (int i = 0; i < _proxies.Length; i++)
                _proxies[i].Register();
            

            // ***      вконце запускаем update      ***
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
            DLog.Alert("------- Entity_Activate "+gameObject.name);
        }
        
        /// <summary>
        /// Вызывать для отключения сущности (return to pool)
        /// </summary>
        /// <param name="desableObject"></param>
        public override void Entity_Deactivate(bool desableObject)
        {
            // * сначалa отключаем от update
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
            // **************
            
            // потом остальное
            IsActive = false;
            disable = true;
            SetRigidbodyGravity(false);
            GetComponent<UnitHitboxController>()?.SetEnabled(false);
            
            for (int i = 0; i < _proxies.Length; i++)
                _proxies[i].Unregister();
            
            GetComponent<TargetInfoProxy>()?.Unregister();
            GetComponent<HitboxProxy>()?.Unregister();

            // * отключаем объект
            gameObject.SetActive(!desableObject);
            DLog.Alert("------- Entity_Deactivate "+gameObject.name);
        }

        #endregion



        #region ENTITY >> DEACTIVATING

        public override void Entity_Destroy()
        {
            Entity_Deactivate(true);
            OnDestory?.Invoke();
            Destroy(gameObject);
        }

        public override void Entity_ToPool()
        {
            if(gameObject.activeSelf)
            {
                Entity_Deactivate(true);
                OnDeactivate?.Invoke(this);
            }
        }

        // все юинты должы использовать base.Die();
        public override void Entity_Die()
        {
            // отключаем обнаружение объекта
            disable = true;
            SetRigidbodyGravity(false);
            GetComponent<UnitHitboxController>()?.SetEnabled(false);
            
            // var energyConsumer = GetComponent<IEnergyConsumer>();
            // if (energyConsumer != null)
            //     energyConsumer.ForcedDisconnect();
            
            // событие для других юнитов о смерти этого
            // ...
            
            // переводим в состояние смерти
            //RequestState(EUnitStateType.DIE, "Unit.Die()", null);
            OnDeath?.Invoke(this);
        }
        
        
        // делаем сброс всех элементов, что бы юнит был как новый
        public override void Entity_Reset(bool desableObject)
        {
            if (desableObject)
                gameObject.SetActive(false);
            
            if (IsActive)
            {
                //Fsm = FSM.die;
                //attackRef.Clear();
                //hpRef.CMD_DEACTIVATE();
                //animRef.SetAnim(FSM.idle);
                Entity_Deactivate(desableObject);
            }

            
            ResetAnim();
            // sounds
            // ...
        }

        /// <summary>
        /// Перевод анимации в начальное состояние
        /// </summary>
        void ResetAnim()
        {
            if(Animation != null && Animation.animator)
            {
                GetComponent<UnitAnimationController>().Restore();
            }
        }


        #endregion




        #region UPDATE


        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }
        
        
        protected bool disable;
        public virtual void UpdateM()
        {
            if (disable) return;
            
            Log(new CEntityDebugParam() { Message = $"Update Logic" });
        }
        
        

        #endregion




        void SetRigidbodyGravity(bool y)
        {
            if (_entityOption.useGravity && rb != null)
                rb.useGravity = y;
        }




        public void Log(CEntityDebugParam param)
        {
            if (UnitInterface.ShowLogs || UnitInterface.OnlyThisLogs)
            {
                param.ShowLogs = UnitInterface.ShowLogs;
                param.OnlyThisEntity = UnitInterface.OnlyThisLogs;
                DebugLog(param);
            }
        }
        
    }
    
    
    public interface ITarget
    {
        _UnitInterface.EGameSide Team { get; }
        EUnitStateType CurrentState { get; }
        
        bool Activated { get; }
        bool IsLive { get; }

        Transform tr { set; get; }
        GameObject Obj { get; }
        Vector3 CenterRadius { get; }
        Vector3 HitCoord();

    }
}