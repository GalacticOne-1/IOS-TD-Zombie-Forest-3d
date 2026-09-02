using System;
using Galactic1.Code.AbstractFactory;
using Galactic1.Code.Gameplay.Construction;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;
using Galactic1.UI.WorldStatus;
using UnityEngine;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    /// <summary>
    /// Lifecycle binder:
    /// Runtime (BaseCampFacilityRuntime) <-> SceneInstance (FacilityInstance)
    ///
    /// Owner:
    /// - bind (adapter, footprint, construction registration)
    /// - unbind
    /// - despawn
    ///
    /// Аналог SurvivorSceneBinder для зданий.
    /// </summary>
    public sealed class FacilitySceneBinder : IDisposable
    {
        // ─────────────────────────────────────────────────────────────
        // Runtime
        // ─────────────────────────────────────────────────────────────

        public IFacilityRuntime Runtime { get; }

        // ─────────────────────────────────────────────────────────────
        // Scene
        // ─────────────────────────────────────────────────────────────

        public FacilityInstance Instance { get; private set; }

        private ISceneFacility _sceneAdapter;

        public bool IsSpawned => Instance != null;

        // ─────────────────────────────────────────────────────────────
        // Dependencies
        // ─────────────────────────────────────────────────────────────

        private readonly ConstructionService _constructionService;
        private readonly WorldStatusFactory _worldStatusFactory;

        private BuildableObject _buildable;
        private GameObject _worldStatusGo;
        private EventBinding<SceneClearEvent> _sceneClearBinding;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        public FacilitySceneBinder(
            IFacilityRuntime runtime,
            ConstructionService constructionService,
            WorldStatusFactory worldStatusFactory)
        {
            Runtime = runtime;

            _constructionService = constructionService;
            _worldStatusFactory = worldStatusFactory;
        }

        public void Attach(FacilityInstance instance, FacilityModule buildItem)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (IsSpawned)
                throw new InvalidOperationException($"[{Runtime.Id}] already attached.");

            Instance = instance;

            Bind(buildItem);
        }

        private void Bind(FacilityModule buildItem)
        {
            
            _sceneAdapter = Runtime switch
            {
                IRaidFacilityRuntime damageable =>
                    new DamageableFacilitySceneAdapter(damageable),

                _ =>
                    new SceneFacilityAdapter(Runtime)
            };

            var footprint = new BuildingFootprintRuntime(
                buildItem.FootprintConfig,
                _sceneAdapter.Position,
                _sceneAdapter.Rotation);

            _buildable = Instance.GetComponent<BuildableObject>();

            switch (_buildable.Facility)
            {
                case CampFacilityInstance campBuilding:
                {
                    campBuilding.Bind(_sceneAdapter);
                    _buildable.Bind(footprint, _constructionService);

                    campBuilding.Entity_Setup(new CUnitData()
                    {
                        // todo
                    });
                }
                    break;
                
                
                case CampHQInstance campHq:
                {
                    campHq.Bind(_sceneAdapter);
                    _buildable.Bind(footprint, _constructionService);
                    campHq.Entity_Setup(new CUnitData()
                    {
                        // todo
                    });
                }
                    break;

                case FacilityDefenseInstance campDefense:
                {
                    campDefense.Bind(_sceneAdapter);
                    _buildable.Bind(footprint, _constructionService);
                    campDefense.Entity_Setup(new CUnitData()
                    {
                        // todo
                    });
                }
                    break;
            }

            _constructionService.Register(_buildable);

            if (Runtime is BaseProductionStationRuntime stationRuntime)
            {
                var view = _worldStatusFactory.Create(stationRuntime, Instance.Tr);
                _worldStatusGo = view.gameObject;

                Instance.OnDestory += DestroyWorldStatus;

                _sceneClearBinding = new EventBinding<SceneClearEvent>(DestroyWorldStatus);
                EventBus<SceneClearEvent>.Register(_sceneClearBinding);
            }
        }

        private void DestroyWorldStatus()
        {
            _worldStatusGo?.DestroyGO();
            _worldStatusGo = null;
        }

        private void Unbind()
        {
            if (_buildable != null)
            {
                _constructionService.Unregister(_buildable);
                _buildable = null;
            }

            DestroyWorldStatus();

            if (_sceneClearBinding != null)
            {
                EventBus<SceneClearEvent>.Deregister(_sceneClearBinding);
                _sceneClearBinding = null;
            }

            (_sceneAdapter as IDisposable)?.Dispose();
            _sceneAdapter = null;
        }

        public void Despawn()
        {
            if (!IsSpawned)
                return;

            var instance = Instance;

            Unbind();

            Instance = null;

            instance?.Entity_Destroy();
        }

        public void Dispose()
        {
            Despawn();
        }
    }
}