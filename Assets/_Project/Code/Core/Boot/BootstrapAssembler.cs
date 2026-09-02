using Galactic1;

namespace Galactic1
{
    public static class BootstrapAssembler
    {

        /*
         *      Управление подписками для структуры игры
         */


        // структора
        private static EventBinding<StartLevelEvent> startLevelEventBinding;
        private static EventBinding<FinishLevelEvent> finishLevelEventBinding;
        private static EventBinding<ExitLevelEvent> exitLevelEventBinding;
        private static EventBinding<LoadLevelEvent> loadLevelEventBinding;
        private static EventBinding<ClearLevelEvent> clearLevelEventBinding;
        private static EventBinding<IsFinishBattleEvent> _eventBindingIsFinishBattle;
        private static EventBinding<IsFinishRaidEvent> _eventBindingIsFinishRaid;

        
        // события в игре
        private static EventBinding<ReachNewPlayerLevelEvent> reachNewPlayerLevelEventBinding;



        

        public static void Subscription()
        {
            // включить что бы ачивки работали
            //ServiceLocator.Current.Get<LibController>().dailyQuest.SubscribtionAchievement();
            
            
            ScreenRegular();
            ScreenMap();
            
            StartLevel();
            ClearGame();
            LoadLevel();
            
            FinishLevel();
            ExitLevel();
            ClearLevel();
            FinishBattle();
            FinishRaid();
            
            NewPlayerLevel();
        }
        
        
        /// <summary>
        /// Загрузка виджетов для базы
        /// </summary>
        public static void ScreenRegular()
        {
            // * LOAD
            EventBinding<ScreenLoadRegularEvent> load;
            //load = new EventBinding<ScreenLoadRegularEvent>(ServiceLocator.Current.Get<ViewGameController>().ConstructViewModel.LoadContent);
            //EventBus<ScreenLoadRegularEvent>.Register(load);

            // *** кнопка для запуска бытвы в лагере
            //load = new EventBinding<ScreenLoadRegularEvent>(new UNIT_CAMP_STATE().Check);
            //EventBus<ScreenLoadRegularEvent>.Register(load);
            
            
            // * CLEAR
            EventBinding<ScreenClearRegularEvent> clear;
            //clear = new EventBinding<ScreenClearRegularEvent>(ServiceLocator.Current.Get<ViewGameController>().ConstructViewModel.ClearContent);
            //EventBus<ScreenClearRegularEvent>.Register(clear);
            
            
            
            
            
            // ***       Контроллер предложения по монетизационным блокам и событиям на базе       ***
            // запускает очередь виджетов
            // (можно вывести на отдельное событие которое будет запускатся один раз при старте приложения)
            
            // DAILY BONUS
            load = new EventBinding<ScreenLoadRegularEvent>(() => new GetDeal_DailyBonus());
            EventBus<ScreenLoadRegularEvent>.Register(load);
            
            
            
            // ***   должен замыкать!!
            //load = new EventBinding<ScreenLoadRegularEvent>(ServiceLocator.Current.Get<ContentQueueController>().LaunchQueueDelay);
            //EventBus<ScreenLoadRegularEvent>.Register(load);
        }
        
        /// <summary>
        /// Загрузка виджетов для карты
        /// </summary>
        public static void ScreenMap()
        {
            // * LOAD
            EventBinding<ScreenLoadMapEvent> load;
            //load =  new EventBinding<ScreenLoadMapEvent>(ServiceLocator.Current.Get<ViewGameController>().MapViewModel.LoadContent);
            //EventBus<ScreenLoadMapEvent>.Register(load);
            
            
            // * CLEAR
            EventBinding<ScreenClearMapEvent> clear;
            //clear =  new EventBinding<ScreenClearMapEvent>(ServiceLocator.Current.Get<ViewGameController>().MapViewModel.ClearContent);
            //EventBus<ScreenClearMapEvent>.Register(clear);
            
            
            
            
            
            // ***       Контроллер предложения по монетизационным блокам и событиям на карте       ***
            // запускает очередь виджетов при открытии карты
            
            // DAILY BONUS
            load = new EventBinding<ScreenLoadMapEvent>(() => new GetDeal_DailyBonus());
            EventBus<ScreenLoadMapEvent>.Register(load);
            
            // AD Equipment Box
            load = new EventBinding<ScreenLoadMapEvent>(() => new GetDeal_ADEquipmentBox());
            EventBus<ScreenLoadMapEvent>.Register(load);
            
            
            // ***   должен замыкать!!
            //load = new EventBinding<ScreenLoadMapEvent>(ServiceLocator.Current.Get<ContentQueueController>().LaunchQueueDelay);
            //EventBus<ScreenLoadMapEvent>.Register(load);
            
        }


        
        
        

        

        // TO LEVEL #1   (кнопка ToBattle)
        static void StartLevel()
        {
            startLevelEventBinding = new EventBinding<StartLevelEvent>(
                () =>
                {
                    DLog.Alert($">>>> Start Level : {ServiceLocator.Current.Get<GameMachine>().MODE}");
                    switch (ServiceLocator.Current.Get<GameMachine>().MODE)
                    {
                        case GameMachine.EMode.REGULAR:
                            ServiceLocator.Current.Get<GameMachine>().Level_Start();
                            break;
                        
                        case GameMachine.EMode.RAID:
                            //ServiceLocator.Current.Get<GameMachine>().Event_Start();
                            break;
                    }
                });
            EventBus<StartLevelEvent>.Register(startLevelEventBinding);
            
        }
        
        // #1.1     при переходе на уровень удаляем подписки для очищаемых виджетов
        static void ClearGame()
        {
            // clearGameEventBinding = new EventBinding<ClearGameEvent>(EventBus<StateSoftCurrencyTempEvent>.Clear);
            // EventBus<ClearGameEvent>.Register(clearGameEventBinding);
            
            // clearGameEventBinding = new EventBinding<ClearGameEvent>(ServiceLocator.Current.Get<ViewGameController>().LocationViewModel.ClearContent);
            // EventBus<ClearGameEvent>.Register(clearGameEventBinding);

            

        }

        // #2
        static void LoadLevel()
        {
            /*
             *      Все что должно запускатся на уровне
             */
            
            // *** 1 сначала сбрасываем дату
            loadLevelEventBinding = new EventBinding<LoadLevelEvent>(() => new StartLevelData());
            EventBus<LoadLevelEvent>.Register(loadLevelEventBinding);
            // ---------------- ^^^ DEFAULT ^^^
            
            
            // * 2 потом запускаем системы
            
            // создаем локацию, запускаем существ
            loadLevelEventBinding = new EventBinding<LoadLevelEvent>(() => new LevelSetting_Regular());
            EventBus<LoadLevelEvent>.Register(loadLevelEventBinding);
            
            
            // popup
            //loadLevelEventBinding = new EventBinding<LoadLevelEvent>(ServiceLocator.Current.Get<PopupController>().Activate);
            //EventBus<LoadLevelEvent>.Register(loadLevelEventBinding);
        }

        
        
        
        // FROM LEVEL #1
        static void FinishLevel()
        {
            finishLevelEventBinding = new EventBinding<FinishLevelEvent>(() =>
            {
                switch (ServiceLocator.Current.Get<GameMachine>().MODE)
                {
                    case GameMachine.EMode.REGULAR:
                        ServiceLocator.Current.Get<GameMachine>().Level_Finish();
                        break;
                        
                    case GameMachine.EMode.RAID:
                        
                        break;
                }
            });
            EventBus<FinishLevelEvent>.Register(finishLevelEventBinding);
            
            // останавливаем все на уровне
            finishLevelEventBinding = new EventBinding<FinishLevelEvent>(() => new LevelStop_Regular());
            EventBus<FinishLevelEvent>.Register(finishLevelEventBinding);
        }

        // #2   (кнопка OK на панели результата deefeat/victory)
        static void ExitLevel()
        {
            exitLevelEventBinding = new EventBinding<ExitLevelEvent>(() =>
            {
                switch (ServiceLocator.Current.Get<GameMachine>().MODE)
                {
                    case GameMachine.EMode.REGULAR:
                        ServiceLocator.Current.Get<GameMachine>().Level_Exit();
                        break;
                        
                    case GameMachine.EMode.RAID:
                        //ServiceLocator.Current.Get<GameMachine>().Event_Exit();
                        break;
                }
            });
            EventBus<ExitLevelEvent>.Register(exitLevelEventBinding);
            
        }

        // #2.1
        static void ClearLevel()
        {
            // #1 clear level
            clearLevelEventBinding = new EventBinding<ClearLevelEvent>(() =>
            {
                switch (ServiceLocator.Current.Get<GameMachine>().MODE)
                {
                    case GameMachine.EMode.REGULAR:
                        new LevelClear_Regular();
                        break;
                        
                    case GameMachine.EMode.RAID:
                        //new EVENT_RAID().LevelClear();
                        break;
                }
            });
            EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);
            
            clearLevelEventBinding = new EventBinding<ClearLevelEvent>(EventBus<StateSilverCurrencyTempEvent>.Clear);
            EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);
            
            // * отображение статы
            /*clearLevelEventBinding = new EventBinding<ClearLevelEvent>(_ => 
                ServiceLocator.Current.Get<ViewGameController>().GetStats(new[] { EStat.PLAYER_RANK, EStat.SOFT, EStat.HARD, }));
            EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);
            
            // popup
            clearLevelEventBinding = new EventBinding<ClearLevelEvent>(ServiceLocator.Current.Get<PopupController>().Clear);
            EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);
            
            
            // #2 load game (lobby)
            clearLevelEventBinding = new EventBinding<ClearLevelEvent>(ServiceLocator.Current.Get<ViewGameController>().FinishLevelPresenter.HideScreen);
            EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);
            clearLevelEventBinding = new EventBinding<ClearLevelEvent>(ServiceLocator.Current.Get<ViewGameController>().MainMenuViewModel.ResetState);
            EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);*/
            
            //clearLevelEventBinding = new EventBinding<ClearLevelEvent>(() => new CheckNewLocation());
            //EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);
            /*clearLevelEventBinding = new EventBinding<ClearLevelEvent>(ServiceLocator.Current.Get<ViewGameController>().LocationViewModel.LoadContent);
            EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);
            
            clearLevelEventBinding = new EventBinding<ClearLevelEvent>(ServiceLocator.Current.Get<ViewGameController>().UnitMngmViewModel.LoadContent);
            EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);
            
            clearLevelEventBinding = new EventBinding<ClearLevelEvent>(ServiceLocator.Current.Get<ViewGameController>().EnrichmentViewModel.LoadContent);
            EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);
            
            // проверка разблокировки при изменении stage
            clearLevelEventBinding = new EventBinding<ClearLevelEvent>(ServiceLocator.Current.Get<LibController>().adventureMap.CheckProgress);
            EventBus<ClearLevelEvent>.Register(clearLevelEventBinding);*/
            
        }

        
        // * FOR REGULAR LEVEL
        static void FinishBattle()
        {
            // #1 finish panel
            _eventBindingIsFinishBattle = new EventBinding<IsFinishBattleEvent>(() => new FinishLevel().Check());
            EventBus<IsFinishBattleEvent>.Register(_eventBindingIsFinishBattle);
            
            // #2 player rank
            _eventBindingIsFinishBattle = new EventBinding<IsFinishBattleEvent>(() => new NewPlayerLevel().Check());
            EventBus<IsFinishBattleEvent>.Register(_eventBindingIsFinishBattle);
            
            // #3 review
            //_eventBindingIsFinishBattle = new EventBinding<IsFinishBattleEvent>(() => Review.I.RequestReview());
            //EventBus<IsFinishBattleEvent>.Register(_eventBindingIsFinishBattle);
            
            // #4 event rider cutscene
            //_eventBindingIsFinishBattle = new EventBinding<IsFinishBattleEvent>(new EVENT_CUTSCENE().Check);
            //EventBus<IsFinishBattleEvent>.Register(_eventBindingIsFinishBattle);
            
            
            // ***   должен замыкать!!
            //_eventBindingIsFinishBattle = new EventBinding<IsFinishBattleEvent>(ServiceLocator.Current.Get<ContentQueueController>().LaunchQueueDelay);
            //EventBus<IsFinishBattleEvent>.Register(_eventBindingIsFinishBattle);
        }

        // * FOR RAID LOCATION
        static void FinishRaid()
        {
            // #1 finish panel
            _eventBindingIsFinishRaid = new EventBinding<IsFinishRaidEvent>(() => new FinishRaid().Check());
            EventBus<IsFinishRaidEvent>.Register(_eventBindingIsFinishRaid);
            
            // #2 player rank
            _eventBindingIsFinishRaid = new EventBinding<IsFinishRaidEvent>(() => new NewPlayerLevel().Check());
            EventBus<IsFinishRaidEvent>.Register(_eventBindingIsFinishRaid);
            
            
            
            // ***   должен замыкать!!
            //_eventBindingIsFinishRaid = new EventBinding<IsFinishRaidEvent>(ServiceLocator.Current.Get<ContentQueueController>().LaunchQueueDelay);
            //EventBus<IsFinishRaidEvent>.Register(_eventBindingIsFinishRaid);
        }


        static void NewPlayerLevel()
        {
            
        }

       


        
        
        
        
    }
}