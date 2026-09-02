using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Galactic1.AbstractFactory
{
    public abstract class _Attack : MonoBehaviour
    {
        protected AttackContainer _attack;




        [Header("Настррйки оружия")]
        [SerializeField] protected CWeaponSetup weaponSetup;
        
        [Header("Настройки пули")]
        [SerializeField] protected CWeapoModeShell modeShell;
        
        [Header("Настройки эффекта")]
        [SerializeField] protected CWeapoShootFx shootFx;
        
        [Header("Стрельба очередью")]
        [SerializeField] protected CRoundReload round;
        
        [Header("Настройка звука")]
        [SerializeField] protected CAudio audio;
        
        
        

        
        [Serializable]
        public struct CWeapoModeShell
        {
            public int bullet;
            public GameObject bar;
        }
        
        [Serializable]
        public struct CWeapoShootFx
        {
            public bool use;
            public GameObject fx;
            public ParticleSystem shellParticles;
        }
        
        
        [Serializable]
        public struct CRoundReload
        {
            public bool use;
            public float time;
            public byte ammo;
            
        }

        [Serializable]
        public struct CAudio
        {
            public EWeaponAudio state;
            public int sampleShot;
        }
        
        public enum EWeaponAudio
        {
            DISABLE, SHORT_SHOT, LONG_SHOT,
        }
        

        #region TRANSITION

        [Header("Настройки состояния")]
        [SerializeField] protected CTransition transition;
        
        [Serializable]
        public struct CTransition
        {
            public float enterTime, exitTime;
            public bool useAnimation;
        }
        

        #endregion


        
        
        #region ATTACK VARIABLES


        protected CProcess process;
        
        [Serializable]
        public struct CProcess
        {
            public float reload;
            public float firerate;
            public float round;
            public float enter;

            public bool mainReload;
            public byte consAmmo;
            public byte consAmmoRound;
        }


        protected float usedDamage;
        protected GameObject cashBullet;
        protected Vector2 direction;
        

        #endregion



        #region LINK
        
        public CWeaponSetup WeaponSetup => weaponSetup;

        public void SetWeapon(CWeaponSetup weaponSetup) => this.weaponSetup = weaponSetup;
        

        #endregion
        
        
        #region CURRENT VALUES


        protected float current_reload => 0;//_attack.Entity._feature.GetAttributeWithBuff(StatId.ReloadSpeed, weaponSetup.reload);

        protected float current_dmg => 0;//_attack.Entity._feature.GetAttributeWithBuff(StatId.Damage, weaponSetup.damage);
        
            
        
        #endregion
        
        
        
        #region STATE
        
        // состояние оружия в процессе стрельбы
        private EAttackStateType STATE;

        public EAttackStateType CurrentState => STATE;

        public enum EAttackStateType
        {
            ANIMATION_START,
            ANIMATION_PROCESS,
            HIT,
            RELOAD
        }    
        
        /// <summary>
        /// For change state
        /// </summary>
        /// <param name="request"></param>
        /// <param name="client"></param>
        public void RequestState(EAttackStateType request, string client)
        {
            if(STATE != request)
            {
                _attack.Entity.Log(new CEntityDebugParam()
                {
                    Message = $">>> Weapon : Changing state [{STATE} => {request}] [client : {client}]",
                    Color = EDlogColor.YELLOW
                });
                STATE = request;
            }
            else
            {
                _attack.Entity.Log(new CEntityDebugParam()
                {
                    Message = $">>> Weapon : Try changing state [{STATE} => {request}] [client : {client}]", 
                    Color = EDlogColor.ORANGE
                });
            }
        }

        /// <summary>
        /// Когда можно безопастно выйти из атаки
        /// </summary>
        /// <returns></returns>
        public bool CanStop()
        {
            // можно выйти если основная иди мелкая перезарядка
            return process.mainReload || process.round > 0;
        }


        #endregion





        #region INITIALIZE


        /// <summary>
        /// Однократная активация (при спавне юнита)
        /// </summary>
        public void Initialize(AttackContainer _attack)
        {
            this._attack = _attack;
            RequestState(EAttackStateType.ANIMATION_START, "Attack.Initialize()");
            
            // *** сбрасываем все значения что бы оружие можно было сразу использовать
            process = new CProcess();
        }

        public void ResetState()
        {
            // ! все скрыл, потому что при новом входе в состояние атаки, не нужно обнулять здесь что-то
            // что бы запущенные перезарядк  правильно дорабатывали и не сбрасывались при переключении на новую цель !
            
            //RequestState(EAttackStateType.ANIMATION_START, "Attack.Initialize()");
            
        }

        /// <summary>
        /// Восстановление состояния после перезарядки
        /// (готовность для новой атаки)
        /// </summary>
        protected void RestoreState()
        {
            RequestState(EAttackStateType.ANIMATION_START, "Attack.Initialize()");
            process = new CProcess();
        }

        #endregion



        #region LOGIC
        
        
        /*
         *      Вход в состояние:
         * 
         *          - если требуется дождаться какой то механики (собственной анимации/звук/анимация эффекта и др)
         *            нужно установить время в transition.enterTime
         *
         *          - если атака должна запускаться сразу, то transition.enterTime = 0
         */
        protected void Enter()
        {
            //_attack.Entity._animation.VisualDirection(_attack.Entity._target.ITarget.tr.position.x - _attack.Entity.tr.position.x);
            
            // #1 ожидание для перехода в след. состояние
            if (transition.enterTime > 0)   
            {
                // звук разгона оружия (перед выстрeлом)
                //if (soundLaunch != -1)
                //ServiceLocator.Current.Get<AudioController>().SoundDelay_Attack(soundLaunch, 2, Random.Range(.8f, 1.0f));
                
                if(shootFx.use) shootFx.fx.SetActive(true);
               
                //unit.animRef.anim.ResetTrigger("Clear");

                process.enter = 0;
                _attack.Entity.Animation.AnimationToggle(_Animation.EAnimationTriggerType.Attack);
                RequestState(EAttackStateType.ANIMATION_PROCESS, "Attack.Animation_Start()");
            }
            
            // #2 без ожидания
            else 
            {
                if(shootFx.use) shootFx.fx.SetActive(true);
                
                // звук выстрела
                if(audio.state != EWeaponAudio.DISABLE)
                    ServiceLocator.Current.Get<AudioController>().SoundDelay_Attack(audio.sampleShot,2, Random.Range(.9f, 1.0f));
                
                // if (ammo == weapon.ammo)
                // {
                //     //unit.atrbRef.consGun++;
                // }
                
                //CheckMissed();
                
                // очередь
                RoundFire();
                // --------
                
                _attack.Entity.Animation.AnimationToggle(_Animation.EAnimationTriggerType.Attack);
                RequestState(EAttackStateType.HIT, "Attack.Animation_Start()");
            }
        }
        
        
        
        /*
         *      Ожидание для перехода в состояние атаки
         *      (если transition.enterTime = 0, то этот метод не будет задействован)
         *
         *      Чек цели происходит в логике юнита,
         *      если в процессе потеряна цель, вызов ForceStop() происходит из логики
         */
        protected void EnterProcess()
        {
            // *** 2 ожидание готовности
            process.enter += Time.deltaTime;
            if (process.enter > transition.enterTime)    
            {
                // *** за время ожидания цель пропала
                // для юнитов которые получают цель через логику 
                // НЕ для юнита которым управляет игрок !!!
                // if (!_attack.unit._target.Available(_attack.unit._feature.GetAttribute(StatId.AttackRange)))       
                // {
                //     DLog.Alert($"Attack : Target lost! {_attack.unit.name}", "orange");
                //     RestoreState();
                //     // ...
                //     return;
                // }
                
                
                // *** все ОК. завершение
                // if (ammo == weapon.ammo)
                // {
                //     unit.atrbRef.consGun++;
                // }
                //CheckMissed();
                //MinusAmmo();
                
                // очередь
                RoundFire();
                // --------
                
                // звук выстрела
                if(audio.state != EWeaponAudio.DISABLE)
                    ServiceLocator.Current.Get<AudioController>().SoundDelay_Attack(audio.sampleShot,2, Random.Range(.9f, 1.0f));
                
                RequestState(EAttackStateType.HIT, "Attack.EnterProcess()");
            }
        }

        /// <summary>
        /// Сброс процесса для смены состояния
        /// </summary>
        public void ForceStop()
        {
            // *** игнорируем отмену атаки и даем урон цели (для быстрых целей, что бы урон всегда проходил)
            // переход в состояние будет после атаки, к в обычном режиме
            if (_attack.fullAttack && _attack.Entity.Target.IsLive())     
            {
                _attack.Entity.Log(new CEntityDebugParam() { Message = $"Attack : Cancel process FULL!", Color = EDlogColor.ORANGE});
                return;
            }
            
            
            // *** отмена атаки 
            _attack.Entity.Log(new CEntityDebugParam() { Message = $"Attack : Cancel process!", Color = EDlogColor.ORANGE});
            if(STATE != EAttackStateType.RELOAD)
            {
                // заменил на простой переход в начальное состояние,
                // с сохранением значений оружия, что бы работала основная перезарядка и все остальное
                // т.е если пришел запрос на остановку (ForceStop), то останавливаем оружие как есть
                // при следующем входе в состояние атаки, оружие продолжает с тех значений которые были на момент остановки
                // например: не полный ammo или процесс перезарядки
                // если вызывать RestoreState() (старый вариант), то оружие всегда будет начинать как новое при потере цели
                // и основная перезарядка ни когда не будет работать
                //ResetState();  
                RequestState(EAttackStateType.ANIMATION_START, "Attack.Initialize()");
            }
            _attack.Entity._FSM.OnExitRequest();
        }

        protected abstract void Attack();

        public virtual void ForcedAttack() => Attack();
        
        
        
        public void Logic()
        {
            switch (STATE)
            {
                
                case EAttackStateType.ANIMATION_START:
                    //ScreenProfiler.Clear();
                    //ScreenProfiler.AddMessage(" ***        ATTACK      ***");
                    Enter();
                    break;
                case EAttackStateType.ANIMATION_PROCESS:
                    EnterProcess();
                    break;
                
                case EAttackStateType.HIT:

                    // * если цель потеряна, отменяем атаку
                    if (!_attack.Entity.Target.IsLive())
                    {
                        _attack.Entity.Log(new CEntityDebugParam()
                        {
                            Message = $"Unit state: {_attack.Entity.CurrentState}; Target: {_attack.Entity.Target.IsLive()}", 
                            Color = EDlogColor.YELLOW
                        });
                        return;
                    }
                    
                    Attack();
                    RequestState(EAttackStateType.RELOAD, "Attack.Logic()");

                    
                    // *** если был запрос на выход из состояния, но не смогли продолжить из-за процесса
                    // запускаем здесь, во время перезарядки, когда процесс завершен
                    if (process.mainReload || process.round > 0)
                    {
                        //if(process.isReload) Debug.LogError("Attack : OnExitRequest >> RELOAD");
                        //if(process.round > 0) Debug.LogError("Attack : OnExitRequest >> ROUND");
                        
                        _attack.Entity._FSM.OnExitRequest();
                    }
                    break;
                
                
                case EAttackStateType.RELOAD:
                    if (process.mainReload || !Reloading_FireRate()) return;
                    // ...
                    break;
                
            }
        }

        #endregion


        #region PROCESS
        
        /// <summary>
        /// Расход патронов при стрельбе
        /// </summary>
        protected void ConsumptionAmmo()
        {
            process.consAmmo++;
            if (process.consAmmo >= weaponSetup.ammo)
            {
                process.mainReload = true;
            }
        }

        void RoundFire()
        {
            if (round.use)
            {
                process.consAmmoRound++;
                if (process.consAmmoRound >= round.ammo)
                {
                    process.consAmmoRound = 0;
                    process.round = round.time + Random.Range(-.2f, .3f);
                }
            }
        }
        
        /// <summary>
        /// Перезарядка внутри магазина
        /// </summary>
        /// <returns></returns>
        protected bool Reloading_FireRate()
        {
            // #1 ожидание для очереди
            if (round.use)
            {
                process.round -= Time.deltaTime;
                if(process.round > 0) return false;
            }
            
            // #2 перезарядка выстрела
            process.firerate += Time.deltaTime;
            if (process.firerate > weaponSetup.fireRate)
            {
                process.firerate = 0;

                RequestState(EAttackStateType.ANIMATION_START, "Attack.Reloading_FireRate()");
            }
            return true;
        }
        
        
        /// <summary>
        /// Процесс перезарядки основной
        /// <br/>(Работает всегда)
        /// </summary>
        public void Reloading()
        {
            if (process.mainReload)
            {
                _attack.Entity.Log(new CEntityDebugParam() { Message = $"Attack : Reloading", Color = EDlogColor.YELLOW});
                process.reload += Time.deltaTime;
                
                // reload complete
                if (process.reload > current_reload)
                {
                    RestoreState();
                }
            }
        }

        #endregion
    }
}