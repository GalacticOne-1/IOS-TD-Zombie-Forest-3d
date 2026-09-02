using System;
using UnityEngine;

namespace Galactic1
{
    public class UnitSetup : IGameService
    {


        /// <summary>
        /// Создание массива id для прямого доступа к CUpgradable[]
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        /*public int[] CreateLinks(CFortressUpgrade[] ar)
        {
            var l = ar.Length;
            int[] links = new int[Enum.GetValues(typeof(EFortressUpgrade)).Length];
            
            for (int i = 0; i < l; i++)
                links[(byte)ar[i].typeUpgrade] = i;

            return links;
        }*/
    }

    
    
    #region PLAYER BASE
    
    public class CFortressData
    {
        public CFortressSetup setup;
    }
    
    
    [Serializable]
    public struct CFortressSetup
    {
        public short bonus_hp;
        public CFortressUpgrade[] list_upgrades;
    }

    public struct CUnitSetup
    {
        public short bonus_dmg;
        public CFortressUpgrade[] list_upgrades;
    }

    public struct CFortressUpgrade
    {
        public bool unlocked;
        //public EFortressUpgrade typeUpgrade;
        public float startValue;
        public float upgradeCoeffStart;
        public float upgradeCoeff;
        public int maxLevel;
    }
    
    
    #endregion
    
    
    
    
    
    #region PLAYER UNIT
    
    public class CUnitData
    {
        public CSetup setup;
        //public CWeaponSetup weapon, weapon2;
    }
    
    [Serializable]
    public class CSetup
    {
        public float[] modificators;        // %
        
        public short hp, max_hp, recoveryHp, armor;
        public short dodge;
        
        public float speed;
        public short accuracy;
        public float detectRange;             
        
        public short critHitChance;
        public short critHitPercent;
        
        public short psiHitChance;
        public short psiHitDamage;
    }
    
    
    [Serializable]
    public struct CWeaponSetup
    {
        public EVariantAttack variantAttack;
        //public ETypeAttack typeTarget;
        public EStatusDamage status;
        public float rangeDetect,           // для обнаружения цели
            rangeAttack;                    // для атаки

        public byte accuracy;               // точночть             
        [Range(0, 1)]
        public float recoil;                // отдача
        
        [Space]
        public float fireRate;
        public float reload;
        public int ammo;
        public float damage;   
        public float damagePerSec;

        [Space, Header("Доп. эффект")] 
        public bool manualMode;             // режим стрельбы
        public short piercing;              // игнорирование брони
        public short shred;                 // разрыв брони
        public float aoeDamage;
        public float aoeRange;
        public float critHitChance;
        public float critHitPercent;

        [Header("***   RAGDOLL   ***")] 
        //[Range(0,300)]
        public float hitForce;
        
        
        [Range(1,20)]
        public byte countTargets;
    }
    #endregion



    #region ENEMY SETUP

    public class CEnemySetup
    {
        public byte assetId;
        public float hp, speed;
        public int rewardBasic, rewardGold, experience;
        public bool required_gold_reward;
        public string title;

        public CWeaponSetup weapon;
    }

    #endregion
}