using System;
using Galactic1.Gameplay.Player;
using UnityEngine;
using R3;
using UnityEngine.UI;

namespace Galactic1.Core.UI.HUD
{
    /// <summary>
    /// Handles UI icon visibility for hunger/thirst warnings based on SurvivalController events.
    /// </summary>
    public class SurvivalIcons : MonoBehaviour, IUpdate
    {
        [SerializeField] private Image hungerIcon;
        [SerializeField] private Image thirstIcon;

        private Camera mainCamera;
        private Vector3 _offset;
        private Transform _target;
        
        
        // public void Initialize(SurvivalController survivalController, Vector2 offset)
        // {
        //     mainCamera = Camera.main;
        //     _target = survivalController.transform;
        //     _offset = offset;
        //     
        //     survivalController.OnCriticalHunger.Subscribe(_ => ShowHungerWarning(_));
        //     survivalController.OnCriticalThirst.Subscribe(_ => ShowThirstWarning(_));
        //     
        //     ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
        // }

        private void OnDisable()
        {
            IUpdateClear();
        }


        public void IUpdateClear()
        {
            DLog.Alert("Survival Icons disabled "+_target);
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        public void UpdateM()
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(_target.position + _offset);
            transform.position = screenPos;
        }


        private void ShowHungerWarning(bool critical)
        {
            hungerIcon.gameObject.SetActive(critical);
        }

        private void ShowThirstWarning(bool critical)
        {
            thirstIcon.gameObject.SetActive(critical);
        }
    }
}
