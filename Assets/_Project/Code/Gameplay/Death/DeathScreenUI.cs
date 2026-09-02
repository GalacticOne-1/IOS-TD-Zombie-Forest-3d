using Galactic1;
using Galactic1.Gameplay.Death;
using Galactic1.Mobile;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;

namespace Game.UI.Death
{
    /// <summary>
    /// UI-экран смерти, показывающий игроку два варианта респавна:
    /// 1) обычный респавн без сохранения лута,
    /// 2) респавн с сохранением лута через просмотр рекламы.
    /// Вызывает события, которые обрабатываются DeathSystem.
    /// </summary>
    public class DeathScreenUI : UIScreenPanel, IGameService
    {
        [Header("UI Elements")]
        
        [SerializeField] private TMP_Text tAlive;
        [SerializeField] private GameObject respawnWithoutLootButton;
        [SerializeField] private GameObject respawnWithLootAdButton;
        

        /// <summary>
        /// Пользователь нажал "Респавн без лута".
        /// </summary>
        public event System.Action OnRespawnWithoutLoot;

        /// <summary>
        /// Пользователь нажал "Респавн с рекламой".
        /// </summary>
        public event System.Action OnRespawnWithLootAd;

        /// <summary>
        /// Закрытие экрана (например, кнопка X).
        /// Обычно не используется в LDoE, но удобно иметь.
        /// </summary>
        public event System.Action OnClose;

        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);

            ServiceLocator.Current.Register(this);
            DeathEvents.OnPlayerDied += Subscription;
            
            // #1 респавн без лута
            respawnWithoutLootButton.RegisterButtonClick(HandleRespawnWithoutLoot);
            
            // #2 полное восстановление
            // respawnWithLootAdButton.RegisterButtonClick(() =>
            //     new AD_Request().Rewarded(
            //         AnalyticsService.ERequestAd.Revive,
            //         HandleRespawnWithLootAd));

            HideImmediate();
        }

        public override void Remove()
        {
            base.Remove();
            ServiceLocator.Current.Unregister<DeathScreenUI>();
            DeathEvents.OnPlayerDied -= Subscription;
        }

        /// <summary>
        /// Показать UI-экран смерти.
        /// </summary>
        void Subscription(Vector3 position)
        {
            DLog.Alert("DeathScreenUI");
            ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.DeathScreen);
        }


        /// <summary>
        /// Скрыть UI-экран.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Скрыть UI мгновенно (без анимаций).
        /// </summary>
        public void HideImmediate()
        {
            gameObject.SetActive(false);
        }

        private void HandleRespawnWithoutLoot()
        {
            OnRespawnWithoutLoot?.Invoke();
            Hide();
        }

        private void HandleRespawnWithLootAd()
        {
            OnRespawnWithLootAd?.Invoke();
            Hide();
        }
    }
}
