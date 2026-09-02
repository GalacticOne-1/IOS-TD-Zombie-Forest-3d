using System.Linq;
using Galactic1.Code.Core;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.World.Threats;
using Galactic1.UI.Shop;
using Galactic1.Structs;
using Galactic1.Window;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Galactic1.Core
{
    public class GameStateProxy
    {
        private readonly GameState _gameState;
        
        
        public readonly ReactiveProperty<ServerTime> Server;
        public readonly ReactiveProperty<CGameStateIAP> IAP;
        public ObservableList<ShopCardProxy> IAPCardsProxy { get; } = new();
        
        public readonly ReactiveProperty<CGameStateAD> ADState;
        public readonly ReactiveProperty<CGameStateDailyReward> DailyReward;
        public readonly ReactiveProperty<CGameStateDailyQuest> DailyQuest;
        public readonly ReactiveProperty<CGameStateReview> Review;
        public readonly ReactiveProperty<CGameStateTutorial> Tutorial;
        
        // ********************         DEFAULT        ************************************************************
        
       
        public ReactiveProperty<CPlayerInventory>[] Inbox;
        
        
        // ********************         DEFAULT        ************************************************************
        public readonly GameLoopContextProxy GameLoopContext;
        
        
        
        
        public ObservableList<BankResourceProxy> BankResources { get; } = new();

        public ObservableList<PlayerProxy> PlayerUnits { get; } = new();                            // <- REMOVE
        public ObservableList<WorldDataProxy> WorldsState { get; } = new();
        

        
        /// <summary>
        /// Инициализации R3 для сохранения
        /// </summary>
        /// <param name="gameState">состояние игры загруженное из диска/облака и пр</param>
        public GameStateProxy(GameState gameState)
        {
            /*
             *      Подписываем оригинальное состояние GameState на это прокси
             *      таким образом GameState будет синхронизировано с GameStateProxy
             *      т.е любое изменение в этом классе, также меняет и GameState
             *      который в свою очередь ипользуется в сервисе сохранения
             */
            
            _gameState = gameState;

            Server = new(_gameState.Server);
            Server.Skip(1).Subscribe(_ => gameState.Server = _);
            
            IAP = new(gameState.IAP);
            IAP.Skip(1).Subscribe(_ => gameState.IAP = _);
            
            ADState = new(gameState.ADState);
            ADState.Skip(1).Subscribe(_ => gameState.ADState = _);
            
            Review = new(gameState.Review);
            Review.Skip(1).Subscribe(_ => gameState.Review = _);
            
            DailyReward = new(gameState.DailyReward);
            DailyQuest = new(gameState.DailyQuest);
            
            Tutorial = new(gameState.Tutorial);
            
            
           
            Inbox = new ReactiveProperty<CPlayerInventory>[gameState.Inbox.Length];
            
            
            GameLoopContext = new(gameState.GameLoopContext);
            
            
            InitializeBankResources(_gameState);
            InitializeIAPCards(_gameState);
            InitializeLocationsState(_gameState);
            InitializePlayerUnits(_gameState);
            
        }



        public int CreateEntityId() => _gameState.CreateEntityId();


        
        
        
        
        
        
        
        
        void InitializeIAPCards(GameState gameState)
        {
            // связываем с масссивом в прокси для синхроницации
            gameState.IAPCardData.ForEach(cardData => IAPCardsProxy.Add(WindowCardProxyFactory.CreateCard(cardData) as ShopCardProxy));
           
            // для добавления
            IAPCardsProxy.ObserveAdd().Subscribe(e =>
            {
                var addedCard = e.Value;
                gameState.IAPCardData.Add(addedCard.Origin as IAPCardData);
            });
          
            // для удаления
            IAPCardsProxy.ObserveRemove().Subscribe(e =>
            {
                var removedCard = e.Value;
                var l = gameState.IAPCardData.Count;
                for (int i = 0; i < l; i++)
                {
                    if(gameState.IAPCardData[i].Id == removedCard.Id)
                        gameState.IAPCardData.RemoveAt(i);
                }
            });
        }
        
        
        void InitializeLocationsState(GameState gameState)
        {
            gameState.WorldsData.ForEach(worldOrigin => WorldsState.Add(new WorldDataProxy(worldOrigin)));
            
            // при добавлении связываем с сохранением
            WorldsState.ObserveAdd().Subscribe(e =>
            {
                var newWorld = e.Value;
                gameState.WorldsData.Add(newWorld.Origin);
            });
            
            // так же при удалении удаляем сохранение
            WorldsState.ObserveRemove().Subscribe(e =>
            {
                var removeWorld = e.Value;
                var removedWorldState = gameState.WorldsData.FirstOrDefault(r => r.Id == removeWorld.Id);
                gameState.WorldsData.Remove(removedWorldState);
            });
        }

        void InitializeBankResources(GameState gameState)
        {
            gameState.BankResources.ForEach(resourceData => BankResources.Add(new BankResourceProxy(resourceData)));
            
            // при добавлении связываем с сохранением
            BankResources.ObserveAdd().Subscribe(e =>
            {
                var newResource = e.Value;
                gameState.BankResources.Add(newResource.Origin);
            });
            
            // так же при удалении удаляем сохранение
            BankResources.ObserveRemove().Subscribe(e =>
            {
                var removedResource = gameState.BankResources.FirstOrDefault(r => r.BankResourceType == e.Value.BankResourceType);
                gameState.BankResources.Remove(removedResource);
            });
        }
        
        
        void InitializePlayerUnits(GameState gameState)
        {
            gameState.PlayerUnits.ForEach(playerEntity => PlayerUnits.Add(new PlayerProxy(playerEntity)));
            
            // при добавлении связываем с сохранением
            PlayerUnits.ObserveAdd().Subscribe(e =>
            {
                gameState.PlayerUnits.Add(e.Value.Origin);
            });
            
            // так же при удалении удаляем сохранение
            PlayerUnits.ObserveRemove().Subscribe(e =>
            {
                gameState.PlayerUnits.Remove(gameState.PlayerUnits.FirstOrDefault(r => r.Id == e.Value.Id));
            });
            
        }
    }
}