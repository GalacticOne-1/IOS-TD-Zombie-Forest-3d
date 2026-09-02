using Galactic1.Configs;
using Galactic1.Repository;
using Galactic1;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    
    public class DeveloperConsole : Singleton<DeveloperConsole>, ISceneActivator
    {
        /*
         *     Содержит методы для запуска игры в режиме разработки
         */

        public DevCore core;
        public DevGame game;


        
        
        
        
        
        
        
        
        /*
        *    При старте приложения, закрываем доступ к dev режиму
        *    Подгоняем все ассеты и пр для готовой игры
        */
        public void Activator()
        {
            SystemRepository.IsRelease =
                ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().modeRegular;
            if (SystemRepository.IsRelease)
            {
                core.showDevPanel = false;
                core.camera_dev = false;
                core.launch_tutorial = false;
                core.test_battle = false;
                core.dev_polygon = false;
                
                game.load_dev = false;
                game.vip_pack = false;
                game.daily_reward_test_work = false;
                game.passDay = false;
                game.passedDay = 0;

                game.load_dev = false;
                game.show_hp_after_damage = false;

                game.unitLog = 0;
                
                // player
                game.player_revive = false;
                game.player_immortal = false;
                game.player_units_spawn_NO = false;
                game.player_units_activate_NO = false;
                game.player_structures_activate_NO = false;
                game.player_units_spawn_all = false;
                
                // enemy
                game.enemy_immortal = false;
                game.enemy_spawn_NO = false;
                game.enemy_move_NO = false;

                game.disable_survival = false;
                
                
                game.not_use_soft = false;
                game.not_use_hard = false;
                game.not_use_resources = false;
                
                game.unlock_skill = false;
                game.unlock_all_assets = false;
                game.unlock_menu = false;

                game.spawnUnits = -1;
            }

            else
            {
                
            }
        }


        

    }


    [System.Serializable]
    public class DevCore
    {
        public bool showDevPanel;
        public bool camera_dev;
        public bool launch_tutorial;
        public bool test_battle, dev_polygon;
    }
    
    [System.Serializable]
    public class DevGame
    {
        public bool vip_pack;

        public bool passDay;
        public int passedDay;
        public bool daily_reward_test_work;
        public bool load_dev;
        public bool show_hp_after_damage;

        [Space] 
        public byte unitLog;
        public bool showAllLogs;

        [Space] 
        public bool player_revive;
        public bool player_immortal;
        public bool player_units_spawn_NO;
        public bool player_units_activate_NO;
        public bool player_structures_activate_NO;
        public bool player_units_spawn_all;
        
        public bool enemy_immortal;
        public bool enemy_spawn_NO;
        public bool enemy_move_NO;

        [Space] 
        public bool disable_survival;
        
        
        [Space]
        public bool not_use_soft;
        public bool not_use_hard;
        public bool not_use_resources;

        [Space]
        public bool unlock_skill;
        public bool unlock_all_assets;
        public bool unlock_menu;

        [Space] public sbyte spawnUnits;


        [HideInInspector]
        public byte lerpVariant;

    }
    [System.Serializable]
    public struct DevHero
    {
        public int id;
    }
}