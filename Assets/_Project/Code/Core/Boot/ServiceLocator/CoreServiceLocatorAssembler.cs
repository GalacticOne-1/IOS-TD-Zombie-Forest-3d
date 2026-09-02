using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Enemies.Repositories;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Gameplay.Survivors.Repositories;
using Galactic1.Code.Gameplay.Units.Repositories;
using Galactic1.Code.Gameplay.World.Repositories;
using Galactic1.Code.Systems;
using Galactic1.Code.Systems.Daily;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Code.Systems.World.Threats;
using Galactic1.Code.UI.Interaction;
using Galactic1.Code.UI.Tooltips;
using Galactic1.Code.UI.World;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Gameplay.Death;
using Galactic1.Systems;
using Galactic1.UI.Core;
using Galactic1.Localisation;
using Galactic1.PoolObject;
using Galactic1.RaidLoot.Services;
using Galactic1.Runtime.Preview;
using Galactic1.UI.CharacterPreview;
using UnityEngine;
using VisualMaterialService = Galactic1.Core.Rendering.VisualMaterialService;

namespace Galactic1
{
    public class CoreServiceLocatorAssembler : MonoBehaviour
    {
        
        [SerializeField] private SettingsManager _settingsManager;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private MonoBehaviourMaster _monoBehaviourMaster;
        [SerializeField] private CoroutineController _coroutineController;
        [SerializeField] private UIAnimationService _uiAnimationService;
        [SerializeField] private UIDetector _uiDetector;
        [SerializeField] private UIInputRouterBehaviour _uiInputRouterBehaviour;
        [SerializeField] private WorldInputDispatcher _worldInputDispatcher;
        [SerializeField] private WorldToastSystem _worldToastSystem;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private EffectRequestSystem _effectRequestSystem;

        //[SerializeField] private CameraController _cameraController;
        [SerializeField] private DeathSystem _deathSystem;
        
        
        [Space]
        [SerializeField] private PreviewService _previewService;
        [SerializeField] private UIModulePreview _uiModulePreview;
        [SerializeField] private UICharacterPreview _uiCharacterPreview;
        [SerializeField] private CharacterPreviewService _characterPreviewService;
        [SerializeField] private CharacterPortraitCache _characterPortraitCache;
        
        
        
        
        // OLD
        [Space]
        [SerializeField] private GUIAssist _guiAssist;
        [SerializeField] private ScreenGFX _screenGFX;
        [SerializeField] private ScreenGFXController _screenGFXController;
        [SerializeField] private GameMachine _gameMachine;
        
        
        
        
        /*
         *      Активация локатора для ядра приложения >>> scene CORE
         */
        public void Initialize(DIContainer container)
        {
            // NEW
            ServiceLocator.Current.Register(container.Resolve<IServerTimeSync>());
            ServiceLocator.Current.Register(container.Resolve<IConfigProvider>().Provider);
            ServiceLocator.Current.Register(_settingsManager);
            ServiceLocator.Current.Register(_monoBehaviourMaster);
            ServiceLocator.Current.Register(_coroutineController);
            ServiceLocator.Current.Register(_audioManager);
            ServiceLocator.Current.Register(_uiAnimationService);
            ServiceLocator.Current.Register(_uiDetector);
            ServiceLocator.Current.Register(_uiInputRouterBehaviour);
            ServiceLocator.Current.Register(new UIStyleResolver());
            ServiceLocator.Current.Register(new VisualMaterialService());
            ServiceLocator.Current.Register(_worldInputDispatcher);
            ServiceLocator.Current.Register(FindAnyObjectByType<FloatingTextService>());
            ServiceLocator.Current.Register(container.Resolve<UIRootView>());
            
            
            ServiceLocator.Current.Register(_worldToastSystem);
            _worldToastSystem.Prewarm();
            
            
            ServiceLocator.Current.Register(new LocalisationService());
            
            ServiceLocator.Current.Register(_uiModulePreview);
            ServiceLocator.Current.Register(_uiCharacterPreview);
            ServiceLocator.Current.Register(_previewService);
            ServiceLocator.Current.Register(_characterPreviewService);
            ServiceLocator.Current.Register(_characterPortraitCache);
            _uiModulePreview.Initialize();
            _uiCharacterPreview.Initialize();
            _characterPortraitCache.Initialize();
            
            ServiceLocator.Current.Register(container.Resolve<UIManager>());
            ServiceLocator.Current.Register(container.Resolve<TooltipController>());
            
            ServiceLocator.Current.Register(new TimeBoundaryService(container));
            //ServiceLocator.Current.Register(_cameraController);
            ServiceLocator.Current.Register(new CameraTargetGroup());
            ServiceLocator.Current.Register(_deathSystem);
            ServiceLocator.Current.Register(new GameSession());
            ServiceLocator.Current.Register(_poolManager);
            ServiceLocator.Current.Register(_effectRequestSystem);
            
            
            // repositories
            var sceneRepository = new UnitSceneRepository();
            ServiceLocator.Current.Register(sceneRepository);
            ServiceLocator.Current.Register(new SurvivorRepository(sceneRepository));
            ServiceLocator.Current.Register(new EnemyRepository(sceneRepository));
            ServiceLocator.Current.Register(new BaseFacilityRepository());
            ServiceLocator.Current.Register(new WorldObjectRepository());
            ServiceLocator.Current.Register(new LootContainerRepository());
            
            // game
            ServiceLocator.Current.Register(new GameTimeService());
            ServiceLocator.Current.Register(new WorldThreatService());
            
            // ======= Событие при смене сцены =======
            EventBus<SceneServicesResetReusableEvent>.Register(new EventBinding<SceneServicesResetReusableEvent>(_ =>
            {
                // #1 очищаем репозитории для новой сцены
                ServiceLocator.Current.Get<UnitSceneRepository>().Clear();
                ServiceLocator.Current.Get<SurvivorRepository>().Clear();
                ServiceLocator.Current.Get<EnemyRepository>().Clear();
                ServiceLocator.Current.Get<BaseFacilityRepository>().Clear();
                ServiceLocator.Current.Get<WorldObjectRepository>().Clear();
                ServiceLocator.Current.Get<LootContainerRepository>().Clear();
                
                // todo ...
            }));
            
            
            
            
            // OLD
            //ServiceLocator.Current.Register(new ConfigsProvider());
            //ServiceLocator.Current.Register(_options);
            //ServiceLocator.Current.Register(_audioCntr);
            //ServiceLocator.Current.Register(_musicManagement);
            
            
            ServiceLocator.Current.Register(_guiAssist);
            ServiceLocator.Current.Register(_screenGFX);
            ServiceLocator.Current.Register(_screenGFXController);
            
            
            ServiceLocator.Current.Register(new Bootstrap());
            ServiceLocator.Current.Register(_gameMachine);
            
        }
    }
}