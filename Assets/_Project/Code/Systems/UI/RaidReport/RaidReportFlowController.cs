using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Core;
using Galactic1.Code.Core.Ads;
using Galactic1.Code.Game.Rewards;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Systems.Economy.Configs;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.UI.RaidReport.Drone;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.UI.CharacterPreview;
using Galactic1.UI.Core;
using Galactic1.UI.Core.TabPanel;
using Galactic1.UI.Text;
using UnityEngine;

namespace Galactic1.Code.UI.RaidReport
{
    public class RaidReportFlowController : MonoBehaviour, IGameService
    {
        private RaidReportController _summaryScreen;
        private GameLoopContext _gameLoopContext;
        private IInventorySource _transportSource;
        
        public RaidReportInventoryContext InventoryContext { get; private set; }

        private RaidBonusService _bonusService;
        private IAdRewardProvider _adProvider;
        private RaidReportData _data;
        private IConfigProvider _configProvider;
        private CharacterPortraitCache _portraitCache;

        private int maxDroneLimit;

        private event Action OnClosed;
        

        public void Initialize(
            GameLoopContext gameLoopContext,
            IInventorySource inventory,
            IAdRewardProvider adProvider,
            IConfigProvider configProvider)
        {
            _gameLoopContext = gameLoopContext;
            _transportSource = inventory;
            _adProvider = adProvider;
            _configProvider = configProvider;
            
            _portraitCache = ServiceLocator.Current.Get<CharacterPortraitCache>();

            maxDroneLimit = configProvider.Get<EconomyConfig>().CargoDroneMaxCharge;
        }

        public void StartFlow(
            string locationTitle,
            RaidRuntime raidRuntime,
            RaidResultProxy raidResult,
            Action onClosed)
        {
            OnClosed = onClosed;
            
            // очистка мертвых юнитов после закрытия панели
            OnClosed += ServiceLocator.Current.Get<GameSession>().GameLoopContext.CleanupDeadUnitsAfterRaid;
            
            _data = BuildReportData(locationTitle, raidRuntime, raidResult);

            // Создаём временные инвентари — живут до GoToMap()
            InventoryContext = new RaidReportInventoryContext(
                _transportSource,
                _configProvider);

            _bonusService = new RaidBonusService(InventoryContext.TransportPort);
            
            
            // Считаем потенциальный бонус
            // реклама будет активна если в транспорт все входит
            float mult = _adProvider.GetAdMultiplier(AdPlacement.PostRaid);
            var lootWithBonus = _bonusService.ApplyBonus(_data.Loot, mult);
            var eligibility = _bonusService.CheckEligibility(lootWithBonus.Item1);

            _data.LootEmpty = lootWithBonus.Item1.Count == 0;
            
            // #1 полученный лут с бонусом вмещается в транспорт
            if (eligibility.IsEligible && eligibility.AdBonusAvail)
            {
                _data.CargoAvail = true;
                _data.AdBonusAvail = true;
                _data.Loot = new(lootWithBonus.Item1);
                _data.BonusLootCount = lootWithBonus.onlyBonus;
            }
            // #2 только снаряга без бонуса вмещается в транспорт
            else if (eligibility.IsEligible)
            {
                _data.CargoAvail = true;
            }
            // #3 лута нет
            else if (!eligibility.IsEligible && eligibility.Reason == IneligibleReason.NoLoot)
            {
                _data.CargoAvail = true;
                _data.Loot = new();
            }
                

            ShowSummary();
        }


        // ─── Шаг 1: Summary ──────────────────────────────────────────────

        private void ShowSummary()
        {
            // ServiceLocator.Current.Get<TabPanelController>().EntryParam = new()
            // {
            //     HideTab = true
            // };
            ServiceLocator.Current.Get<UIManager>().OpenScreen(
                UIScreenId.RaidReport,
                null,
                _ =>
                {
                    if (_summaryScreen == null)
                        _summaryScreen = _.GetComponent<RaidReportController>();

                    _summaryScreen.Show(_data, OnSummaryNext);
                });
        }
        
        
        /// <summary>
        /// Первая кнопка Continue
        /// <br/>a: сразу закрывает панель
        /// <br/>b: лут не вошел, панель с дроном
        /// </summary>
        /// <param name="adWatched"></param>
        private void OnSummaryNext(bool adWatched)
        {
            if (adWatched)
            {
                _data.AdBonusApplied = true;
            }

            _summaryScreen.Hide();

            // === получаем готовые слоты для инвентаря
            var finalSlots = BuildInventorySlots(_data.Loot, adWatched);
            
            
            // Всё влезает — грузим напрямую в реальный транспорт
            if (_data.CargoAvail)
            {
                LoadLootToTransport(finalSlots);
                ReportFinish();
            }
            // Не влезает — показываем экран инвентаря с буфером
            else 
            {
                var l = finalSlots.Count;
                for (int i = 0; i < l; i++)
                    InventoryContext.LootBufferSource.SetSlot(i, finalSlots[i]);
                
                ShowInventory();
            }
        }

        // ─── Шаг 2: Inventory + Drone ────────────────────────────────────

        private void ShowInventory()
        {
            ServiceLocator.Current.Get<UIManager>().OpenScreen(
                UIScreenId.Inventory, 
                new TabPanelController.FlagInventory(),
                screen =>
                {
                    var window = screen.GetComponent<InventoryManagementWindow>();
                    
                    // Создаём состояние дрона из конфига
                    var droneState = DroneSessionState.FromConfig(
                        ServiceLocator.Current.Get<GameSession>().GameLoopContext.Proxy.RemainingDroneCharge.Value,
                        maxDroneLimit);

                    // Устанавливаем контекст ДО Open() — окно подхватит его внутри
                    window.SetDroneContext(new DroneOpenContext(
                        droneState,
                        OnDroneSent));

                    // Открываем окно с двумя временными источниками напрямую
                    // Левый  = реальный транспорт
                    // Правый = буфер лута
                    screen.GetComponent<InventoryManagementWindow>().modeController
                        .Open(InventoryGameplayMode.Transport_BufferLoot);

                    // Подписываемся на закрытие окна как на Continue
                    window.OnClosed += ReportFinish;
                });
        }

        private void ReportFinish()
        {
            // Всё что осталось в LootBufferSource — пропадает
            // Реальный транспорт уже изменён через drag-and-drop в InventoryManagementWindow
            DisposeInventoryContext();
            OnClosed?.Invoke();
            OnClosed = null;
            DLog.Alert("Raid report flow finished", EDlogColor.BLUE);
        }

        
        // Вызывается после каждой успешной отправки дрона
        private void OnDroneSent(List<InventorySlotRuntime> sent)
        {
            // todo
            // звук запуска дрона
            
            // Добавляем в CampInbox — аналог CollectCompleted в UniversalProductionSceneAdapter
            var inboxService = ServiceLocator.Current.Get<InboxService>();

            foreach (var slot in sent)
                inboxService.AddReward(slot);
        }

        // ─── Helpers ─────────────────────────────────────────────────────

        private void LoadLootToTransport(List<InventorySlotRuntime> loot)
        {
            if (loot == null || loot.Count == 0) 
                return;
            
            foreach (var item in loot)
            {
                InventoryContext.TransportPort.TryAdd(item);
            }
        }

        private static List<InventorySlotRuntime> BuildInventorySlots(List<RaidLootResult> loot, bool adWatched)
        {
            return loot.Select(l => new InventorySlotRuntime(
                l.Item,
                adWatched ? l.TotalAmount : l.Amount,
                l.Durability,
                l.AmmoInMagazine
            )).ToList();
        }

        // Контекст уничтожается — временные инвентари уходят вместе с ним
        private void DisposeInventoryContext() => InventoryContext = null;


        private RaidReportData BuildReportData(
            string title, 
            RaidRuntime raidRuntime, 
            RaidResultProxy result)
        {
            
            UIStyleResolver style = ServiceLocator.Current.Get<UIStyleResolver>();
            
            // // survivors
            // var survivors = new List<RaidSurvivorResult>();
            //
            // // === для отображения текущего хп у живых
            // var squadUnits = _gameLoopContext.StrategicSquadUnits;
            // foreach (var unit in squadUnits)
            // {
            //     if (!unit.Stats.IsDead)
            //     {
            //         var value01 = unit.Stats.CurrentHP / unit.Stats.MaxHP;
            //         var status = "HP " + TextBuilder.Start()
            //             .Color(style.ResolveValueColor(ValueRangeType.Health, value01))
            //             .Size(90)
            //             .Text(Mathf.FloorToInt(unit.Stats.CurrentHP).ToString())
            //             .End() // size
            //             .End() // color
            //             .Text("/")
            //             .Size(80)
            //             .Text(unit.Stats.MaxHP.ToString())
            //             .End();
            //         
            //         survivors.Add(new()
            //         {
            //             RenderPortrait = _portraitCache.GetPortrait(unit.ArchetypeId),
            //             Name = unit.DisplayName,
            //             Status = status
            //         });
            //     }
            //
            //     else
            //     {
            //         var status = TextBuilder.Start()
            //             .Color(Color.red)
            //             .Text("Dead")
            //             .End()
            //             .ToString();
            //     
            //         survivors.Add(new()
            //         {
            //             RenderPortrait = _portraitCache.GetPortrait(unit.ArchetypeId),
            //             Name = unit.DisplayName,
            //             Status = status
            //         });
            //     }
            // }
            
            // старый вариант, состояние берется из временного рантйам рейда,
            // назависимо от варианта выхода из локации (raid complete/debug exit)
            // Поменял на _gameLoopContext.StrategicSquadUnits что бы было удобней для дебага
//             var l = raidRuntime.Squad.Units.Count;
//             for (int i = 0; i < l; i++)
//             {
//                 var status = "";
//
//                 // === для живых
//                 if (!raidRuntime.Squad.Units[i].Stats.IsDead)
//                 {
//                     var value01 = raidRuntime.Squad.Units[i].Stats.CurrentHP / raidRuntime.Squad.Units[i].Stats.MaxHP;
//                     status = "HP " + TextBuilder.Start()
//                         .Color(style.ResolveValueColor(ValueRangeType.Health, value01))
//                         .Size(90)
//                         .Text(Mathf.FloorToInt(raidRuntime.Squad.Units[i].Stats.CurrentHP).ToString())
//                         .End() // size
//                         .End() // color
//                         .Text("/")
//                         .Size(80)
//                         .Text(raidRuntime.Squad.Units[i].Stats.MaxHP.ToString())
//                         .End();
//                 }    
//
//                 // === для мертвых
//                 else
//                 {
//                     status = TextBuilder.Start()
//                         .Color(Color.red)
//                         .Text("Dead")
//                         .End()
//                         .ToString();
//                 }
//                 
//                 
//                 survivors.Add(new()
//                 {
//                     RenderPortrait = _portraitCache.GetPortrait(raidRuntime.Squad.Units[i].ArchetypeId),
//                     Name = raidRuntime.Squad.Units[i].DisplayName,
//                     Status = status
//                 });
//
// #if UNITY_EDITOR
//                 DLog.Alert($"Raid report surv: #{i} {survivors[survivors.Count-1].Name} / {status}");
// #endif
//             }
            
            
            
            // loot
            var loot = new List<RaidLootResult>();
            var l = result.LootReceived.Count;
            for (int i = 0; i < l; i++)
            {
                var item = result.LootReceived[i];
                if (GameContent.ResolveItem(item.ConfigId.Value, out var config))
                {
                    loot.Add(new RaidLootResult
                    {
                        Item = config,
                        Amount = item.Amount.Value,
                        TotalAmount = item.Amount.Value,
                        Durability = item.Durability.Value,
                        AmmoInMagazine = item.AmmoInMagazine.Value
                    });
                }
            }

            return new RaidReportData
            {
                Survivors = RaidReportUtility.Survivors(_gameLoopContext.StrategicSquadUnits),
                LocationTitle = title,
                Loot = loot,
            };
        }
    }
}