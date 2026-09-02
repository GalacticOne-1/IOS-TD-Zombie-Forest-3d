using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Core;
using Galactic1.Code.Core.State;
using Galactic1.Utility;
using Galactic1.Configs;
using Galactic1.UI.Shop;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.Game.World.StartLocation;
using Galactic1.Structs;
using Galactic1.Window;
using R3;
using UnityEngine;

namespace Galactic1.Core
{
    public class JsonGameStateProvider : IGameStateProvider
    {
        private readonly ConfigProvider _configsProvider;
        private const string GAME_STATE_KEY = nameof(GAME_STATE_KEY);
        private const string SAVE_PATH = "zombie_forest_3d_savedata";
        
        public GameStateProxy GameStateProxy { get; private set; }

        private GameState _gameStateOrigin;


        public JsonGameStateProvider(ConfigProvider configsProvider)
        {
            _configsProvider = configsProvider;
        }

        public Observable<GameStateProxy> LoadGameState()
        {
            if (!PlayerPrefs.HasKey(GAME_STATE_KEY))
            {
                PlayerPrefs.SetString(GAME_STATE_KEY, "y");
                GameStateProxy = CreateGameStateFromConfig();
                InitializeFirstSave();
                DLog.Alert("Game State created from default basicSettings ", EDlogColor.YELLOW, AppConstants.show_log_core);

                SaveGameState();        // сохраняем состояние при первом старте аппки
            }
            
            else
            {
                // загружаем существующее состояние
                _gameStateOrigin = DataSaver.loadData<GameState>(SAVE_PATH);
                GameStateProxy = new GameStateProxy(_gameStateOrigin);
                
                
            }
            
            return Observable.Return(GameStateProxy);
        }

        public Observable<bool> SaveGameState()
        {
            // сохраняем
            DataSaver.saveData(_gameStateOrigin, SAVE_PATH);
            
            DLog.Alert("===== Game Saving =====", EDlogColor.GREEN, AppConstants.show_log_core);
            return Observable.Return(true);
        }

        public Observable<bool> ResetGameState()
        {
            GameStateProxy = CreateGameStateFromConfig();
            SaveGameState();        
            
            return Observable.Return(true);
        }
        
        
        /// <summary>
        /// Создание базового сотояния из конфига
        /// <br/>(монеты на старте, какие то постройки, предметы в инвентаре и пр)
        /// </summary>
        /// <returns></returns>
        GameStateProxy CreateGameStateFromConfig()
        {
            // === iap cards
            var iapCards = new List<IAPCardData>();
            var iapConfigs = _configsProvider.IAP._configs;
            var n = 0;
            foreach (var config in iapConfigs)
            {
                var cardData = WindowCardDataFactory.CreateCard(config.Value);
                cardData.Id = n;
                n++;
                iapCards.Add(cardData as IAPCardData);
            }
            
            
            // === resources stat
            var bankResources = new List<BankResourceData>();
            var l = Enum.GetNames(typeof(EBankResourceType)).Length;
            for (int i = 0; i < l; i++)
            {
                bankResources.Add(new BankResourceData()
                {
                    BankResourceType = (EBankResourceType)i,
                    Amount = 0
                });
            }

            // === game loop
            var gameLoopContext = new GameLoopContextData();
            gameLoopContext.CurrentLocationStateId = 0;
            gameLoopContext.PlayerOnMap = false;
            gameLoopContext.CurrentLocationNode = "home";
            gameLoopContext.CurrentDay = 1;
            gameLoopContext.RemainingHour = 24;
            gameLoopContext.ThreatData = null;
            gameLoopContext.LastRaidResult = new();
            gameLoopContext.LastRaidResult.LootReceived = new();
            gameLoopContext.LastRaidResult.ResourcesLost = new();

            // === player
            gameLoopContext.PlayerUnitData = new ();
            var playerStatsBase = _configsProvider.Get<PlayerStatsBase>();
            
            gameLoopContext.PlayerUnitData.Add(new PlayerData()
            {
                Id = "first_survivor",
                Name = "Jack Ranger",                                                                // FIX: name
                ArchetypeId = "survivor.1",
                Stats = DictionaryUtility.ToList(playerStatsBase.GetBaseStats()),
                
                Inventory = new (),
                Equipment = new ()
            });

            // transport
            gameLoopContext.PlayerTransport = new TransportData()
            {
                Id = "player_transport",
                ConfigId = GameIdProvider.Transport.Guid,
                
                Inventory = new(),
                Equipment = new()
            };
            
            gameLoopContext.SquadUnitId = new();
            gameLoopContext.BaseData = new();
            
            // === facility
            // *передаем список стартовых объектов
            var startFacilities =
                _configsProvider.Get<WorldStartConfig>().StartFacilities(_configsProvider);
            gameLoopContext.BaseData.Buildings = new(startFacilities.ToList());
            
            
            
            
            // === player OLD
            playerStatsBase = _configsProvider.Get<PlayerStatsBase>();
            var playerData = new List<PlayerData>(10);
            
            playerData.Add(new PlayerData()
            {
                
                Name = "New Player",                                                                // FIX: name
                Stats = DictionaryUtility.ToList(playerStatsBase.GetBaseStats()),
                
                Inventory = new List<InventorySlotData>(),
                Equipment = new List<InventorySlotData>()
            });
           
            for (int i = 1; i < 10; i++)
            {
                playerData.Add(new PlayerData()
                {
                    Name = "",
                    Stats = DictionaryUtility.ToList(playerStatsBase.GetBaseStats()),
                
                    Inventory = new List<InventorySlotData>(),
                    Equipment = new List<InventorySlotData>()
                });
            }
            // ====

            
            var server = new ServerTime
            {

            };
            var IAP = new CGameStateIAP
            {
                vip_pack_paid = false,
            };
            var AD = new CGameStateAD
            {
                ShowAutoAds = true,
                RemainingLimit = 10
            };
            var dailyReward = new CGameStateDailyReward
            {
                
            };

            var dailyQuest = new CGameStateDailyQuest
            {

            };

            var review = new CGameStateReview
            {

            };

            var tutorial = new CGameStateTutorial
            {

            };

            var inbox = new CPlayerInventory[0];
            
            
            _gameStateOrigin = new GameState
            {
                Server = server,
                IAP = IAP,
                IAPCardData = iapCards,
                ADState = AD,
                DailyReward = dailyReward,
                DailyQuest = dailyQuest,
                Review = review,
                Tutorial = tutorial,
                Inbox = inbox,
                
                GlobalEntityId = 0,
                BankResources = bankResources,
                
                
                GameLoopContext = gameLoopContext,
                // CurrentDay = 1,
                // RemainingHour = 24,
                // ThreatData = null,
                
                PlayerUnits = playerData,
                WorldsData = new List<WorldData>(),
                
            };
            
            _GameState.FirstStart_();
            return new GameStateProxy(_gameStateOrigin);
        }
        
        private void InitializeFirstSave()
        {
            // #1 внутриигровые покупки
            StateWriter.Write(GameStateProxy.IAP, (ref CGameStateIAP iap) => iap.double_hard = new());
            var iapConfigs = _configsProvider.IAP._configs;
            foreach (var cnf in iapConfigs)
            {
                if (cnf.Value is IAPConfig)
                    GameStateProxy.IAP.Value.double_hard.Add(false);
            }
            //
            
            // #2 первоначальный ad limit
            StateWriter.Write(GameStateProxy.ADState, (ref CGameStateAD ad) =>
            {
                ad.RemainingLimit = _configsProvider.Get<GameConfig>().Ad.dailyLimit;
            });
            
            
            //ServiceLocator.Current.Get<ConfigProvider>().Get<DailyQuestConfig>().NewSaveData(_gameStateOrigin);
            //ServiceLocator.Current.Get<ConfigProvider>().Get<DailyRewardConfig>().NewSaveData(_gameStateOrigin);
            //ServiceLocator.Current.Get<AccessController>().NewSaveData(_gameStateOrigin);
            
        }
    }
}