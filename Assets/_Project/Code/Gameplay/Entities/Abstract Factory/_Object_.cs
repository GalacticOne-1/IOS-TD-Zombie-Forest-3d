
using System;
using Galactic1.Code.AbstractFactory;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.AbstractFactory
{
    // любой объект на уровне (юнит, здание и тд)
    // наследование от этого класса
    public abstract class _Object_ : MonoBehaviour
    {
        #region LINK

        public string UniqueId { get; set; }
        
        // расположение созданного объекта в своем []
        public int Id { set; get; }
        
        public _UnitInterface.EGameSide Team { get; private set; }

        public void SetTeam(_UnitInterface.EGameSide team) => Team = team;

        #endregion
        
        
        

        #region ISceneEntity
        
        
        // true - юнит запущен в действие
        public bool IsActive { get; set; }
        public Transform Tr { get; set;}
        public GameObject GameObject { get; set;}
        
        public bool TryGet<T>(out T component) where T : class
        {
            component = GetComponent<T>();
            return component != null;
        }
        
        #endregion
        

        public Vector3 CenterRadius => Tr.position + visualPosition;
        private Vector3 visualPosition;

        // events
        public Action<float> OnDamage;
        
        /// Сразу когда хп <= 0 
        public Action<_Object_> OnDeath;
        
        /// После анимации смерти, когда объект отключается и возвращается в пул
        public Action<_Object_> OnDeactivate;
        public Action OnDestory;
        
        
        
        


        public void DebugLog(CEntityDebugParam param)
        {
            if (param.ShowLogs || param.OnlyThisEntity)
            {
                var mes = $"{gameObject.name}\n"+param.Message;
                DLog.Alert(mes, param.Color, (byte)Team, param.OnlyThisEntity);
            }
        }
        
        


        #region STATE
        
        // для блокировки запроса на новое состояние
        public bool RequestedTransition { get; private set; }

        public EUnitStateType STATE { get; private set; }

        /// <summary>
        /// Сброс состояние
        /// </summary>
        public void ResetState()
        {
            RequestedTransition = false;
            STATE = EUnitStateType.IDLE;
        }

        private DFunc onStateTransitionComplete;

        /// <summary>
        /// Вызывать при состоявшемся переходе
        /// <br/>(нужно что бы не было расхождений с логикой)
        /// </summary>
        public void OnStateTransitionComplete() => onStateTransitionComplete?.Invoke();
        
        
        /// <summary>
        /// For change state
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        public void RequestState(EUnitStateType request, string client, DFunc onComplete)
        {
            if(!RequestedTransition && STATE != request)
            {
                DebugLog(new CEntityDebugParam()
                {
                    Message = $"Oject : Request state [{STATE} => {request}] [client : {client}]",
                    Color = EDlogColor.YELLOW
                });
                
                // *** меняем состояние и убираем блокировку когда переход завершится
                onStateTransitionComplete = onComplete;
                onStateTransitionComplete += () =>
                {
                    RequestedTransition = false;
                    STATE = request;
                };
                
                RequestedTransition = true;
                SetStateHandler(request);           // сам запрос на смену состояния
            }
            else
            {
                DebugLog(new CEntityDebugParam()
                {
                    Message = $"Oject : Try changing state [{STATE} => {request}] [client : {client}]",
                    Color = EDlogColor.ORANGE
                });
            }
        }

        /// <summary>
        /// Для смены обработчика состояния
        /// </summary>
        /// <param name="state"></param>
        protected abstract void SetStateHandler(EUnitStateType state);
        

        #endregion





        #region INITIALIZE
        
        protected virtual void OnEnable() {}
        protected virtual void OnDisable() {}

        public virtual void Awake()
        {
            Tr = transform;
            GameObject = gameObject;

            var v = transform.Find("VisualRoot");
            if (v)
                visualPosition = v.localPosition;
            else
                Debug.LogError($"Entity {name} dont find Visual object");
        }

        

        /*
         *      Порядок вызова методов для активации сущности :
         *      1 - Initialize();
         *      2 - Initialize<T>(T data);
         *      3 - Activate();
         */

        /// <summary>
        /// Создание классов управления и состояния
        /// </summary>
        public abstract void Entity_Initialize(ISceneEntityRuntime runtime);

        /// <summary>
        /// Внедрение доп. зависимостей для наследников от _Unit
        /// </summary>
        protected virtual void Entity_Dependency_Injection(){}
        
        /// <summary>
        /// Только для загрузки значений (перед активацией)
        /// <br/>(здесь не должно быть добавлений компонентов и пр)
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public abstract void Entity_Setup<T>(T data);

        /// <summary>
        /// Добавление модуля атаки
        /// </summary>
        public virtual void Attack_Initialize(){}
        
        /// <summary>
        /// Запускает юнит
        /// </summary>
        public abstract void Entity_Activate();
        
        /// <summary>
        /// Отключает cущность
        /// </summary>
        public abstract void Entity_Deactivate(bool desableObject);
        
        /// <summary>
        /// Включение для геймплея
        /// </summary>
        public virtual void Entity_Enable(){}
     
        /// <summary>
        /// Отключение для геймплея
        /// </summary>
        public virtual void Entity_Disable(){}

        #endregion
        
        
        /// <summary>
        /// Безопасно удаляет cущность из сцены
        /// </summary>
        public abstract void Entity_Destroy();

        /// <summary>
        /// Для возврата в пул
        /// </summary>
        public abstract void Entity_ToPool();
        
        /// <summary>
        /// Вызывать для убийства cущности
        /// <br/>(Остается в сцене, объект отключается)
        /// </summary>
        public abstract void Entity_Die();
    
       
        /// <summary>
        /// Сброс состояния для нового спавна
        /// </summary>
        public abstract void Entity_Reset(bool desableObject);
        
        


        

    }

    
}