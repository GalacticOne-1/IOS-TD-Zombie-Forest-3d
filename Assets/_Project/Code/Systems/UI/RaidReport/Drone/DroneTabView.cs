
using System;
using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Notification;
using Galactic1.Code.Systems.Ads;
using Galactic1.Code.Systems.Economy;
using Galactic1.Configs;
using Galactic1.Core.Results;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;

namespace Galactic1.Code.UI.RaidReport.Drone
{
    public class DroneTabView : MonoBehaviour
    {
        [Header("Charge indicators")] 
        [SerializeField]private GameObject chargeIndicators;
        private Sprite chargeActiveSprite;
        private Sprite chargeEmptySprite;

        [SerializeField] private TMP_Text chargeLabel;
        [SerializeField] private TMP_Text resetHintText;


        [Header("Send button")] 
        [SerializeField] private GameObject sendAdButton;
        [SerializeField] private GameObject sendPremiumButton;


        // ─── State ───────────────────────────────────────────────────────

        private DroneSessionState _state;
        private IInventorySource _droneBuffer; // SnapshotInventorySource (WorldMapDrone)
        private IEconomyService _economy;
        

        private Action<List<InventorySlotRuntime>> _onSent; // коллбэк → FlowController
        
        bool initialized = false;

        // ─── Init ────────────────────────────────────────────────────────

        public void Initialize(IEconomyService economy, UIStyleResolver style)
        {
            _economy = economy;
            
            chargeActiveSprite = style.RaidReportIconConfig.ChargeActiveSprite;
            chargeEmptySprite = style.RaidReportIconConfig.ChargeEmptySprite;

            sendAdButton.RegisterButtonClick(OnSendAd, () => !ValidateSendPreconditions());
            sendPremiumButton.RegisterButtonClick(OnSendPremium);

            initialized = false;
        }

        void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                
            }
        }

        // ─── Public API ──────────────────────────────────────────────────

        /// <summary>
        /// Открывает вкладку дрона.
        /// droneBuffer  — временный WorldMapDrone инвентарь (3 слота)
        /// lootBuffer   — правый буфер лута для перетаскивания
        /// onSent       — вызывается после успешной отправки
        /// </summary>
        public void Show(
            DroneSessionState state,
            IInventorySource droneBuffer,
            Action<List<InventorySlotRuntime>> onSent)
        {
            _state = state;
            _droneBuffer = droneBuffer;
            _onSent = onSent;
            Initialize();

            gameObject.SetActive(true);
            Render();
        }

        public void Hide() => gameObject.SetActive(false);

        // ─── Render ──────────────────────────────────────────────────────

        private void Render()
        {
            RenderCharges();
            RenderPanels();
            RenderSendButtons();
        }

        private void RenderCharges()
        {
            var l = chargeIndicators.transform.childCount;
            for (int i = 0; i < l; i++)
                chargeIndicators.GetChild(i).CMP_Image().sprite =
                    i < _state.ChargesLeft ? chargeActiveSprite : chargeEmptySprite;

            //chargeLabel.text = $"{_state.ChargesLeft}/{_state.ChargesMax}";

            // resetHintText.text = _state.IsExhausted
            //     ? "Дрон вернётся когда вы доберётесь до лагеря"
            //     : "Лимит восстановится в лагере";
        }

        private void RenderPanels()
        {
            bool hasCharges = _state.HasCharges;

        }




        private void RenderSendButtons()
        {
            int selectedCount = CountSelectedInDrone();
            bool hasSelected = selectedCount > 0;
            bool hasCharges = _state.HasCharges;

            // selectedCountLabel.text =
            //     $"Выбрано: {selectedCount} из {_state.SlotsPerCharge}";

            // Кнопки отправки активны только если есть что отправлять и есть заряды
            bool canSend = hasSelected && hasCharges;

            //sendAdButton.ButtonSetInteractable(canSend);

            int sendCost = _economy.CalculateDroneSendCost();
            sendPremiumButton.ButtonSetText($"{sendCost}");
            //sendPremiumButton.ButtonSetInteractable(canSend);
        }

        // ─── Send logic ──────────────────────────────────────────────────

        private void OnSendAd()
        {
            // Проверяем лимит перед показом рекламы
            if (!ValidateSendPreconditions()) return;

            if (ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().requiresAdService)
            {
                ServiceLocator.Current.Get<AdService>().OnGrantRewardEvent((placement) =>
                {
                    ExecuteSend();
                });
            }
        }

        private void OnSendPremium()
        {
            if (!ValidateSendPreconditions()) return;

            int cost = _economy.CalculateDroneSendCost();
            if (!_economy.TrySpend(EBankResourceType.CurrencyPremium, cost)) return;

            ExecuteSend();
        }

        private bool ValidateSendPreconditions()
        {
            // Лимит исчерпан 
            if (_state.IsExhausted)
            {
                ServiceLocator.Current.Get<INotificationService>().Push(NotificationFailReason.CargoDroneNotCharge);
                return false;
            }

            // если слоты пустые
            if (CountSelectedInDrone() == 0)
            {
                ServiceLocator.Current.Get<INotificationService>().Push(NotificationFailReason.CargoDroneEmptySlots);
                return false;
            }

            return true;
        }

        private void ExecuteSend()
        {
            // Собираем предметы из droneBuffer
            var sent = new List<InventorySlotRuntime>();
            var slots = _droneBuffer.GetSlots();

            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty)
                    sent.Add(slots[i]);
            }

            if (sent.Count == 0) return;

            // Списываем заряд
            _state.ChargesLeft--;
            ServiceLocator.Current.Get<GameSession>().GameLoopContext.Proxy.RemainingDroneCharge.Value--;

            // Очищаем drone buffer
            for (int i = 0; i < slots.Count; i++)
                _droneBuffer.ClearSlot(i);

            // Уведомляем FlowController — он добавит в CampInbox
            _onSent?.Invoke(sent);

            // Перерисовываем
            Render();
        }



        // ─── Helpers ────────────────────────────────────────────────────

        private int CountSelectedInDrone()
        {
            int count = 0;
            var slots = _droneBuffer.GetSlots();
            foreach (var slot in slots)
                if (!slot.IsEmpty)
                    count++;
            return count;
        }
    }
}