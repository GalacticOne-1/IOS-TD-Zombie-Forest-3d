
using System.Collections;
using Galactic1.Core;
using UnityEngine;
using Galactic1.Core.GameSession;
using Galactic1.Core.Systems;
using Galactic1.Gameplay.Player;
using Game.UI.Death;

namespace Galactic1.Gameplay.Death
{
    /// <summary>
    /// DeathSystem — центральная система смерти в игровом мире.
    /// Она:
    ///  - подписывается на событие смерти игрока (PlayerStatsController.OnDeath)
    ///  - создаёт корпус (CorpseContainer) и переливает в него лут
    ///  - управляет временем жизни корпуса
    ///  - вызывает RespawnService для респавна игрока
    /// 
    /// Рекомендуется регистрировать DeathSystem как IGameService / компонент в Core scene,
    /// либо добавлять экземпляр в SceneSession при загрузке локации.
    /// </summary>
    public class DeathSystem : MonoBehaviour, IGameService
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject corpsePrefab; // труп игрока

        [Header("Behaviour")]
        [SerializeField] private bool respawnImmediate = false;
        [SerializeField] private float respawnDelaySeconds = 1.0f;

        
        
        private RespawnService _respawnService;

        private DIContainer _container;
        private SceneSessionDefinition _session;
        

        
        
        
        /// <summary>
        /// Для сброса после смерти, если игрок не вызывал респавн
        /// </summary>
        public void InitializePlayerState(DIContainer container)
        {
            _container = container;
            _respawnService = new RespawnService(_container);
            
            if (ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnits[0].IsDead.Value)
            {
                _container.Resolve<LocationTransitionService>().SetLocation(0);
                _respawnService.ClearPlayerInventory();
                _respawnService.ClearPlayerStats();
            }
        }

        /// <summary>
        /// Инициализация DeathSystem — вызывать из SceneSession после того, как создан SceneSessionDefinition и спавнен игрок
        /// </summary>
        public void Initialize(SceneSessionDefinition session)
        {
            _session = session;

            // подписываемся на on-death у PlayerStats
            //_session.Player.OnDeath += OnPlayerDeathInternal;

            
            // подписываемся для респавна
            var deathScreenUI = ServiceLocator.Current.Get<DeathScreenUI>();
            
            // обычный респавн с потерей лута
            deathScreenUI.OnRespawnWithoutLoot +=
                () =>
                {
                    _respawnService.ClearPlayerInventory();
                    _respawnService.RespawnPlayerImmediate(_session);
                };
            
            // полноценный респавн за рекламу
            deathScreenUI.OnRespawnWithLootAd +=
                () => _respawnService.RespawnPlayerImmediate(_session);
        }


        


        private void OnPlayerDeathInternal()
        {
            // var player = _session.Player;
            // if (player == null) return;
            //
            // var deathPos = player.transform.position;
            //
            // Debug.Log("[DeathSystem] Player died at " + deathPos);
            //
            // _container.Resolve<IGameStateProvider>().GameStateProxy.PlayerUnitData[0].IsDead.Value = true;
            //
            // // 1) Сформировать корпус
            // //CreateCorpseFromPlayer(deathPos);
            //
            // // 2) Уведомления
            // DeathEvents.RaisePlayerDied(deathPos);

            // 3) Очистка/отключение игрока (взят из вашего pipeline)
            //player.Entity_Deactivate(false); // предполагается метод: деактивирует управление/физику

            // 4) Респавн
            // if (respawnImmediate)
            // {
            //     // краткая задержка чтобы дать UI проиграть анимацию
            //     StartCoroutine(RespawnCoroutine());
            // }
            // else
            // {
            //     // можно показать экран "You died" с кнопкой и затем вызвать RespawnService
            //     //StartCoroutine(RespawnCoroutine());
            // }

            _container.Resolve<IGameStateProvider>().SaveGameState();
        }

        private void CreateCorpseFromPlayer(Vector3 position)
        {
            if (corpsePrefab == null)
            {
                Debug.LogError("[DeathSystem] corpsePrefab is not assigned!");
                return;
            }

            // Instantiate corpse prefab at position
            var go = Instantiate(corpsePrefab, position, Quaternion.identity);
            var corpse = go.GetComponent<CorpseContainer>();
            if (corpse == null)
            {
                Debug.LogError("[DeathSystem] corpsePrefab has no CorpseContainer component.");
                Destroy(go);
                return;
            }

            // Fill corpse with player's items using player's inventory API
            //var inventory = player.Inventory;
            // if (inventory != null)
            // {
            //     // Если у тебя нет метода ExtractAllAsStacks(), напиши временно:
            //     // var stacks = inventory.GetAllAsStacks();
            //     // inventory.Clear();
            //     // corpse.Initialize(new CorpseData
            //     // {
            //     //     Position = position,
            //     //     Items = inventory.ExtractAllAsStacks(), // <- адаптируй под свой API
            //     //     IsPersistent = (_session.LocationType == LocationType.Home) // если в базе — не удалять
            //     // });
            //
            //     // Очистить инвентарь игрока после создания корпуса
            //     inventory.Clear();
            // }
            // else
            // {
            //     corpse.Initialize(new CorpseData { Position = position });
            // }

            // Подписаться на события корпуса (например, при истечении или взятии лута)
            corpse.OnExpired += OnCorpseExpired;
            corpse.OnLooted += OnCorpseLooted;
        }


        private void OnCorpseExpired(CorpseContainer corpse)
        {
            // лут улетел в небытие — можно логгировать/вызывать другие события
            Debug.Log("[DeathSystem] Corpse expired: " + corpse.name);
        }

        private void OnCorpseLooted(CorpseContainer corpse)
        {
            // игрок собрал лут, можно логгировать
            Debug.Log("[DeathSystem] Corpse looted: " + corpse.name);
        }
    }
}
