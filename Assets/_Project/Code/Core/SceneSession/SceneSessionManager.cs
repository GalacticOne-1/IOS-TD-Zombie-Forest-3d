
using System;
using Galactic1.Code.Cameras;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Runtime;
using UnityEngine;
using Galactic1.Core.GameSession;
using Galactic1.Core.Systems.Factories;

namespace Galactic1.Core.Systems.GameSession
{
    
    /// <summary>
    /// GameSessionManager — центральный класс, отвечающий за запуск игровой сессии.
    /// Аналог GameUI / Game / SceneManager из LDoE.
    /// 
    /// Он:
    /// 1) Загружает сохранение игрока
    /// 2) Создаёт игрока через PlayerFactory
    /// 3) Создаёт камеру и привязывает её к игроку
    /// 4) Загружает HUD и привязывает кнопки
    /// 5) Инициализирует менеджеры уровня (AI, спавнеры объектов/мобов/лутa)
    /// 6) Передаёт контекст уровня другим системам
    /// </summary>
    
    public abstract class SceneSessionManager : MonoBehaviour, IGameService
    {
        protected DIContainer _container;
        
        
        protected SceneSessionDefinition _session;
        public SceneSessionDefinition Session => _session;

        
        protected GameLoopSession.GameSession gameSession;

        /// После вызова очищается  
        public event Action OnSceneLoaded;
        
        
        
        public void START(DIContainer container)
        {
#if UNITY_EDITOR
            DLog.Alert("============================== [GameSession] Initializing session...", 
                EDlogColor.BLUE,
                AppConstants.show_log_core);
#endif
            
            _container = container;

            gameSession = container.Resolve<GameLoopSession.GameSession>();
            
            // ******************************************************************************************************
            // ******************************************************************************************************
            
            // для всех наследников
            Initialize(container);
            
            // корутину не трогать!!
            // зависимость для сетки
            ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(1f, () =>
            {
                OnSceneLoaded?.Invoke();
                OnSceneLoaded = null;
            });
            
            // *****************************************************************************************************
            // *** событие когда все в сцене загружено (нужно для восстановления состояний у некоторых систем или кнопок)
            // (иногда какая нибудь кнопка спавнится раньше чем подтягивается состояние,
            // как результат кнопка не отражает текущее состояние на старте)
            EventBus<SceneReadyEvent>.Raise(new SceneReadyEvent());
            EventBus<SceneActivateEvent>.Raise(new SceneActivateEvent(), true);
            EventBus<SceneUIReadyEvent>.Raise(new SceneUIReadyEvent(), true);       // <<< В КОНЦЕ !!!
        }

        protected abstract void Initialize(DIContainer container);
        protected abstract void InitializeLevelSystems();


    }
}