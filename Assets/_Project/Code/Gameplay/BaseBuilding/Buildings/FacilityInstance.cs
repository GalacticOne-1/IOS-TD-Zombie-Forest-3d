using Galactic1.AbstractFactory;
using Galactic1.Code.AbstractFactory;
using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.Grid;
using Galactic1.Code.Systems.Interaction;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.BuildingPanel;
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.Gameplay.Interaction;
using Galactic1.PoolObject;
using UnityEngine;
using IInteractable = Galactic1.Code.Gameplay.Interaction.IInteractable;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    public abstract class FacilityInstance : 
        _Entity, 
        IInteractable
    {
        public GameObject GetObject => gameObject;

        private InteractionPolicyService _interactionPolicyService;
        
        
        public ISceneFacility FacilityAdapter => RuntimeAdapter as ISceneFacility;
        public IUnitSceneContext SceneContext => RuntimeAdapter as IUnitSceneContext;
        
        
        public Bounds NavigationBounds
        {
            get
            {
                return new Bounds(transform.position, GetComponent<ObjectContext>().Size);
            }
        }

        
        
        protected override void OnEnable()
        {
            // отключаем коллайдеры на моделях
            var meshColliders = GetComponentsInChildren<MeshCollider>();
            var l = meshColliders.Length;
            for (var i = 0; i < l; i++)
                meshColliders[i].enabled = false;

            _interactionPolicyService = ServiceLocator.Current.Get<InteractionPolicyService>();

            ServiceLocator.Current.Get<BaseFacilityRepository>().Register(UniqueId, this);
        }

        protected override void OnDisable()
        {
            ServiceLocator.Current.Get<BaseFacilityRepository>().Unregister(UniqueId, this);

        }



        public void OnInteract()
        {
            if (!_interactionPolicyService.CanInteractWithFacilities)
                return;

            ServiceLocator.Current.Get<CameraController>().FocusOnPositionFacility(transform.position, false);
            ServiceLocator.Current.Get<FacilityPanelController>().Open(this);
        }

        

        public override void Entity_Setup<T>(T data)
        {

        }

        public virtual void Bind(ISceneFacility adapter)
        {
            Entity_Initialize(adapter);
        }
        
        
        public override void Entity_Die()
        {
            base.Entity_Die();
            
            NavigationBlocker.Unblock(NavigationBounds);
            gameObject.SetActive(false);
            
            // todo
            // 1. sound
            
            
            // VFX взрыва
            ServiceLocator.Current.Get<EffectRequestSystem>().Request(
                new EffectRequest
                {
                    Id = GameIdProvider.FacilityExplosionVfx, // беру обычный взрыв
                    Position = Tr.position
                },
                EffectPriority.Normal,
                fx => fx.gameObject.SetActive(true));
        }

    }
}