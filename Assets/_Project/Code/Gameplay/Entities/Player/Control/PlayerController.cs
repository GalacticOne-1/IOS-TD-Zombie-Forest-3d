using Galactic1.AbstractFactory;
using Galactic1.Configs;
using Galactic1.Core.Input;
using UnityEngine;

namespace Galactic1.Gameplay.Player
{
    public class PlayerController : _Entity, IPlayerController
    {
        
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerCombatController combat;

        
        
        [Space] [Header("* WALL JUMPING")]
        public Transform checkWall;
        public Vector2 wallCheckSize;

        public _Entity Entity => this;
        //public StatsControllerBase StatsController { get; }
        private CapsuleCollider2D _capsuleCollider2D;
        public CapsuleCollider2D CapsuleCollider2D => _capsuleCollider2D;

        [HideInInspector] public bool ONE_ARM;
        
        
        
        // private void Update()
        // {
        //     //movement.SetMoveVector(InputManager.Instance.MoveDirection);
        // }

        public override void Entity_Setup<T>(T data)
        {
            if (data is PlayerLoadData loadData)
            {
                _capsuleCollider2D = GetComponent<CapsuleCollider2D>();
                
                
                // #1 Передаём базовые статы
                //StatsController.Initialize(loadData.PlayerStatsBase, EquipmentContainer_old);
                
            }
            else
            {
                Debug.LogError($"Player got wrong data for Initialize {data}");
            }
        }
        
        public void ControllerEnable()
        {
            // var psc = StatsController as PlayerStatsController;
            //
            // psc.SetControlFeatures(StatId.SpeedMovement);
            // psc.SetControlFeatures(StatId.JumpForce);
            // psc.SetControlFeatures(StatId.WallJumpForce);
            // psc.SetControlFeatures(StatId.WallSlideSpeed);
            
            // теперь Player активная сущность
            // InputManager.Instance.SetControllable(ControllableType.Player);
            //
            // // подписываемся на input
            // InputManager.Instance.OnAttack += combat.TryAttack;
            // InputManager.Instance.OnInteract += interaction.TryInteract;
        }

        public void ControllerDisable()
        {
            return;
            InputManager.Instance.OnAttack -= combat.TryAttack;
        }




        public void SwitchAnimatorController()
        {
            bool isDragon = ServiceLocator.Current.Get<HeroStateMachine>().IsDragon;
            
            // new Inventory_GET_SLOT_ID(EInventorySlot.main_weapon, out byte slot);
            // if (new Inventory_AVAIL().InStock(slot))
            // {
            //     new Inventory_GET_SLOT_ASSET(slot, out AssetItems goods, out InventoryConfigs equipment);
            //     ServiceLocator.Current.Get<ConfigProvider>().Get<PlayerCharacterConfig>()
            //         .SetAnimatorController(isDragon, 0, (equipment as AssetInventory_weapon).equipmentType,
            //             out ONE_ARM);
            // }
            
            //else
            {
                ServiceLocator.Current.Get<ConfigProvider>().Get<PlayerCharacterConfig>()
                    .SetAnimatorController(isDragon, 0, EEquipmentType.Unarmed,
                        out ONE_ARM);
            }
        }
        
    }
    
    
    // Weapon component used by builder
    public class PlayerWeaponComponent : MonoBehaviour
    {
        private int ammo;

        public void SetAmmo(int a) => ammo = a;
        public int GetAmmo() => ammo;
    }
}