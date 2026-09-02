using System;
using Galactic1.Repository;
using UnityEngine;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "PlayerCharacterConfig", menuName = "Game Configs/Player/Player Character Config")]
    public class PlayerCharacterConfig : ScriptableObject
    {
        

        #region DOLL SKIN

        [Header("Готовый юнит с анимацией")]
        [field:SerializeField] public GameObject Prefab {get; private set;}

        [SerializeField] private CUnitSkin[] dollSkin;
        
        [Serializable]
        public struct CUnitSkin
        {
            public Sprite head,
                torso;

            public Sprite[] arms, legs;
        }
        /// <summary>
        /// Для получения комплекта скина юнита
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CUnitSkin GetSkin(int id) => dollSkin[id];

        public int skinCount => dollSkin.Length;
        

        #endregion
        
        
        public InventoryConfigs fight;
        
        
        


        [Header("Animator CNTR")] [SerializeField]
        private CAnimatorController[] _animatorControllers;
        
        [System.Serializable]
        public struct CAnimatorController
        {
            public EAnimatorVariant type;
            public RuntimeAnimatorController controller;
            public bool oneArm;
        }
        
        public enum EAnimatorVariant
        {
            Unarmed, 
            Tool, Spear, Sidearm, Smg, Shotgun, Rifle,
            _1, _2, _3, _4, _5,
            Unarmed_Dragon
        }
        
        
        
        
        
        
        public RuntimeAnimatorController cntrGun;
        public RuntimeAnimatorController cntrRifle;
        public RuntimeAnimatorController cntrSMG;
        public RuntimeAnimatorController cntrShotgun;
        public RuntimeAnimatorController cntrSpear;
        public RuntimeAnimatorController cntrTool;
        public RuntimeAnimatorController cntrFight;









        public void SetAnimatorController(bool isDragon, int id, EEquipmentType klass, out bool oneArm)
        {
            oneArm = false;
                
            // #1 определяем контроллер
            EAnimatorVariant requires = EAnimatorVariant.Unarmed;

            // unit
            if (!isDragon)
            {
                switch (klass)
                {
                    case EEquipmentType.Unarmed:
                        requires = EAnimatorVariant.Unarmed;
                        break;
                
                    case EEquipmentType.Tool_hit:
                        requires = EAnimatorVariant.Tool;
                        break;
                
                    case EEquipmentType.Spear:
                        requires = EAnimatorVariant.Spear;
                        break;
                    case EEquipmentType.Sidearm:
                        requires = EAnimatorVariant.Sidearm;
                        break;
                
                    case EEquipmentType.Smg:
                        requires = EAnimatorVariant.Smg;
                        break;
                
                    case EEquipmentType.Shotgun:
                        requires = EAnimatorVariant.Shotgun;
                        break;
                
                
                    case EEquipmentType.Mg:
                    case EEquipmentType.Assault_rifle:
                    case EEquipmentType.Marksman_rifle:
                    case EEquipmentType.Rifle:
                        requires = EAnimatorVariant.Rifle;
                        break;
                
                }
            }

            else
            {
                switch (klass)
                {
                    case EEquipmentType.Unarmed:
                        requires = EAnimatorVariant.Unarmed_Dragon;
                        break;

                }
            }

            // #2 находим нужный и подключаем
            var l = _animatorControllers.Length;
            for (int i = 0; i < l; i++)
            {
                if (_animatorControllers[i].type == requires)
                {
                    ServiceLocator.Current.Get<PlayerRepository>().GetUnit("player").Animation.animator
                        .runtimeAnimatorController = _animatorControllers[i].controller;
                    oneArm = _animatorControllers[i].oneArm;
                }
            }
        }


        
        
        /// <summary>
        /// Смена контроллера под используемое оружие
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /*public void SetAnimatorController(int id, EEquipmentType klass, out bool one_arm)
        {
            one_arm = false;
            switch (klass)
            {
                case EEquipmentType.Unarmed:
                    _PointerHub.player_unit[id].animRef.anim.runtimeAnimatorController = cntrFight;
                    break;
                
                
                case EEquipmentType.Tool_hit:
                    _PointerHub.player_unit[id].animRef.anim.runtimeAnimatorController = cntrTool;
                    one_arm = true;
                    break;
                
                case EEquipmentType.Spear:
                    _PointerHub.player_unit[id].animRef.anim.runtimeAnimatorController = cntrSpear;
                    one_arm = true;
                    break;
                

                case EEquipmentType.Sidearm:
                    _PointerHub.player_unit[id].animRef.anim.runtimeAnimatorController = cntrGun;
                    one_arm = true;
                    break;
                
                case EEquipmentType.Smg:
                    _PointerHub.player_unit[id].animRef.anim.runtimeAnimatorController = cntrSMG;
                    one_arm = true;
                    break;
                
                case EEquipmentType.Shotgun:
                    _PointerHub.player_unit[id].animRef.anim.runtimeAnimatorController = cntrShotgun;
                    one_arm = true;
                    break;
                
                
                case EEquipmentType.Mg:
                case EEquipmentType.Assault_rifle:
                case EEquipmentType.Marksman_rifle:
                case EEquipmentType.Rifle:
                    _PointerHub.player_unit[id].animRef.anim.runtimeAnimatorController = cntrRifle;
                    break;
                
            }
        }*/



        

    }
}