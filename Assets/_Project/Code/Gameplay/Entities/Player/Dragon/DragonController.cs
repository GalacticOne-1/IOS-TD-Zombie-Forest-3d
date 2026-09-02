using Galactic1.AbstractFactory;
using Galactic1.Core.Input;
using Galactic1.Code.Gameplay.Units.Stats;
using UnityEngine;

namespace Galactic1.Gameplay.Player
{
    public class DragonController : _Entity, IPlayerController
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerCombatController combat;

        public _Entity Entity => this;
        //public StatsControllerBase StatsController { get; }
        public Dragon_GroundDetector _groundDetector;
        public GameObject playerPlace;
        
        private void Update()
        {
            //movement.SetMoveVector(InputManager.Instance.MoveDirection);
        }

        public override void Entity_Setup<T>(T data)
        {
            if (data is PlayerLoadData loadData)
            {
                
                // #1 Передаём базовые статы
                //StatsController.Initialize(loadData.dragonStatsBase, EquipmentContainer_old);
                
            }
        }
        
        
        public void ControllerEnable()
        {
            //var psc = StatsController as PlayerDragonStatsController;
            
            //psc.SetControlFeatures(StatId.SpeedMovement);
            
            // теперь Player активная сущность
            //InputManager.Instance.SetControllable(ControllableType.Dragon);

            // подписываемся на input
            //InputManager.Instance.OnAttack += combat.TryAttack;
            //InputManager.Instance.OnInteract += interaction.TryInteract;
        }

        public void ControllerDisable()
        {
            return;
            InputManager.Instance.OnAttack -= combat.TryAttack;
        }

        
    }
}