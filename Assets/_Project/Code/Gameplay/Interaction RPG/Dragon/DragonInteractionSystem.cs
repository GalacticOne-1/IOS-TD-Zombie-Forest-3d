using System;
using Galactic1.AbstractFactory;
using Galactic1.Gameplay.Control;
using Galactic1.Repository;
using R3;
using UnityEngine;

namespace Galactic1.Gameplay.Interaction
{
    public class DragonInteractionSystem : MonoBehaviour, IGameService, IUpdate
    {
        private float registryScanInterval = 0.15f;
        
        private Transform _player;
        private Transform _dragon;
        
        private float _nextScanTime;


        public ReactiveProperty<bool> OnDetectDragon = new();
        public ReactiveProperty<bool> OnDetectGround = new();



        
        
        public void Initialize()
        {
            _player = ((_Object_)ServiceLocator.Current.Get<PlayerRepository>().GetController).Tr;
            //_dragon = ServiceLocator.Current.Get<DragonRepository>().GetController._groundDetector.tr;
            
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
            
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
            {
                OnDetectDragon = new();
                OnDetectGround = new();
                IUpdateClear();
            }));
            
        }
        
        
        
        
        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        public void UpdateM()
        {
            if (Time.time >= _nextScanTime)
            {
                _nextScanTime = Time.time + registryScanInterval;
                DetectDragon();
            }
        }


        public void DetectDragon()
        {
            if (!ControllableSwitcher.IsDragon)
            {
                OnDetectDragon.OnNext(_player.position.Distance(_dragon.position) < AppConstants.MAX_DISTANCE_TO_DRAGON);
            }
        }
        
        public void DetectGround(bool isGround)
        {
            if (ControllableSwitcher.IsDragon)
            {
                OnDetectGround.OnNext(isGround);
            }
        }

        
    }
}