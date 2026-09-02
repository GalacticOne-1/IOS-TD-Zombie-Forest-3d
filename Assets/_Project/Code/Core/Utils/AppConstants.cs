using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class AppConstants
    {
        // true - для релиза
        public const bool SERVER_ON = false;
        
        public const float DELAY_HINT = .35f;
        public const float BTN_ALPHA = .3f;

        public const string LAUNCH = "second_launch";

        public const string SERVER = "galactic1games.com";
        public const string GAME_PAGE = "https://play.google.com/store/apps/";


        public const bool show_log_core = true;
        public const bool show_log_economics = true;
        public const bool show_log_scene_clear_event = true;
        public const bool show_log_unit_fsm = true;
        public const bool show_log_building = true;
        public const bool show_log_inventory = true;

        public const string EXIT_SCENE_REQUEST_TAG = nameof(EXIT_SCENE_REQUEST_TAG);

        // squad
        public const float ACCURACY = 10;
        public const float MAX_STABILITY = 1.3f;        // после этого значения начинается сильный разброс оружия
        public const float SPAWN_DELAY = .2f;

        public const float gravity = 5;
        
        public const float DetectionRadius = .48f;

        // 1 - для прямого управления
        public const byte player_unit_qu = 1;

        

        /// Слой для пуль (for player)
        public const byte layer_player_bullet = 10;
        /// Слой для обнаружения вражеских юнитов (for player)
        public const byte layer_detect_enemies_gr = 11;
        /// Слой для обнаружения вражеских юнитов (for player)
        public const byte layer_detect_enemies_air = 0;                 // ! не назначено !
        
        
        
        /// Слой для пуль (for enemy)
        public const byte layer_enemy_bullet = 13;
        /// Слой для обнаружения юнитов игрока (for enemy)
        public const byte layer_detect_player_gr = 8;
        /// Слой для обнаружения юнитов игрока (for enemy)
        public const byte layer_detect_player_air = 0;                  // не назначено !!!
        /// Слой для обнаружения объектов игрока (for enemy)    
        public const byte layer_detect_player_obj = 26;                 // не назначено !!!
        /// Слой для обнаружения объектов игрока которые могут уничтожаться (for enemy)
        public const byte layer_detect_player_obj_damage = 27;          // не назначено !!!

        public const byte layer_detect_player_obj_destroyable = 0;      // не назначено !!!
        
        /// Слой земли по которой можно ходить
        public const byte layer_walkable_ground = 15;
        /// Слой не проходимая преграда для всех
        public const byte layer_obstacle_hard = 16;
        /// Слой непроходимый игроком
        public const byte layer_player_border = 17;
        /// Слой для объектов взаимодействия
        public const byte layer_interaction_obj = 18;




        public const byte max_stage = 8;
        public const byte wave_in_stage = 10;
        
        
        // GUI
        public static Color color_lock_grey = new Color(0.49f, 0.51f, 0.48f);       // заблокиорванный контент
        public static Color color_lock_black = new Color(0,0,0);


        // должно быть больше размера колайдера дракона, что бы можно было всегда залезть обратно на дракона
        public const byte MAX_DISTANCE_TO_DRAGON = 4;

        //
        // /// границы лагеря
        // public static Vector2 CAMP_BORDER_X = new Vector2(-12, 50);
        // /// границы лагеря
        // public static Vector2 CAMP_BORDER_Y = new Vector2(1.5f, 50);
        //
        // /// границы лагеря без выхода на карту
        // public static Vector2 CAMP_BORDER_CLOSE_X = new Vector2(-5, 43);
        // /// границы лагеря без выхода на карту
        // public static Vector2 CAMP_BORDER_CLOSE_Y = new Vector2(1.5f, 43);

        /// дней до сброса рейда
        public const byte EVENT_LOCATION_REQUIRE_DAY = 5;



        public const byte WEAPON_CONSUMPTION = 3;
        public const byte ARMOR_CONSUMPTION = 10;


        public const string PATH_GAMEPLAY = "Prefabs/GAMEPLAY/";
        public const string PATH_UI = "Prefabs/UI/";
        public const string PATH_UI_GAMEPLAY = "Prefabs/UI/Gameplay/";
        public const string PATH_ITEMS = "Prefabs/Gameplay/Items/";
        public const string PATH_FX = "Prefabs/Gameplay/Effects/";
        
        public const string PATH_PLAYER = "Prefabs/Gameplay/Player/Survivors/";
        public const string PATH_ENTITIES = "Prefabs/Gameplay/Entities/";
        public const string PATH_STRUCTURES = "Prefabs/Gameplay/Entities/Facilities/";
        public const string PATH_ENEMIES = "Prefabs/Gameplay/Enemies/";
        public const string PATH_ZOMBIE_VISUALS = "Prefabs/Gameplay/Enemies/Zombies/Visuals/";

    }


    #region STRUCTURE => DEFAULT
    
    
    public struct CStatData
    {
        public bool isItem;
        public EBankResourceType BankResourceType;
        //public EItem item;
        public int volume;
    }
    
    public enum ECanvas
    {
        OFF, LOBBY, GAME, MAP, LEVEL, OVER
    }
    
    public enum EMenu
    {
        OFF, REGULAR, CANCEL, CONSTRUCT, MOVING, REPAIR
    }
    
    public enum ECanvasStat
    {
        REGULAR,                // обычное отображение
        LEVEL,                  // бой/уровнеь
        
        ARSENAL,
        WORLD,
        SCRAP,
        SHOP
    }
    
    public enum EWindow
    {
        START,
        PAUSE,
        FINISH,
        
        PLAYER_DEAD,
        
        POPUP_REWARD,
        LEVEL_UP_PLAYER,
        NEW_UNIT,
        LEVEL_UP_UNIT,
        NEW_MODIFICATOR,
        NEW_LOCATION,
        EQUIPMENT_UPGRADE,
        
        RAID_COMPLETE,
    }
    
    
    public enum EAssetSign
    {
        FREE, GEMS, SHOP
    }

    public enum ETutorial
    {
        NULL, ACTIVE, FINISHED
    }
    
    public enum EStateAd
    {
        START, NOT_AVAILABLE, FIRST_LOAD, AVAILABLE, RESTORING
    }
    
    public enum EStateButton
    {  
        ENABLE, 
        DISABLE, 
        SELECTED,
        TXT_REGULAR, 
        TXT_ALERT,           // for money
        ENABLE_ONLY_TEXT,
        DISABLE_ONLY_TEXT,
    }
    
    public enum EState
    {
        CLOSED, REFRESH, PROCEEDING, COLLECT, FINISH
    }

    public enum EAssetState
    {
        LOCK,               // не доступен по прогрессу
        UNLOCK_DELAY,       // доступен по прогрессу лвл, но ждет предыдущего
        UNLOCK,             // разблокирован, нужно купить
        USING               // полностью доступен
    }

    public enum EUnitRenderParts
    {
        head, torso, arms, legs
    }

    
    public enum EMission
    {
        supply, military_base, lab, equipment, armory
    }
    
    
    public enum ERarities
    {
        Standard = 0, Superior = 1, High_End = 2, Exotic = 3
    }

    public enum ESorting
    {
        Resource = 0, Item = 1, Equipment = 2, Armor = 3, Weapon = 4
    }
    
    public enum ECompare
    {
        EQUAL, MORE, LESS
    }


    public enum EUnitMode { EMPTY, PLAYER_UNIT, PLAYER_DRAGON }
    

    #endregion


    

    #region ATTRIBUTES & STATS


    public enum EAnimationVariant
    {
        Idle, Movement, Jumping, Touches_Wall, Die, 
        Attack, Attack_Unarmed, Grab, Chest,
    }


    public enum EClassUnit
    {
        /*
        Assault,                    // Владение автоматами и дробовиками
        Heavy,                      // Владение пулеметами, пушками и гранатометами
        Sniper,                     // Владение снайперскими винтовками и пистолетами
        Specialist,                 // Умение ставить силовые поля и управлять атакующими дронами
        Engineer,                   // Умение чинить объекты и ставиь автоматические турели
        Medic,
        Psi_operative,

        Berserker,                  // ??? ближний бой
        Infiltrator,                // Владение арбалетами и био-оружием. Получите +25% к скрытности , пока вас не обнаружат.
        */
        
        Shooter,                    // обычные стрелки
        Damager,                    // атака по большому кол-ву врагов
        Support,                    // 
    }
    
    public enum EVariantAttack
    {
        One_target, 
        Several_targets, 
        Splash, 
        Close_one_target, 
        Close_several_targets
    }
    
    /*
     *  Что такое мастерство?
        В Phoenix Point любой оперативник может использовать любое оружие, броню или другое оборудование независимо от его класса , 
        но отсутствие опыта в его использовании (не связанного с классом оружия) влечет за собой штраф:

        Уменьшение эффективной дальности действия оружия дальнего боя на 50% при использовании его без соответствующего навыка.
        Во всех остальных случаях, например, при атаке оружием ближнего боя , не владея им, есть 50% шанс нащупать мяч ; 
        то есть не выполнить действие после того, как потратит на него необходимые AP и WP .
     */

    // все аттрибуты в одном месте (for unit, weapon, equipment)


    public enum EInventorySlot
    {
        NULL,
        regular_slot,
        main_weapon,   
        torso, head, arms, legs,
        backpack,
        fast_pocket_1,
        fast_pocket_2,
        
        Result,
        Fuel,
        Recipe
    }
    

    
    
    public enum EEquipmentType
    {
        NON, Unarmed, Sidearm, Rifle, Assault_rifle, Marksman_rifle, Mg, Smg, Shotgun, 
        granade_launcher, rocket_launcher, cannon, flamethrower, granade, ammo, mines,
        
        helmet, torso, arms, legs, holsters,
        
        Tool_hit,
        Spear,
    }

    public enum EModeSlot
    {
        // WEAPON
        muzzle,
        optic,
        gear,
        underbarrel,
        magazine,
        butt
    }

    public enum ETypeTactical
    {
        launch,             // запуск сразу
        target              // запуск через наведение
    }
    
    
    public enum EStatusDamage
    {
        missed, hit, bullet, aoe
    }
    
    
    public enum EHPBodyParts
    {
        torso, 
        head, 
        arm,
        leg,
        claw
    }


    #endregion


    public enum EUsing
    {
        NULL, HEALTH, HUNGER, THIRST
    }

    
    public enum EItems
    {
        Common_Log, Common_Plank,
        Aluminium_Bar, Aluminium_Plate, Aluminium_Wire,
        Animal_Rawhide, Leather, 
        Ball_Bearing,
        EMPTY_3, EMPTY_4,
        Batteries,
        Bauxite,
        Belt,
        Berry,
        Berry_Tea,
        Bolts,
        Bottle_Water, Empty_Bottle,
        Empty_Can,
        Cog,
        Copper_Ore, Copper_Bar, Cooper_Wire,
        Duct_Tape,
        Edible_Mushroom,
        Electronic_Circuit,
        Electric_motor,
        Explosive_Material,
        Fuel_Briquette,
        Gasoline,
        Sand, Glass,
        Plastic,
        Scrap_Metal,
        Iron_Ore, Iron_bar, Iron_Plate, Iron_Wire,
        Lead, Lead_Plate,
        Limestone, Stone_Brick,
        Charcoal, Steel_Bar, Steel_Plate, Steel_Pipes,
        
        Glue,
        Lens,
        Nails,
        EMPTY_1, EMPTY_2,
        Canned_Food, Canned_fish,
        Car_Battery,
        Sulfur,
        Gunpowder,
        Plant_Fiber, Piece_Cloth, Rope, Thick_Fabric,
        Carbon_Composite, 
        Alcohol,
        Anti_radiation_Pills,
        
        Raw_Meat, Jerky, Juicy_Steak, Raw_Turkey, Roasted_Turkey,
        
        Rubber_Parts,
        Transistor,
        Wiring,
        Spring,
        Rations_MRE,
        Light_Bulb,
        Quartz,
        Wheel_Parts,
        
    }


    public enum EEquipment
    {
        // --- equipment
        
        Basic_Backpack, Military_Backpack, Tactical_Backpack,
        hlmet_1, hlmet_2, hlmet_3, hlmet_4, hlmet_5, hlmet_6, hlmet_7, 
        Reinforced_Builder_Vest, Thick_Jacket, Reinforced_Jacket, Tactical_Body_Armor, SWAT_Body_Armor, Kevlar_Body_Armor, Wasteland_Body_Armor, torso_8,
        
        // --- weapon
        Hatchet, Pickaxe, Spear, Makeshift_Bat, Iron_Hatchet, Iron_Pickaxe, 
        Crowbar, Skull_Crusher, Machete, Saw_Blade_Mace,
        Hammer, baseball_bat, Iron_Makeshift_Bat , Zip_Gun,
        glock_17, Colt_Python, Shotgun, AK_47, Mini_Uzi, VSS_Vintorez, Crossbow, Turret, 
        Bandages, First_Aid_Kit,
        
        
        Campfire, Gunsmith_Bench, Medical_Table, Melting_Furnace,
        Rain_Catcher, Recycler, Refined_Melting_Furnace, Sewing_Table,
        Small_Box, Stonecutter_Table, Tanning_Rack, Woodworking_Bench,
        Workbench, Room,
        
        Spike_Trap, Wheels, Wheels_Reinforced, Fishing_Net,
        Anti_Tank_Barrier, Barrels_Expl, Block_1, Block_2,
        Repair_Complect
        
    }
     

    
    
    
}