using DEV;
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class ProgressParam
    {
        /*
         *    Текущие хар-ки юнитов с учетом всех улучшений
         */

        
        
        

        
        
        
        
        
        
        
        
        
        
        #region Skill Upgrade
        
        // значение за один уровень, общая прокачка  

        // tier 1
        public const float skillEarnCoins = .025f;
        public const float skillEarnExp = .025f;
        public const float skillMonolith = .025f;
        public const float skillHp = .01f;
        public const float skillLifespan = .005f;
        public const float skillCooldownAbility = .005f;
        
        
        // tier 2
        public const float skillDamage = .025f;
        public const float skillDamageAbility = .025f;
        public const float skillDurationEffect = .025f;
        

        #endregion



        

        #region Support Towers

        public static float supportTowerCoins;
        public static float supportTowerExp;
        public static float supportTowerScrap;
        public static float supportTowerBiomaterial;

        #endregion
        
        
        
        #region Enemies
        
        // значение за один уровень,

        private const int wave_2 = 1;
        private const int wave_5 = 2;


        #endregion









        
        
        
        


        #region Skill upgrade
        
        // складываем бонусы из прокачки, зданий и пр
        
        // для получения фактического значения add = 0
        // для показа в виджете add = 1
        
        // TIER 1

        /// <summary>
        /// Добавление к зарабатываемы деньгам за врага или в шахте
        /// </summary>
        /// <returns></returns>
        public static float SkillEarnedCoins(byte add = 0)
            => 0;//(GAMEPLAY.DataGameplay().upgCoin + add) * skillEarnCoins;
        
        /// <summary>
        /// Добавление к опыту за врага
        /// </summary>
        /// <returns></returns>
        public static float SkillEarnedExp(byte add = 0)
            => 0;//(GAMEPLAY.DataGameplay().upgExperience + add) * skillEarnExp;
        
        /// <summary>
        /// Добавление к здоровью героев
        /// </summary>
        /// <returns></returns>
        public static float SkillMonolithHealthBonus(byte add = 0)
            =>  0;//(GAMEPLAY.DataGameplay().upgMonolith + add) * skillMonolith;
        
        /// <summary>
        /// Увеличение здоровья призванным существам
        /// </summary>
        /// <returns></returns>
        public static float SkillHp(byte add = 0)
            => 0;// (GAMEPLAY.DataGameplay().upgHp + add) * skillHp;
        
        /// <summary>
        /// Увеличение времени жизни призванных отрядов
        /// </summary>
        /// <returns></returns>
        public static float SkillLifespan(byte add = 0)
            =>  0;//(GAMEPLAY.DataGameplay().upgLifespan + add) * skillLifespan;
        
        /// <summary>
        /// Ускорение отката способностей героев
        /// </summary>
        /// <returns></returns>
        public static float SkillCooldownAbility(byte add = 0)
            =>  0;//(GAMEPLAY.DataGameplay().upgCooldown + add) * skillCooldownAbility;
        
        
        
        
        // TIER 2
        
        /// <summary>
        /// Увеличение урона для всех юнитов
        /// </summary>
        /// <returns></returns>
        public static float SkillDamage(byte add = 0)
            =>  0;//(GAMEPLAY.DataGameplay().upgDamage + add) * skillDamage;
        
        /// <summary>
        /// Увеличение урона для ability
        /// </summary>
        /// <returns></returns>
        public static float SkillDamageAbility(byte add = 0)
            => 0;// (GAMEPLAY.DataGameplay().upgDamageAbility + add) * skillDamageAbility;
        
        /// <summary>
        /// Увеличение продолжительности способности
        /// </summary>
        /// <returns></returns>
        public static float SkillDurationEffect(byte add = 0)
            =>  0;//(GAMEPLAY.DataGameplay().upgDurEffect + add) * skillDurationEffect;

        #endregion





        #region MODULES


        /// <summary>
        /// Урон с критом
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public static float CritDamage(byte critHitChance, float critHitPercent, float damage, out bool isCrit)
        {
            DLog.Alert($"Crit Chance: {critHitChance}, Damage Percent: {critHitPercent}");
            isCrit = Random.Range(0, 100) < critHitChance;
            return isCrit ? CritDamage(critHitPercent, damage) : damage;
        }
        
        /// <summary>
        /// Формула крит. урона
        /// <br/>(процент от основоного урона 150%/200%/300% ...) 
        /// </summary>
        /// <param name="critHitPercent"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        public static float CritDamage(float critHitPercent, float damage)
            => damage * (critHitPercent / 100);



        public static bool use_steal_life;
        public static float stealLife;
        
        /// <summary>
        /// Восстановление хп за счет урона по врагу
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public static float StealLife(float damage)
            => use_steal_life ? damage * stealLife : 0;


        /// <summary>
        /// Значение для лечебной корбки (bakta)
        /// </summary>
        public static float medikit;
        
        

        #endregion
       
        

        #region SUMMON

        // default unit damge * self upgrade * oaverall upgrade * sinergia hero;

        public static float DamageSummon(float baseVal)
            => baseVal
               + (baseVal * SkillDamage());

        public static float HpSummon(float baseVal)
            => baseVal
               + (baseVal * SkillHp());

        
        
        
        #endregion


        #region Modules

        
        
        public static float DamageModules(float baseVal)
            => baseVal
               + (baseVal * SkillDamage());

        
        #endregion
        
        
        #region Heroes
        
        
        /// <summary>
        /// Здоровье героя с учетом скила
        /// </summary>
        /// <param name="baseVal"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static float HpHero(float baseVal)
            => baseVal
               + (baseVal * SkillHp());

        /// <summary>
        /// Урон героев
        /// </summary>
        /// <param name="baseVal"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static float DamageHero(float baseVal)
            => baseVal
               + (baseVal * SkillDamage());

        /// <summary>
        /// Урон for ability
        /// </summary>
        /// <param name="baseVal"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static float DamageAbility(float unitDamage, float percent)
            => unitDamage * (percent/100 + SkillDamageAbility());
        
        /// <summary>
        /// Продолжительность эффекта от абилки
        /// </summary>
        /// <param name="baseVal"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static float DurationEffect(float baseVal)
            => baseVal
               + (baseVal * SkillDurationEffect());
        
        /// <summary>
        /// Отакт способности
        /// </summary>
        /// <param name="baseVal"></param>
        /// <returns></returns>
        public static float CooldownHero(float baseVal)
            => baseVal
               - (baseVal * SkillCooldownAbility());
        
        /// <summary>
        /// Срок жизни юнита
        /// </summary>
        /// <param name="baseVal"></param>
        /// <returns></returns>
        public static float LifespanSummons(float baseVal)
            => baseVal
               + (baseVal * SkillLifespan());

        
        #endregion
        



        #region Enemies
        
        

        // деньги за караван
        //public static int RewardCaravan()
            //=> RefController.I.dataBase.caravanWave.MoneyForWave() * 9;
        
        
        // прогресс для вражеских существ 
        
        
        // увеличение объема существ в волне после боя с боссом
        /*public static int VolumeEnemies_small(int baseVolume)
            => baseVolume
               + (Mathf.FloorToInt(GAMEPLAY_old.CurWave() / 2) * wave_2)
               + (Mathf.FloorToInt(GAMEPLAY_old.CurWave() / 5) * wave_5);
        
        public static int VolumeEnemies_mid(int baseVolume)
            => baseVolume
               + (Mathf.FloorToInt(GAMEPLAY_old.CurWave() / 2) * wave_2)
               + (Mathf.FloorToInt(GAMEPLAY_old.CurWave() / 5) * wave_5);
        
        public static int VolumeEnemies_big(int baseVolume)
            => baseVolume
               + (Mathf.FloorToInt(GAMEPLAY_old.CurWave() / 5) * wave_5);
        
        public static int VolumeEnemies_heavy(int baseVolume)
            => baseVolume
               + (Mathf.FloorToInt(GAMEPLAY_old.CurWave() / 5) * wave_2);

        public static int VolumeEnemies_legendary(int baseVolume)
            => baseVolume
               + (Mathf.FloorToInt(GAMEPLAY_old.CurWave() / 10) * wave_2);
        
        
        // цена существа с учетом всех улучшений
        public static int CostEnemy(int baseVal)
            => baseVal + (Mathf.FloorToInt(baseVal * SkillEarnedCoins()))
                        + (Mathf.FloorToInt(baseVal * supportTowerCoins));
        
        // опыт за существо с учетом всех улучшений
        public static int ExpEnemy(int baseVal)
            => baseVal + (Mathf.FloorToInt(baseVal * SkillEarnedExp()))
                       + (Mathf.FloorToInt(baseVal * supportTowerExp));
                       */
        



        #endregion
    }
}